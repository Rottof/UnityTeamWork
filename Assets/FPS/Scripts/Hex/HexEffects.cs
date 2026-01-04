using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using System;

namespace Unity.FPS.Hex
{
    // 海克斯数据类型
    [System.Serializable]
    public class HexData
    {
        public string hexName;           // 海克斯名称
        public string hexDescription;    // 海克斯描述
        public Action hexEffect;         // 海克斯效果函数

        public HexData(string name, string description, Action effect)
        {
            hexName = name;
            hexDescription = description;
            hexEffect = effect;
        }
    }

    public class HexEffects : MonoBehaviour
    {
        // 所有可用的海克斯效果列表
        public List<HexData> allHexEffects = new List<HexData>();

        // 海克斯效果激活状态标志
        private bool isHeartOfSteelActive = false;
        private bool isSwiftFootworkActive = false;
        private bool isMultiShotActive = false;
        private bool isCriticalStrikeActive = false;
        private bool isBerserkerActive = false;
        private bool isBerserkerEffectActive = false; // 狂战士效果是否正在生效

        // 记录武器原始伤害值，用于恢复
        private Dictionary<ProjectileStandard, float> originalDamageValues = new Dictionary<ProjectileStandard, float>();

        // 多重打击配置
        private const float MULTI_SHOT_CHANCE = 1.0f; // 100%触发概率
        private const int MULTI_SHOT_EXTRA_BULLETS = 2; // 额外发射2颗子弹
        private const float MULTI_SHOT_SPREAD_ANGLE = 45f; // 扇形总角度45度（3颗子弹，每颗间隔15度）

        // 致命打击配置
        private const float CRITICAL_STRIKE_CHANCE = 0.2f; // 20%暴击概率
        private const float CRITICAL_STRIKE_MULTIPLIER = 2.0f; // 双倍伤害

        // 狂战士配置
        private const float BERSERKER_HEALTH_THRESHOLD = 0.3f; // 30%生命值阈值
        private const float BERSERKER_DAMAGE_MULTIPLIER = 1.5f; // 伤害增加50%（即1.5倍）

        // 保存武器和对应的射击回调，用于清理
        private Dictionary<WeaponController, System.Action> multiShotCallbacks = new Dictionary<WeaponController, System.Action>();
        private Dictionary<WeaponController, System.Action> criticalStrikeCallbacks = new Dictionary<WeaponController, System.Action>();
        
        // 保存狂战士效果激活前的原始伤害
        private Dictionary<ProjectileStandard, float> berserkerOriginalDamages = new Dictionary<ProjectileStandard, float>();

        /// <summary>
        /// 应用海克斯效果并在UI上显示
        /// </summary>
        public void ApplyHexEffect(HexData hexData)
        {
            if (hexData == null)
            {
                Debug.LogWarning("尝试应用空的海克斯效果！");
                return;
            }

            // 执行海克斯效果
            hexData.hexEffect?.Invoke();

            // 在UI上显示
            if (HexEffectDisplayManager.Instance != null)
            {
                HexEffectDisplayManager.Instance.AddHexEffect(hexData.hexName);
                Debug.Log($"已添加海克斯效果到UI: {hexData.hexName}");
            }
            else
            {
                Debug.LogWarning("HexEffectDisplayManager 未找到！请确保场景中有 HexEffectPanel 对象并附加了 HexEffectDisplayManager 组件。");
            }
        }

