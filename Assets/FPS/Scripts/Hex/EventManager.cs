using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using Unity.FPS.Hex;

public class EventManager : MonoBehaviour
{
    [SerializeField] GameObject eventPanel;
    [SerializeField] Button Button1;
    [SerializeField] Button Button2;
    [SerializeField] Button Button3;
    [SerializeField] TMP_Text countdownText;
    
    // 引用HexEffects组件
    HexEffects hexEffects;


    private GameObject triggerObject;

    void Start()
    {
        if (eventPanel == null) eventPanel = gameObject;
        eventPanel.SetActive(false); 
        
        // 获取或添加HexEffects组件
        hexEffects = GetComponent<HexEffects>();
        if (hexEffects == null)
        {
            hexEffects = gameObject.AddComponent<HexEffects>();
        }
        


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
            Button3.onClick.AddListener(hexEffects.OnMultiShot);
        }
        
        // 初始时隐藏倒计时文本
        if (countdownText != null) countdownText.text = "";
    }    

    void OnHealthUp()
    {
        if (hexEffects != null)
        {
            hexEffects.OnHealthUp();
        }
        ClosePanel();
    }

    void OnRandomEffect()
    {
        // 随机选择一个效果
        int r = Random.Range(2, 8); // 从效果2到7中随机选择
        
        switch(r)
        {
            case 2:
                if (hexEffects != null)
                {
                    hexEffects.OnAttackUp();
                }
                break;
            case 3:
                if (hexEffects != null)
                {
                    hexEffects.OnSpeedUp();
                }
                break;
            case 4:
                if (hexEffects != null)
                {
                    hexEffects.OnHeartOfSteel();
                }
                break;
            case 5:
                if (hexEffects != null)
                {
                    hexEffects.OnSwiftFootwork();
                }
                break;
            case 6:
                if (hexEffects != null)
                {
                    hexEffects.OnMultiShot();
                }
                break;
            case 7:
                if (hexEffects != null)
                {
                    hexEffects.OnLifeSource();
                }
                break;
        }
        ClosePanel();
    }

    void OnSpeedUp()
    {
        if (hexEffects != null)
        {
            hexEffects.OnSpeedUp();
        }
        ClosePanel();
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
        if (eventPanel != null && eventPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePanel();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                if (Button1 != null)
                {
                    Button1.onClick.Invoke();
                }
                else
                {
                    OnHealthUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                if (Button2 != null)
                {
                    Button2.onClick.Invoke();
                }
                else
                {
                    OnRandomEffect();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                if (Button3 != null)
                {
                    Button3.onClick.Invoke();
                }
                else
                {
                    OnSpeedUp();
                }
            }
        }
    }


}
