using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour
{
    [SerializeField] GameObject eventPanel;
    [SerializeField] Button Button1;
    [SerializeField] Button Button2;
    [SerializeField] Button Button3;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text countdownText;


    private GameObject triggerObject;

    void Start()
    {
        if (eventPanel == null) eventPanel = gameObject;
        eventPanel.SetActive(false); 

        if (Button1 != null)
        {
            Button1.onClick.RemoveAllListeners();
            Button1.onClick.AddListener(OnHealthUp);
        }
        if (Button2 != null)
        {
            Button2.onClick.RemoveAllListeners();
            Button2.onClick.AddListener(OnRandomEffect);
        }
        if (Button3 != null)
        {
            Button3.onClick.RemoveAllListeners();
            Button3.onClick.AddListener(OnSpeedUp);
        }
        
        // 初始时隐藏倒计时文本
        if (countdownText != null) countdownText.text = "";
    }    

    void OnHealthUp()
    {
        if (resultText != null) resultText.text = "Your health + 20";
    }

    void OnRandomEffect()
    {
        int r = Random.Range(0, 4);
        if (r == 0)
        {
            if (resultText != null) resultText.text = "Your fire rate + 10%";
        }
        else if (r == 1)
        {
            if (resultText != null) resultText.text = "Your health - 5";
        }
        else if (r == 2)
        {
            if (resultText != null) resultText.text = "Your fire: scatter 3-way";
        }
        else
        {
            if (resultText != null) resultText.text = "Your fire rate + 10%";
        }
    }

    void OnSpeedUp()
    {
        if (resultText != null) resultText.text = "Your speed + 10%";
    }

    void ClosePanel()
    {
        if (eventPanel != null) eventPanel.SetActive(false);
        Time.timeScale = 1f;
        if (countdownText != null) countdownText.text = "";
        
        // 销毁触发物
        if (triggerObject != null)
        {
            Destroy(triggerObject);
        }
    }
    
    public void SetTriggerObject(GameObject trigger)
    {
        triggerObject = trigger;
    }
    
    void Update()
    {
        // 检测ESC键来关闭面板
        if (Input.GetKeyDown(KeyCode.Escape) && eventPanel != null && eventPanel.activeSelf)
        {
            ClosePanel();
        }
    }


}