        void Awake()
        {
            // 初始化海克斯效果列表，一共12个
            allHexEffects.Clear();
            allHexEffects.Add(new HexData("生命强化", "最大生命值 +10", OnHealthUp));
            allHexEffects.Add(new HexData("攻击强化", "攻击力 +5", OnAttackUp));
            allHexEffects.Add(new HexData("速度强化", "移动速度 +10%", OnSpeedUp));
            allHexEffects.Add(new HexData("心之钢", "每击杀一个敌人，最大生命值 +3", OnHeartOfSteel));
            allHexEffects.Add(new HexData("迅捷步伐", "击杀敌人后，移动速度 +30%，持续1秒", OnSwiftFootwork));
            allHexEffects.Add(new HexData("强力攻击", "攻击时同时发射3颗子弹", OnMultiShot));
            allHexEffects.Add(new HexData("生命源泉", "每隔5秒，回复10点生命值", OnLifeSource));
            allHexEffects.Add(new HexData("致命打击", "有20%的概率造成双倍伤害", OnCriticalStrike));
            allHexEffects.Add(new HexData("狂战士", "生命值低于30%时，伤害 +50%", OnBerserker));

            allHexEffects.Add(new HexData("坚韧不拔", "受到伤害降低10%", OnDamageReduction));
            allHexEffects.Add(new HexData("弹药充裕", "最大弹药量 +50%", OnAmmoBoost));
            allHexEffects.Add(new HexData("幸运之子", "增加击杀敌人掉落战利品的概率", OnVampirism));

            Debug.Log("海克斯效果列表初始化完成");
        }

        // 获取随机的N个海克斯效果（不重复）
        public List<HexData> GetRandomHexEffects(int count)
        {
            List<HexData> randomEffects = new List<HexData>();
            List<HexData> tempList = new List<HexData>(allHexEffects);

            // 确保不超过可用效果数量
            count = Mathf.Min(count, tempList.Count);

            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempList.Count);
                randomEffects.Add(tempList[randomIndex]);
                tempList.RemoveAt(randomIndex); // 移除已选择的，避免重复
            }

