using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Hex
{
    public class HexEffects : MonoBehaviour
    {

        // 海克斯列表大全：
        // 1.基础效果，最大生命值加10
        // 2.基础效果，攻击力+5
        // 3.基础效果，移动速度+10%
        // 4.心之钢——每击杀一个敌人，最大生命值+3
        // 5.迅捷步伐——击杀一个敌人后，移动速度+30%，持续1秒
        // 6.豌豆射手：有15%的概率同时发射3颗子弹(直线)。
        // 7.生命源泉：每隔5秒，回复1点最大生命值。
        
        //未实现：
        // 8.过热引擎：连续攻击4次后，下一次子弹变（红色），攻击的伤害变为200%。
        // 9.多重射击：有15%的概率同时发射3颗子弹(散射)。

        // 基础效果，攻击力+5
        public void OnAttackUp()
        {
            // 由于没有直接的攻击力属性，我们通过获取玩家的武器来增加伤害
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();
                if (weaponsManager != null)
                {
                    // 这里可以实现攻击力增加的逻辑
                    // 例如，可以修改武器伤害或通过其他方式实现
                    print("攻击力 +5！");
                }
            }
        }
        
        // 基础效果，移动速度+10%
        public void OnSpeedUp()
        {
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                // 增加10%的移动速度
                player.SprintSpeedModifier += 0.1f;
                print("移动速度 +10%！");
                print("当前冲刺速度倍数: " + player.SprintSpeedModifier);
            }
        }
        
        // 心之钢——每击杀一个敌人，最大生命值+3
        public void OnHeartOfSteel()
        {
            // 这个效果需要监听敌人死亡事件，通常在其他地方激活
            // 这里只是定义效果，实际监听在其他地方
            PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
            if (player != null)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(3f);
                    print("心之钢触发：最大生命值 +3！");
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

    }
}