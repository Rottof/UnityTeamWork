using Unity.FPS.Game;
using Unity.FPS.Hex;
using UnityEngine;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Hex
{
    public class HexPickup : Pickup
    {
        [Header("Hex Panel Settings")]
        public GameObject hexPanelObject;

        private bool hasTriggered = false;
        
        //触发海克斯科技面板
        private void OnTriggerEnter(Collider other)
        {
            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();
            
            if (pickingPlayer != null && !hasTriggered)
            {
                hasTriggered = true; 

                if (hexPanelObject != null)
                {
                    // 获取 HexPanel 组件
                    HexPanel hexPanel = hexPanelObject.GetComponent<HexPanel>();
                    if (hexPanel != null)
                    {
                        // 显示海克斯面板（会自动暂停游戏并随机选择3个效果）
                        hexPanel.ShowPanel();
                        
                        Debug.Log("海克斯面板已显示，游戏已暂停！");
                    }
                    else
                    {
                        Debug.LogWarning("未找到HexPanel组件！");
                    }
                    
                    // Play pickup feedback and destroy the object
                    PlayPickupFeedback();
                    Destroy(gameObject);
                }
                else
                {
                    // If no hex panel is assigned, use default pickup behavior
                    base.OnPicked(pickingPlayer);
                    Destroy(gameObject);
                }
                
                // Broadcast pickup event
                // PickupEvent evt = Events.PickupEvent;
                // evt.Pickup = gameObject;
                // EventManager.Broadcast(evt);
            }
        }
        
        // Override OnPicked to prevent double processing
        protected override void OnPicked(PlayerCharacterController player)
        {
            // Only process if hexPanelObject is null, otherwise handled in OnTriggerEnter
            if (hexPanelObject == null && !hasTriggered)
            {
                base.OnPicked(player);
            }
        }
    }
}