            return randomEffects;
        }

        // 基础效果，攻击力+5 （所有武器的伤害增加5点）
        public void OnAttackUp()
        {
            // 由于没有直接的攻击力属性，我们通过获取玩家的武器来增加伤害
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();
                if (weaponsManager != null)
                {
                    // 遍历所有武器槽位，增加武器伤害
                    int weaponCount = 0;
                    for (int i = 0; i < 9; i++) // PlayerWeaponsManager 有9个武器槽位
                    {
                        WeaponController weapon = weaponsManager.GetWeaponAtSlotIndex(i);
                        if (weapon != null && weapon.ProjectilePrefab != null)
                        {
                            // 检查投射物是否是 ProjectileStandard 类型
                            ProjectileStandard projectile = weapon.ProjectilePrefab.GetComponent<ProjectileStandard>();
                            if (projectile != null)
                            {
                                // 如果还没记录原始伤害，先记录
                                if (!originalDamageValues.ContainsKey(projectile))
                                {
                                    originalDamageValues[projectile] = projectile.Damage;
                                }
                                
                                // 直接增加伤害值
                                float oldDamage = projectile.Damage;
                                projectile.Damage += 5f;
                                
                                weaponCount++;
                                Debug.Log($"武器 {weapon.WeaponName} 的伤害增加了5点（{oldDamage} -> {projectile.Damage}）");
                            }
                        }
                    }
                    
                    if (weaponCount > 0)
                    {
                        print($"攻击力 +5！已增强 {weaponCount} 件武器的伤害");
                    }
                    else
                    {
                        print("攻击力 +5！但当前没有可增强的武器");
                    }
                }
            }
        }
        
        // 基础效果，移动速度+10%（提升正常行走速度）
        public void OnSpeedUp()
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                // 增加10%的正常行走速度
                float oldSpeed = player.MaxSpeedOnGround;
                player.MaxSpeedOnGround *= 1.1f;
                print($"移动速度 +10%！（{oldSpeed:F2} -> {player.MaxSpeedOnGround:F2}）");
            }
        }
        
        // 心之钢——每击杀一个敌人，最大生命值+3
        public void OnHeartOfSteel()
        {
            if (!isHeartOfSteelActive)
            {
                isHeartOfSteelActive = true;
                // 注册敌人击杀事件监听器
                EventManager.AddListener<EnemyKillEvent>(OnEnemyKilledForHeartOfSteel);
                print("心之钢激活：每击杀一个敌人，最大生命值 +3！");
            }
        }

        // 心之钢效果的回调函数
        void OnEnemyKilledForHeartOfSteel(EnemyKillEvent evt)
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    float oldMaxHealth = playerHealth.MaxHealth;
                    playerHealth.IncreaseMaxHealth(3f);
                    print($"心之钢触发：最大生命值 +3！（{oldMaxHealth} -> {playerHealth.MaxHealth}）");
                }
            }
        }
        
        // 迅捷步伐——击杀一个敌人后，移动速度+30%，持续1秒
        public void OnSwiftFootwork()
        {
            if (!isSwiftFootworkActive)
            {
                isSwiftFootworkActive = true;
                // 注册敌人击杀事件监听器
                EventManager.AddListener<EnemyKillEvent>(OnEnemyKilledForSwiftFootwork);
                print("迅捷步伐激活：击杀敌人后，移动速度 +30%，持续1秒！");
            }
        }

        // 迅捷步伐效果的回调函数
        void OnEnemyKilledForSwiftFootwork(EnemyKillEvent evt)
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                // 在玩家对象上启动协程，避免在非激活的对象上启动
                player.StartCoroutine(ApplySpeedBoostForSwiftFootwork(player));
            }
        }
        
        // 多重射击：每次射击都会同时发射3颗子弹
        public void OnMultiShot()
        {
            if (!isMultiShotActive)
            {
                isMultiShotActive = true;
                // 为所有武器注册多重射击监听器
                RegisterMultiShotForAllWeapons();
                print("多重射击激活：每次射击都会同时发射3颗子弹！");
            }
        }

        // 为所有武器注册多重射击事件
        void RegisterMultiShotForAllWeapons()
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();
                if (weaponsManager != null)
                {
                    // 为现有武器注册
                    for (int i = 0; i < 9; i++)
                    {
                        WeaponController weapon = weaponsManager.GetWeaponAtSlotIndex(i);
                        if (weapon != null)
                        {
                            RegisterMultiShotForWeapon(weapon);
                        }
                    }

                    // 监听新武器添加事件
                    weaponsManager.OnAddedWeapon += (weapon, index) => RegisterMultiShotForWeapon(weapon);
                }
            }
        }

        // 为单个武器注册多重射击回调
        void RegisterMultiShotForWeapon(WeaponController weapon)
        {
            if (weapon != null && !multiShotCallbacks.ContainsKey(weapon))
            {
                // 创建回调并保存引用
                System.Action callback = () => OnWeaponShootForMultiShot(weapon);
                multiShotCallbacks[weapon] = callback;
                weapon.OnShootProcessed += callback;
                Debug.Log($"已为武器 {weapon.WeaponName} 注册多重射击效果");
            }
        }

        // 武器射击时的多重射击回调
        void OnWeaponShootForMultiShot(WeaponController weapon)
        {
            // 100%触发（如需概率可修改 MULTI_SHOT_CHANCE）
            if (UnityEngine.Random.value <= MULTI_SHOT_CHANCE)
            {
                // 发射额外的子弹，成扇形分布
                float angleStep = MULTI_SHOT_SPREAD_ANGLE / (MULTI_SHOT_EXTRA_BULLETS + 1);
                float startAngle = -MULTI_SHOT_SPREAD_ANGLE / 2f;

                for (int i = 0; i < MULTI_SHOT_EXTRA_BULLETS; i++)
                {
                    // 计算扇形中每颗子弹的角度
                    float angle = startAngle + angleStep * (i + 1);
                    Vector3 direction = Quaternion.Euler(0, angle, 0) * weapon.WeaponMuzzle.forward;

                    // 实例化投射物
                    ProjectileBase newProjectile = Instantiate(weapon.ProjectilePrefab,
                        weapon.WeaponMuzzle.position,
                        Quaternion.LookRotation(direction));
                    newProjectile.Shoot(weapon);
                }

                Debug.Log($"多重射击触发！额外发射了 {MULTI_SHOT_EXTRA_BULLETS} 颗子弹（总共3颗，扇形角度45度）");
            }
        }
        
        // 生命源泉：每隔5秒，回复10点生命值
        public void OnLifeSource()
        {
            // 在玩家对象上启动协程，避免在非激活对象上启动
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                player.StartCoroutine(HealOverTime(player));
                print("生命源泉激活：每隔5秒，回复10点生命值！");
            }
            else
            {
                Debug.LogWarning("生命源泉：未找到玩家对象！");
            }
        }
        
        IEnumerator ApplySpeedBoostForSwiftFootwork(PlayerCharacterController player)
        {
            // 保存原始速度
            float originalSpeed = player.MaxSpeedOnGround;
            // 临时提升30%移动速度
            player.MaxSpeedOnGround *= 1.3f;
            
            // 广播速度提升事件（激活）
            SpeedBoostEvent speedBoostEvent = Events.SpeedBoostEvent;
            speedBoostEvent.IsActive = true;
            EventManager.Broadcast(speedBoostEvent);
            
            print($"迅捷步伐触发：移动速度 +30%！（{originalSpeed:F2} -> {player.MaxSpeedOnGround:F2}）");
            
            // 等待1秒
            yield return new WaitForSeconds(1.0f);
            
            // 恢复原始速度
            if (player != null)
            {
                player.MaxSpeedOnGround = originalSpeed;
                print($"迅捷步伐效果结束，速度恢复至 {originalSpeed:F2}");
            }
            
            // 广播速度提升事件（结束）
            speedBoostEvent.IsActive = false;
            EventManager.Broadcast(speedBoostEvent);
        }
        
        IEnumerator HealOverTime(PlayerCharacterController player)
        {
            if (player == null)
            {
                Debug.LogError("生命源泉：玩家对象为空，无法启动协程！");
                yield break;
            }

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("生命源泉：玩家没有 Health 组件！");
                yield break;
            }

            while (true)
            {
                yield return new WaitForSeconds(5.0f); // 等待5秒
                
                // 检查玩家对象是否还存在
                if (player != null && playerHealth != null)
                {
                    // 回复10点生命值
                    playerHealth.Heal(10f);
                    print($"生命源泉：回复了10点生命值！（当前: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}）");
                }
                else
                {
                    Debug.LogWarning("生命源泉：玩家对象已销毁，停止回复。");
                    yield break;
                }
            }
        }
        
        public void OnHealthUp()
        {
            // 查找玩家并增加最大生命值
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(10f);

                    print("最大生命值 +10!");
                    print(playerHealth.MaxHealth);
                }
            }
        }

        // 吸血鬼：造成伤害时回复伤害值的10%生命值
        public void OnVampirism()
        {
            // 这个效果需要在造成伤害时触发，通常在武器击中敌人时调用
            print("吸血鬼激活：造成伤害时回复伤害值的10%生命值！");
        }

        // 弹药充裕：最大弹药量+50%
        public void OnAmmoBoost()
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();
                if (weaponsManager != null)
                {
                    // 增加所有武器的最大弹药量
                    WeaponController[] weapons = weaponsManager.GetComponentsInChildren<WeaponController>();
                    foreach (WeaponController weapon in weapons)
                    {
                        if (weapon != null)
                        {
                            // 这里需要访问武器的弹药系统来增加最大弹药
                            // 具体实现取决于武器系统的结构
                            print($"武器 {weapon.WeaponName} 的弹药量增加！");
                        }
                    }
                    print("弹药充裕激活：最大弹药量 +50%！");
                }
            }
        }

        // 致命打击：有20%的概率造成双倍伤害
        public void OnCriticalStrike()
        {
            if (!isCriticalStrikeActive)
            {
                isCriticalStrikeActive = true;
                RegisterCriticalStrikeForAllWeapons();
                print("致命打击激活：有20%的概率造成双倍伤害！");
            }
        }

        void RegisterCriticalStrikeForAllWeapons()
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();
                if (weaponsManager != null)
                {
                    // 为所有现有武器注册暴击效果
                    for (int i = 0; i < 9; i++)
                    {
                        WeaponController weapon = weaponsManager.GetWeaponAtSlotIndex(i);
                        if (weapon != null)
                        {
                            RegisterCriticalStrikeForWeapon(weapon);
                        }
                    }

                    // 为将来添加的武器注册暴击效果
                    weaponsManager.OnAddedWeapon += (weapon, index) => RegisterCriticalStrikeForWeapon(weapon);
                }
            }
        }

        void RegisterCriticalStrikeForWeapon(WeaponController weapon)
        {
            if (weapon != null && !criticalStrikeCallbacks.ContainsKey(weapon))
            {
                System.Action callback = () => OnWeaponShootForCriticalStrike(weapon);
                criticalStrikeCallbacks[weapon] = callback;
                weapon.OnShootProcessed += callback;
                Debug.Log($"已为武器 {weapon.WeaponName} 注册致命打击效果");
            }
        }

        void OnWeaponShootForCriticalStrike(WeaponController weapon)
        {
            // 20%概率触发暴击
            if (UnityEngine.Random.value <= CRITICAL_STRIKE_CHANCE)
            {
                if (weapon.ProjectilePrefab != null)
                {
                    ProjectileStandard projectile = weapon.ProjectilePrefab.GetComponent<ProjectileStandard>();
                    if (projectile != null)
                    {
                        // 保存原始伤害
                        float originalDamage = projectile.Damage;
                        
                        // 临时增加伤害为双倍
                        projectile.Damage *= CRITICAL_STRIKE_MULTIPLIER;
                        
                        Debug.Log($"💥 致命打击触发！武器 {weapon.WeaponName} 造成暴击伤害！（{originalDamage} -> {projectile.Damage}）");
                        
                        // 在下一帧恢复原始伤害
                        PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
                        if (player != null)
                        {
                            player.StartCoroutine(RestoreDamageAfterShoot(projectile, originalDamage));
                        }
                    }
                }
            }
        }

        IEnumerator RestoreDamageAfterShoot(ProjectileStandard projectile, float originalDamage)
        {
            // 等待一帧，让投射物实例化完成
            yield return null;
            
            // 恢复原始伤害
            if (projectile != null)
            {
                projectile.Damage = originalDamage;
            }
        }

        // 坚韧不拔：受到伤害降低10%
        public void OnDamageReduction()
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    // 这里需要修改Health组件来支持伤害减免
                    // 可以通过添加一个伤害减免系数来实现
                    print("坚韧不拔激活：受到伤害降低10%！");
                }
            }
        }

        // 狂战士：生命值低于30%时，伤害+50%
        public void OnBerserker()
        {
            if (!isBerserkerActive)
            {
                isBerserkerActive = true;
                // 在玩家对象上启动协程，避免在非激活对象上启动
                PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
                if (player != null)
                {
                    player.StartCoroutine(BerserkerEffect(player));
                    print("狂战士激活：生命值低于30%时，伤害 +50%！");
                }
                else
                {
                    Debug.LogWarning("狂战士：未找到玩家对象！");
                }
            }
        }

        IEnumerator BerserkerEffect(PlayerCharacterController player)
        {
            if (player == null)
            {
                Debug.LogError("狂战士：玩家对象为空，无法启动协程！");
                yield break;
            }

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("狂战士：玩家没有 Health 组件！");
                yield break;
            }

            PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();
            if (weaponsManager == null)
            {
                Debug.LogError("狂战士：玩家没有 PlayerWeaponsManager 组件！");
                yield break;
            }

            while (true)
            {
                yield return new WaitForSeconds(0.1f); // 每0.1秒检测一次，更快响应
                
                // 检查玩家对象是否还存在
                if (player == null || playerHealth == null)
                {
                    Debug.LogWarning("狂战士：玩家对象已销毁，停止检测。");
                    yield break;
                }

                float healthPercentage = playerHealth.CurrentHealth / playerHealth.MaxHealth;
                
                // 如果生命值低于30%且效果未激活
                if (healthPercentage < BERSERKER_HEALTH_THRESHOLD && !isBerserkerEffectActive)
                {
                    ActivateBerserkerDamageBoost(weaponsManager);
                }
                // 如果生命值恢复到30%以上且效果已激活
                else if (healthPercentage >= BERSERKER_HEALTH_THRESHOLD && isBerserkerEffectActive)
                {
                    DeactivateBerserkerDamageBoost();
                }
            }
        }

        void ActivateBerserkerDamageBoost(PlayerWeaponsManager weaponsManager)
        {
            isBerserkerEffectActive = true;
            int weaponCount = 0;

            for (int i = 0; i < 9; i++)
            {
                WeaponController weapon = weaponsManager.GetWeaponAtSlotIndex(i);
                if (weapon != null && weapon.ProjectilePrefab != null)
                {
                    ProjectileStandard projectile = weapon.ProjectilePrefab.GetComponent<ProjectileStandard>();
                    if (projectile != null)
                    {
                        // 保存原始伤害（如果还没保存过）
                        if (!berserkerOriginalDamages.ContainsKey(projectile))
                        {
                            berserkerOriginalDamages[projectile] = projectile.Damage;
                        }

                        float oldDamage = projectile.Damage;
                        projectile.Damage *= BERSERKER_DAMAGE_MULTIPLIER;
                        weaponCount++;
                        Debug.Log($"狂战士效果触发！武器 {weapon.WeaponName} 伤害增加50%（{oldDamage} -> {projectile.Damage}）");
                    }
                }
            }

            if (weaponCount > 0)
            {
                print($"🔥 狂战士效果触发！所有武器伤害 +50%！（{weaponCount} 件武器）");
            }
        }

        void DeactivateBerserkerDamageBoost()
        {
            isBerserkerEffectActive = false;
            int weaponCount = 0;

            foreach (var kvp in berserkerOriginalDamages)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.Damage = kvp.Value;
                    weaponCount++;
                    Debug.Log($"狂战士效果结束，恢复武器伤害：{kvp.Key.gameObject.name} = {kvp.Value}");
                }
            }

            berserkerOriginalDamages.Clear();

            if (weaponCount > 0)
            {
                print($"狂战士效果结束，所有武器伤害恢复正常。（{weaponCount} 件武器）");
            }
        }

        void OnDestroy()
        {
            // 清理事件监听器，防止内存泄漏
            if (isHeartOfSteelActive)
            {
                EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilledForHeartOfSteel);
            }
            if (isSwiftFootworkActive)
            {
                EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilledForSwiftFootwork);
            }

            // 清理多重打击回调
            if (isMultiShotActive)
            {
                foreach (var kvp in multiShotCallbacks)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.OnShootProcessed -= kvp.Value;
                    }
                }
                multiShotCallbacks.Clear();
            }

            // 清理致命打击回调
            if (isCriticalStrikeActive)
            {
                foreach (var kvp in criticalStrikeCallbacks)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.OnShootProcessed -= kvp.Value;
                    }
                }
                criticalStrikeCallbacks.Clear();
            }

            // 清理狂战士效果
            if (isBerserkerEffectActive)
            {
                foreach (var kvp in berserkerOriginalDamages)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.Damage = kvp.Value;
                        Debug.Log($"狂战士效果清理：恢复投射物伤害 {kvp.Key.gameObject.name} = {kvp.Value}");
                    }
                }
                berserkerOriginalDamages.Clear();
            }

            // 恢复武器原始伤害值（攻击强化效果）
            foreach (var kvp in originalDamageValues)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.Damage = kvp.Value;
                    Debug.Log($"恢复投射物伤害：{kvp.Key.gameObject.name} = {kvp.Value}");
                }
            }
            originalDamageValues.Clear();
        }

    }
}