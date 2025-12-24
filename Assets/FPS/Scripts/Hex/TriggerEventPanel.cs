using UnityEngine;

public class TriggerEventPanel : MonoBehaviour
{
    // 1. 删除之前的 public GameObject eventPanelPrefab;
    // 2. 改为直接引用场景中的面板实例
    public GameObject eventPanel; 

    private bool hasTriggered = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true; 

            // 3. 核心逻辑：将面板设置为 Active (可见)
            if (eventPanel != null)
            {
                // 设置为 true 即显示
                eventPanel.SetActive(true); 
                Time.timeScale = 0; 
                Debug.Log("面板已显示！");
            }
            else
            {
                Debug.LogError("请在 Inspector 中赋值 Event Panel！");
            }
        }
    }
}