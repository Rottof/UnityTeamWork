using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Hex
{
    public class HexEffects : MonoBehaviour
    {
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

        public void OnRandomEffect()
        {
            int r = Random.Range(0, 4);
            if (r == 0)
            {
                }
            else if (r == 1)
            {
            }
            else if (r == 2)
            {
            }
            else
            {
            }
        }

        public void OnSpeedUp()
        {
        }
    }
}