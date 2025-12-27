using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class HexPickup : Pickup
    {
        [Header("Event Panel Settings")]
        public GameObject eventPanel;

        private bool hasTriggered = false;
        
        //触发海克斯科技面板
        private void OnTriggerEnter(Collider other)
        {
            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();
            
            if (pickingPlayer != null && !hasTriggered)
            {
                hasTriggered = true; 

                if (eventPanel != null)
                {
                    // 显示事件面板
                    eventPanel.SetActive(true); 
                    
                    // 暂停游戏
                    Time.timeScale = 0; 
                    
                    // 获取 EventManager 组件
                    EventManager eventManager = eventPanel.GetComponent<EventManager>();
                    if (eventManager != null)
                    {
                        // 将当前触发物传递给EventManager
                        eventManager.SetTriggerObject(gameObject);
                    }
                    
                    Debug.Log("事件面板已显示，游戏已暂停！");
                    
                    // Play pickup feedback and destroy the object
                    PlayPickupFeedback();
                    Destroy(gameObject);
                }
                else
                {
                    // If no event panel is assigned, use default pickup behavior
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
            // Only process if eventPanel is null, otherwise handled in OnTriggerEnter
            if (eventPanel == null && !hasTriggered)
            {
                base.OnPicked(player);
            }
        }
    }
}
