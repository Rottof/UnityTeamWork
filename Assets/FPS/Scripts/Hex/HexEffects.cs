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
            allHexEffects.Add(new HexData("豌豆射手", "有15%的概率同时发射3颗子弹", OnMultiShot));
            allHexEffects.Add(new HexData("生命源泉", "每隔5秒，回复1点生命值", OnLifeSource));
            allHexEffects.Add(new HexData("吸血鬼", "造成伤害时回复伤害值的10%生命值", OnVampirism));
            allHexEffects.Add(new HexData("弹药充裕", "最大弹药量 +50%", OnAmmoBoost));
            allHexEffects.Add(new HexData("致命打击", "有20%的概率造成双倍伤害", OnCriticalStrike));
            allHexEffects.Add(new HexData("坚韧不拔", "受到伤害降低10%", OnDamageReduction));
            allHexEffects.Add(new HexData("狂战士", "生命值低于30%时，攻击速度 +50%", OnBerserker));

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
                            ProjectileStandard originalProjectile = weapon.ProjectilePrefab.GetComponent<ProjectileStandard>();
                            if (originalProjectile != null)
                            {
                                // 创建投射物预制体的运行时副本，避免修改原始资源
                                // 这样重新开始游戏时，武器会使用原始预制体，伤害会恢复正常
                                ProjectileBase runtimeProjectilePrefab = Instantiate(weapon.ProjectilePrefab);
                                runtimeProjectilePrefab.gameObject.SetActive(false);
                                
                                // 防止副本在场景切换时被销毁
                                DontDestroyOnLoad(runtimeProjectilePrefab.gameObject);
                                
                                // 修改副本的伤害值
                                ProjectileStandard runtimeProjectile = runtimeProjectilePrefab.GetComponent<ProjectileStandard>();
                                if (runtimeProjectile != null)
                                {
                                    runtimeProjectile.Damage = originalProjectile.Damage + 5f;
                                    
                                    // 替换武器的投射物预制体引用为运行时副本
                                    weapon.ProjectilePrefab = runtimeProjectilePrefab;
                                    
                                    weaponCount++;
                                    Debug.Log($"武器 {weapon.WeaponName} 的伤害增加了5点（基础伤害 {originalProjectile.Damage} -> 增强后 {runtimeProjectile.Damage}）");
                                }
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
            // 这个效果需要在击杀敌人后激活，这里实现加速逻辑
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                StartCoroutine(ApplySpeedBoost(player, 0.3f, 1.0f));
                print("迅捷步伐触发：移动速度 +30%，持续1秒！");
            }
        }
        
        // 多重射击：有15%的概率同时发射3颗子弹
        public void OnMultiShot()
        {
            // 这个效果需要修改武器的射击逻辑，这里只是记录
            print("多重射击激活：有15%的概率同时发射3颗子弹！");
        }
        
        // 生命源泉：每隔5秒，回复1点最大生命值
        public void OnLifeSource()
        {
            // 这个效果需要持续运行，这里启动一个协程
            StartCoroutine(HealOverTime());
            print("生命源泉激活：每隔5秒，回复1点生命值！");
        }
        
        IEnumerator ApplySpeedBoost(PlayerCharacterController player, float boostAmount, float duration)
        {
            float originalSpeed = player.SprintSpeedModifier;
            player.SprintSpeedModifier += boostAmount;
            
            yield return new WaitForSeconds(duration);
            
            // 确保玩家对象仍然存在
            if (player != null)
            {
                player.SprintSpeedModifier = originalSpeed;
            }
        }
        
        IEnumerator HealOverTime()
        {
            while (true)
            {
                yield return new WaitForSeconds(5.0f); // 等待5秒
                
                PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
                if (player != null)
                {
                    Health playerHealth = player.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        // 回复1点生命值
                        playerHealth.Heal(1f);
                        print("生命源泉：回复了1点生命值！");
                    }
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
            // 这个效果需要在造成伤害时判断触发，通常在武器系统中实现
            print("致命打击激活：有20%的概率造成双倍伤害！");
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

        // 狂战士：生命值低于30%时，攻击速度+50%
        public void OnBerserker()
        {
            // 这个效果需要持续检测玩家生命值，并在生命值低于30%时激活
            StartCoroutine(BerserkerEffect());
            print("狂战士激活：生命值低于30%时，攻击速度 +50%！");
        }

        IEnumerator BerserkerEffect()
        {
            bool isBerserkerActive = false;
            
            while (true)
            {
                yield return new WaitForSeconds(0.5f); // 每0.5秒检测一次
                
                PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
                if (player != null)
                {
                    Health playerHealth = player.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        float healthPercentage = playerHealth.CurrentHealth / playerHealth.MaxHealth;
                        
                        // 如果生命值低于30%且狂战士未激活
                        if (healthPercentage < 0.3f && !isBerserkerActive)
                        {
                            isBerserkerActive = true;
                            print("狂战士效果触发！攻击速度大幅提升！");
                            // 这里可以修改武器的射速或其他攻击相关属性
                        }
                        // 如果生命值恢复到30%以上且狂战士已激活
                        else if (healthPercentage >= 0.3f && isBerserkerActive)
                        {
                            isBerserkerActive = false;
                            print("狂战士效果结束。");
                            // 恢复正常的攻击速度
                        }
                    }
                }
            }
        }

        void OnDestroy()
        {
            // 清理事件监听器，防止内存泄漏
            if (isHeartOfSteelActive)
            {
                EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilledForHeartOfSteel);
            }
        }

    }
}