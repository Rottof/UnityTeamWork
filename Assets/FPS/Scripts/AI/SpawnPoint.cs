using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class SpawnPoint : MonoBehaviour
    {
        [Header("刷怪配置（按怪物ID）")]
        public List<SpawnConfig> spawnConfigs = new List<SpawnConfig>();

        [Header("通用设置")]
        public int maxMonsterCount = 5;
        public float activeDistance = 20f;

        [Header("冷却设置")]
        public float cooldownTime = 10f;
        public float resetDistance = 30f;

        private Transform player;
        private int currentMonsterCount = 0;

        private bool inCooldown = false;
        private float cooldownTimer = 0f;
        private bool playerHasLeft = false;

        public void Init(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void Update()
        {
            if (player == null) return;
            if (spawnConfigs.Count == 0) return;

            float distance = Vector3.Distance(player.position, transform.position);

            // ======================
            // 冷却状态处理
            // ======================
            if (inCooldown)
            {
                cooldownTimer += Time.deltaTime;

                if (distance >= resetDistance)
                {
                    playerHasLeft = true;
                }

                // 两个条件都满足才解除冷却
                if (playerHasLeft && cooldownTimer >= cooldownTime)
                {
                    ExitCooldown();
                }

                return; // 冷却中绝不刷怪
            }

            // ======================
            // 非激活距离不刷怪
            // ======================
            if (distance > activeDistance)
                return;

            // ======================
            // 正常刷怪逻辑
            // ======================
            foreach (var config in spawnConfigs)
            {
                config.timer += Time.deltaTime;
                if (config.timer >= config.spawnInterval)
                {
                    TrySpawn(config.monsterId);
                    config.timer = 0f;
                }
            }
        }

        private void TrySpawn(int monsterId)
        {
            if (currentMonsterCount >= maxMonsterCount)
            {
                // 👉 已刷满，立刻进入冷却
                if (!inCooldown)
                {
                    EnterCooldown();
                }
                return;
            }

            GameObject prefab = MonsterDatabase.Instance.GetMonsterById(monsterId);
            if (prefab == null)
            {
                Debug.LogWarning($"Monster ID {monsterId} not found!");
                return;
            }

            Vector3 pos = transform.position + Random.insideUnitSphere * 2f;
            pos.y = transform.position.y;

            GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
            currentMonsterCount++;

            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            EnemyManager.Instance.RegisterEnemy(enemyController);
            enemyController.getHealth().OnDie += OnDead;

            // 👉 刷完这只刚好满
            if (currentMonsterCount >= maxMonsterCount)
            {
                EnterCooldown();
            }
        }
        
        public void OnDead()
        {
            Debug.unityLogger.Log("OnDead");
            currentMonsterCount--;
            if (currentMonsterCount < 0)
                currentMonsterCount = 0;

            // 全清 → 进入冷却
            if (currentMonsterCount == 0 && !inCooldown)
            {
                EnterCooldown();
            }
        }

        private void EnterCooldown()
        {
            inCooldown = true;
            cooldownTimer = 0f;
            playerHasLeft = false;

            // 重置刷怪计时器，防止一恢复就连刷
            foreach (var config in spawnConfigs)
            {
                config.timer = 0f;
            }
        }

        private void ExitCooldown()
        {
            inCooldown = false;
            cooldownTimer = 0f;
            playerHasLeft = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, activeDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, resetDistance);
        }
    }
}
