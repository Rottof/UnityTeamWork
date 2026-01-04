using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 小型怪类型配置
    /// </summary>
    [System.Serializable]
    public class SmallEnemyType
    {
        [Tooltip("怪物预制体")]
        public GameObject prefab;
        
        [Tooltip("生成概率权重（例如：70表示70%）")]
        [Range(0, 100)]
        public float spawnWeight = 50f;
        
        [Tooltip("类型名称（用于调试）")]
        public string typeName = "小型怪";
    }

    /// <summary>
    /// 怪物波次刷新管理器
    /// 负责管理小型怪和大型怪的定时刷新，实现难度递增机制
    /// </summary>
    public class WaveSpawnManager : MonoBehaviour
    {
        [Header("基础设置")]
        [Tooltip("玩家Transform引用")]
        public Transform player;

        [Tooltip("波次刷新间隔（秒）")]
        public float waveInterval = 30f;

        [Header("小型怪设置")]
        [Tooltip("小型怪类型列表（支持多种类型和概率）")]
        public List<SmallEnemyType> smallEnemyTypes = new List<SmallEnemyType>();
        
        [Tooltip("【已弃用】单一小型怪预制体（保留兼容性，建议使用上方列表）")]
        public GameObject smallEnemyPrefab;

        [Tooltip("第一波小型怪数量上限")]
        public int initialSmallEnemyLimit = 5;

        [Tooltip("每波增加的数量上限")]
        public int limitIncreasePerWave = 2;

        [Tooltip("小型怪刷新半径（玩家周围）")]
        public float spawnRadius = 15f;

        [Tooltip("刷新位置NavMesh采样范围")]
        public float navMeshSampleDistance = 5f;

        [Header("大型怪设置")]
        [Tooltip("大型怪预制体")]
        public GameObject largeEnemyPrefab;

        [Tooltip("大型怪刷新点")]
        public List<LargeEnemySpawnPoint> largeEnemySpawnPoints = new List<LargeEnemySpawnPoint>();

        [Header("难度设置")]
        [Tooltip("每波血量增长百分比（0.1 = 10%）")]
        public float healthIncreasePerWave = 0.1f;

        [Tooltip("每波攻击力增长百分比（0.1 = 10%）")]
        public float damageIncreasePerWave = 0.1f;

        [Header("Debug")]
        [Tooltip("显示刷新范围")]
        public bool showSpawnRadius = true;

        // 私有变量
        private int currentWave = 0;
        private int currentSmallEnemyLimit = 5;
        private float waveTimer = 0f;
        private bool isSpawning = false;

        private List<GameObject> spawnedSmallEnemies = new List<GameObject>();
        private EnemyManager enemyManager;

        void Start()
        {
            // 查找EnemyManager
            enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager == null)
            {
                Debug.LogError("WaveSpawnManager: 找不到EnemyManager！");
            }

            // 如果没有设置玩家，自动查找
            if (player == null)
            {
                var playerController = FindObjectOfType<Unity.FPS.Gameplay.PlayerCharacterController>();
                if (playerController != null)
                {
                    player = playerController.transform;
                }
                else
                {
                    Debug.LogError("WaveSpawnManager: 找不到玩家！");
                }
            }

            // 初始化数量上限
            currentSmallEnemyLimit = initialSmallEnemyLimit;

            // 启动第一波刷新
            StartCoroutine(StartWaveSpawning());
        }

        void Update()
        {
            // 清理已销毁的小型怪引用
            spawnedSmallEnemies.RemoveAll(enemy => enemy == null);
        }

        /// <summary>
        /// 开始波次刷新协程
        /// </summary>
        IEnumerator StartWaveSpawning()
        {
            while (true)
            {
                yield return new WaitForSeconds(waveInterval);

                if (!isSpawning)
                {
                    StartCoroutine(SpawnWave());
                }
            }
        }

        /// <summary>
        /// 刷新一波怪物
        /// </summary>
        IEnumerator SpawnWave()
        {
            isSpawning = true;
            currentWave++;

            Debug.Log($"[波次 {currentWave}] 开始刷新 - 小型怪上限: {currentSmallEnemyLimit}");

            // 计算当前波次的属性加成
            float healthMultiplier = 1f + (currentWave - 1) * healthIncreasePerWave;
            float damageMultiplier = 1f + (currentWave - 1) * damageIncreasePerWave;

            // 刷新小型怪
            SpawnSmallEnemies(healthMultiplier, damageMultiplier);

            // 刷新大型怪
            SpawnLargeEnemies(healthMultiplier, damageMultiplier);

            // 增加下一波的数量上限
            currentSmallEnemyLimit += limitIncreasePerWave;

            isSpawning = false;
            yield return null;
        }

        /// <summary>
        /// 刷新小型怪
        /// </summary>
        void SpawnSmallEnemies(float healthMultiplier, float damageMultiplier)
        {
            if (player == null)
            {
                Debug.LogWarning("玩家未设置！");
                return;
            }

            // 检查是否有可用的小型怪类型
            if (!HasValidSmallEnemyTypes())
            {
                Debug.LogWarning("没有有效的小型怪预制体！请在 Small Enemy Types 列表中添加怪物类型。");
                return;
            }

            // 计算需要刷新的数量
            int currentCount = spawnedSmallEnemies.Count;
            int spawnCount = Mathf.Max(0, currentSmallEnemyLimit - currentCount);

            if (spawnCount <= 0)
            {
                Debug.Log($"[波次 {currentWave}] 小型怪数量已达上限 ({currentCount}/{currentSmallEnemyLimit})，跳过刷新");
                return;
            }

            Debug.Log($"[波次 {currentWave}] 刷新 {spawnCount} 只小型怪 (当前: {currentCount}, 上限: {currentSmallEnemyLimit})");

            // 刷新小型怪
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPosition = GetRandomSpawnPositionAroundPlayer();
                if (spawnPosition != Vector3.zero)
                {
                    // 根据概率选择怪物类型
                    GameObject selectedPrefab = SelectRandomSmallEnemyPrefab();
                    if (selectedPrefab != null)
                    {
                        GameObject enemy = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                        ApplyWaveBuffs(enemy, healthMultiplier, damageMultiplier, false);
                        spawnedSmallEnemies.Add(enemy);
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否有有效的小型怪类型
        /// </summary>
        bool HasValidSmallEnemyTypes()
        {
            // 优先检查新的类型列表
            if (smallEnemyTypes != null && smallEnemyTypes.Count > 0)
            {
                foreach (var type in smallEnemyTypes)
                {
                    if (type.prefab != null && type.spawnWeight > 0)
                    {
                        return true;
                    }
                }
            }

            // 兼容旧版单一预制体
            return smallEnemyPrefab != null;
        }

        /// <summary>
        /// 根据概率权重随机选择一个小型怪预制体
        /// </summary>
        GameObject SelectRandomSmallEnemyPrefab()
        {
            // 优先使用新的类型列表
            if (smallEnemyTypes != null && smallEnemyTypes.Count > 0)
            {
                // 收集所有有效的类型
                List<SmallEnemyType> validTypes = new List<SmallEnemyType>();
                foreach (var type in smallEnemyTypes)
                {
                    if (type.prefab != null && type.spawnWeight > 0)
                    {
                        validTypes.Add(type);
                    }
                }

                if (validTypes.Count == 0)
                {
                    // 如果列表中没有有效类型，回退到单一预制体
                    return smallEnemyPrefab;
                }

                // 计算总权重
                float totalWeight = 0f;
                foreach (var type in validTypes)
                {
                    totalWeight += type.spawnWeight;
                }

                // 随机选择
                float randomValue = Random.Range(0f, totalWeight);
                float cumulativeWeight = 0f;

                foreach (var type in validTypes)
                {
                    cumulativeWeight += type.spawnWeight;
                    if (randomValue <= cumulativeWeight)
                    {
                        Debug.Log($"[波次 {currentWave}] 选择怪物类型: {type.typeName} (权重: {type.spawnWeight}/{totalWeight})");
                        return type.prefab;
                    }
                }

                // 如果出错，返回第一个有效类型
                return validTypes[0].prefab;
            }

            // 兼容旧版：使用单一预制体
            return smallEnemyPrefab;
        }

        /// <summary>
        /// 刷新大型怪
        /// </summary>
        void SpawnLargeEnemies(float healthMultiplier, float damageMultiplier)
        {
            if (largeEnemyPrefab == null)
            {
                Debug.LogWarning("大型怪预制体未设置！");
                return;
            }

            int spawnedCount = 0;

            foreach (var spawnPoint in largeEnemySpawnPoints)
            {
                if (spawnPoint != null && !spawnPoint.HasLargeEnemy())
                {
                    GameObject enemy = Instantiate(largeEnemyPrefab, spawnPoint.transform.position, Quaternion.identity);
                    ApplyWaveBuffs(enemy, healthMultiplier, damageMultiplier, true);
                    spawnPoint.RegisterEnemy(enemy);
                    spawnedCount++;
                }
            }

            if (spawnedCount > 0)
            {
                Debug.Log($"[波次 {currentWave}] 刷新 {spawnedCount} 只大型怪");
            }
        }

        /// <summary>
        /// 应用波次增益（血量和攻击力）
        /// </summary>
        void ApplyWaveBuffs(GameObject enemy, float healthMultiplier, float damageMultiplier, bool isLargeEnemy)
        {
            // 增强血量
            var health = enemy.GetComponent<Health>();
            if (health != null)
            {
                float originalMaxHealth = health.MaxHealth;
                health.MaxHealth *= healthMultiplier;
                health.CurrentHealth = health.MaxHealth;
                Debug.Log($"怪物血量: {originalMaxHealth} -> {health.MaxHealth} (x{healthMultiplier:F2})");
            }

            // 增强攻击力
            var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                var weapons = enemyController.GetComponentsInChildren<WeaponController>();
                foreach (var weapon in weapons)
                {
                    // 遍历武器的所有射击模块
                    var projectileBase = weapon.GetComponentInChildren<Unity.FPS.Gameplay.ProjectileStandard>();
                    if (projectileBase != null)
                    {
                        float originalDamage = projectileBase.Damage;
                        projectileBase.Damage *= damageMultiplier;
                        Debug.Log($"武器伤害: {originalDamage} -> {projectileBase.Damage} (x{damageMultiplier:F2})");
                    }
                }
            }

            // 标记怪物类型（可选，用于后续识别）
            var enemyType = enemy.AddComponent<EnemyWaveData>();
            enemyType.waveNumber = currentWave;
            enemyType.isLargeEnemy = isLargeEnemy;
            enemyType.healthMultiplier = healthMultiplier;
            enemyType.damageMultiplier = damageMultiplier;
        }

        /// <summary>
        /// 获取玩家周围的随机刷新位置（在NavMesh上）
        /// </summary>
        Vector3 GetRandomSpawnPositionAroundPlayer()
        {
            const int maxAttempts = 30;

            for (int i = 0; i < maxAttempts; i++)
            {
                // 在圆形范围内随机生成位置
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 randomPosition = player.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                // 尝试在NavMesh上采样位置
                if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    // 确保位置在地面上且不在玩家视野正前方
                    Vector3 directionToSpawn = (hit.position - player.position).normalized;
                    Vector3 playerForward = player.forward;
                    float angle = Vector3.Angle(playerForward, directionToSpawn);

                    // 避免在玩家正前方60度范围内刷新
                    if (angle > 60f)
                    {
                        return hit.position;
                    }
                }
            }

            Debug.LogWarning("无法找到合适的刷新位置！");
            return Vector3.zero;
        }

        /// <summary>
        /// 手动触发刷新（用于调试）
        /// </summary>
        [ContextMenu("手动刷新一波")]
        public void ManualSpawnWave()
        {
            if (!isSpawning)
            {
                StartCoroutine(SpawnWave());
            }
        }

        /// <summary>
        /// 重置波次系统
        /// </summary>
        [ContextMenu("重置波次")]
        public void ResetWaves()
        {
            currentWave = 0;
            currentSmallEnemyLimit = initialSmallEnemyLimit;
            
            // 清理所有已刷新的小型怪
            foreach (var enemy in spawnedSmallEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            spawnedSmallEnemies.Clear();

            Debug.Log("波次系统已重置");
        }

        void OnDrawGizmosSelected()
        {
            if (!showSpawnRadius || player == null)
                return;

            // 绘制小型怪刷新范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, spawnRadius);

            // 绘制大型怪刷新点
            Gizmos.color = Color.red;
            foreach (var spawnPoint in largeEnemySpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.transform.position, 2f);
                }
            }
        }

        void OnGUI()
        {
            // 显示波次信息（调试用）
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Box($"=== 波次刷新系统 ===\n" +
                         $"当前波次: {currentWave}\n" +
                         $"小型怪上限: {currentSmallEnemyLimit}\n" +
                         $"场上小型怪: {spawnedSmallEnemies.Count}\n" +
                         $"血量倍率: {(1f + currentWave * healthIncreasePerWave):F2}x\n" +
                         $"伤害倍率: {(1f + currentWave * damageIncreasePerWave):F2}x");
            GUILayout.EndArea();
        }
    }
}

