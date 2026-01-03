using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Unity.FPS.Hex
{
    public class HexEffectToast : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Text content that will display the hex effect name")]
        public TextMeshProUGUI TextContent;
        
        [Tooltip("Canvas used to fade in the content")]
        public CanvasGroup CanvasGroup;
        
        [Tooltip("Background image (optional)")]
        public Image BackgroundImage;

        [Header("Animation")]
        [Tooltip("Duration of the fade in")]
        public float FadeInDuration = 0.5f;
        
        [Tooltip("Color for the text (optional, can be set in prefab)")]
        public Color TextColor = Color.white;

        bool m_Initialized;
        float m_InitTime;

        public void Initialize(string hexName)
        {
            if (TextContent != null)
            {
                TextContent.text = hexName;
                TextContent.color = TextColor;
            }
            
            m_InitTime = Time.time;
            m_Initialized = true;
            
            // 开始时透明
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = 0f;
            }
        }

        void Update()
        {
            if (m_Initialized && CanvasGroup != null)
            {
                float timeSinceInit = Time.time - m_InitTime;
                
                if (timeSinceInit < FadeInDuration)
                {
                    // 淡入效果
                    CanvasGroup.alpha = timeSinceInit / FadeInDuration;
                }
                else
                {
                    // 完全显示
                    CanvasGroup.alpha = 1f;
                    m_Initialized = false; // 停止更新
                }
            }
        }
    }
}

