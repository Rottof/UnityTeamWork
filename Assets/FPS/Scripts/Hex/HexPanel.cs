using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Unity.FPS.Hex
{
    public class HexPanel : MonoBehaviour
    {
        [Header("Hex卡片组件")]
        [SerializeField] private GameObject hex1;
        [SerializeField] private GameObject hex2;
        [SerializeField] private GameObject hex3;

        [Header("Hex卡片文本组件")]
        [SerializeField] private TMP_Text hex1Text1; // Hex1的名称文本
        [SerializeField] private TMP_Text hex1Text2; // Hex1的描述文本
        [SerializeField] private TMP_Text hex2Text1; // Hex2的名称文本
        [SerializeField] private TMP_Text hex2Text2; // Hex2的描述文本
        [SerializeField] private TMP_Text hex3Text1; // Hex3的名称文本
        [SerializeField] private TMP_Text hex3Text2; // Hex3的描述文本

        [Header("Hex卡片按钮组件")]
        [SerializeField] private Button hex1Button;
        [SerializeField] private Button hex2Button;
        [SerializeField] private Button hex3Button;

        [Header("面板设置")]
        [SerializeField] private GameObject hexPanel;

        // 引用HexEffects组件
        private HexEffects hexEffects;

        // 当前显示的3个海克斯效果
        private List<HexData> currentHexEffects = new List<HexData>();

        void Start()
        {
            // 获取或添加HexEffects组件
            hexEffects = GetComponent<HexEffects>();
            if (hexEffects == null)
            {
                hexEffects = gameObject.AddComponent<HexEffects>();
            }

            // 初始化按钮事件
            if (hex1Button != null)
            {
                hex1Button.onClick.RemoveAllListeners();
                hex1Button.onClick.AddListener(() => OnHexButtonClicked(0));
            }

            if (hex2Button != null)
            {
                hex2Button.onClick.RemoveAllListeners();
                hex2Button.onClick.AddListener(() => OnHexButtonClicked(1));
            }

            if (hex3Button != null)
            {
                hex3Button.onClick.RemoveAllListeners();
                hex3Button.onClick.AddListener(() => OnHexButtonClicked(2));
            }

            // 初始时隐藏面板
            if (hexPanel != null)
            {
                hexPanel.SetActive(false);
            }
        }

        // 显示面板并随机选择3个海克斯效果
        public void ShowPanel()
        {
            if (hexPanel != null)
            {
                hexPanel.SetActive(true);
            }

            // 暂停游戏
            Time.timeScale = 0f;

            // 随机选择3个海克斯效果
            RefreshHexCards();
        }

        // 刷新海克斯卡片
        public void RefreshHexCards()
        {
            if (hexEffects == null) return;

            // 获取3个随机的海克斯效果
            currentHexEffects = hexEffects.GetRandomHexEffects(3);

            // 为每张卡片分配海克斯名称和描述
            if (currentHexEffects.Count > 0)
            {
                if (hex1Text1 != null) hex1Text1.text = currentHexEffects[0].hexName;
                if (hex1Text2 != null) hex1Text2.text = currentHexEffects[0].hexDescription;
            }

            if (currentHexEffects.Count > 1)
            {
                if (hex2Text1 != null) hex2Text1.text = currentHexEffects[1].hexName;
                if (hex2Text2 != null) hex2Text2.text = currentHexEffects[1].hexDescription;
            }

            if (currentHexEffects.Count > 2)
            {
                if (hex3Text1 != null) hex3Text1.text = currentHexEffects[2].hexName;
                if (hex3Text2 != null) hex3Text2.text = currentHexEffects[2].hexDescription;
            }
        }

        // 点击海克斯按钮时触发
        void OnHexButtonClicked(int index)
        {
            if (index >= 0 && index < currentHexEffects.Count)
            {
                // 应用海克斯效果并在UI上显示
                if (hexEffects != null)
                {
                    hexEffects.ApplyHexEffect(currentHexEffects[index]);
                }

                // 关闭面板
                ClosePanel();
            }
        }

        // 关闭面板
        public void ClosePanel()
        {
            if (hexPanel != null)
            {
                hexPanel.SetActive(false);
            }

            // 恢复游戏
            Time.timeScale = 1f;
        }

        void Update()
        {
            // 如果面板激活，按ESC键关闭
            if (hexPanel != null && hexPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ClosePanel();
                }

                // 快捷键1、2、3选择对应的海克斯
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                {
                    if (hex1Button != null)
                    {
                        hex1Button.onClick.Invoke();
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    if (hex2Button != null)
                    {
                        hex2Button.onClick.Invoke();
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                {
                    if (hex3Button != null)
                    {
                        hex3Button.onClick.Invoke();
                    }
                }
            }
        }
    }
}
