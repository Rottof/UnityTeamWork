using UnityEngine;

public class TriggerEventPanel : MonoBehaviour
{
    public GameObject eventPanel; 

    private bool hasTriggered = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
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
            }
            else
            {
                Debug.LogError("请在 Inspector 中赋值 Event Panel！");
            }
        }
    }
}