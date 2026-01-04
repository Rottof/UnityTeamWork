using Unity.FPS.Game;
using Unity.FPS.Hex;
using UnityEngine;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Hex
{
    public class HexPickup : Pickup
    {
        [Header("Hex Panel Settings")]
        [Tooltip("可选：手动指定海克斯面板对象。如果为空，将自动在场景中查找HexPanel组件（包括隐藏的）")]
        public GameObject hexPanelObject;

        private bool hasTriggered = false;
        
        /// <summary>
        /// 查找场景中的HexPanel（包括inactive的对象）
        /// </summary>
        private HexPanel FindHexPanel()
        {
            // 先尝试找active的
            HexPanel hexPanel = FindObjectOfType<HexPanel>();
            
            // 如果没找到，尝试找所有的（包括inactive的）
            if (hexPanel == null)
            {
                HexPanel[] allPanels = Resources.FindObjectsOfTypeAll<HexPanel>();
                foreach (HexPanel panel in allPanels)
                {
                    // 排除prefab资源，只要场景中的实例
                    if (panel.gameObject.scene.name != null)
                    {
                        hexPanel = panel;
                        break;
                    }
                }
            }
            
            return hexPanel;
        }
        
        //触发海克斯科技面板
        private void OnTriggerEnter(Collider other)
        {
            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();
            
            if (pickingPlayer != null && !hasTriggered)
            {
                hasTriggered = true; 

                Debug.Log($"[HexPickup] 玩家触发了海克斯拾取物！");

                // 在真正需要时才查找HexPanel（延迟查找）
                HexPanel hexPanel = null;
                
                if (hexPanelObject != null)
                {
                    hexPanel = hexPanelObject.GetComponent<HexPanel>();
                }
                else
                {
                    // 自动查找场景中的HexPanel（包括隐藏的）
                    hexPanel = FindHexPanel();
                    if (hexPanel != null)
                    {
                        Debug.Log($"[HexPickup] 自动找到海克斯面板: {hexPanel.gameObject.name}");
                    }
                }
                
                if (hexPanel != null)
                {
                    Debug.Log($"[HexPickup] 找到HexPanel组件，准备显示面板");
                    
                    // 显示海克斯面板（会自动暂停游戏并随机选择3个效果）
                    hexPanel.ShowPanel();
                    
                    Debug.Log("[HexPickup] 海克斯面板已显示，游戏已暂停！");
                    
                    // Play pickup feedback and destroy the object
                    PlayPickupFeedback();
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning("[HexPickup] 未找到HexPanel！请确保场景中有HexPanel组件。");
                    
                    // If no hex panel is found, use default pickup behavior
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

