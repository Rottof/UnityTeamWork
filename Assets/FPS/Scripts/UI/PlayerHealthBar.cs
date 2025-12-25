using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [Tooltip("Image component dispplaying current health")]
        public Image HealthFillImage;

        [Tooltip("HealthIcon RectTransform component to scale")]
        public RectTransform HealthIcon;

        Health m_PlayerHealth;
        RectTransform m_HealthBarRectTransform;
        float m_InitialMaxHealth;
        Vector2 m_InitialBarSize;

        void Start()
        {
            PlayerCharacterController playerCharacterController =
                GameObject.FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PlayerHealthBar>(
                playerCharacterController, this);

            m_PlayerHealth = playerCharacterController.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerHealthBar>(m_PlayerHealth, this,
                playerCharacterController.gameObject);

            // 保存初始的最大生命值和血条尺寸
            m_InitialMaxHealth = m_PlayerHealth.MaxHealth;
            
            // 如果手动指定了HealthIcon，使用它；否则使用HealthFillImage
            if (HealthIcon != null)
            {
                m_HealthBarRectTransform = HealthIcon;
            }
            else
            {
                m_HealthBarRectTransform = HealthFillImage.GetComponent<RectTransform>();
            }
            
            if (m_HealthBarRectTransform != null)
            {
                m_InitialBarSize = m_HealthBarRectTransform.sizeDelta;
            }
        }

        void Update()
        {
            // update health bar value
            HealthFillImage.fillAmount = m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth;

            // 根据最大生命值的变化调整血条宽度
            if (m_HealthBarRectTransform != null && m_InitialMaxHealth > 0)
            {
                float healthRatio = m_PlayerHealth.MaxHealth / m_InitialMaxHealth;
                m_HealthBarRectTransform.sizeDelta = new Vector2(
                    m_InitialBarSize.x * healthRatio,
                    m_InitialBarSize.y
                );
            }
        }
    }
}