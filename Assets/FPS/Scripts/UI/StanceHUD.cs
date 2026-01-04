using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class StanceHUD : MonoBehaviour
    {
        [Tooltip("Image component for the stance sprites")]
        public Image StanceImage;

        [Tooltip("Sprite to display when standing")]
        public Sprite StandingSprite;

        [Tooltip("Sprite to display when crouching")]
        public Sprite CrouchingSprite;

        [Header("Speed Boost Effect")]
        [Tooltip("Background color when speed boost is active")]
        public Color SpeedBoostColor = new Color(0f, 1f, 0.835f, 1f); // 青绿色 (0, 255, 213, 255)

        private Color originalColor;

        void Start()
        {
            PlayerCharacterController character = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, StanceHUD>(character, this);
            character.OnStanceChanged += OnStanceChanged;

            OnStanceChanged(character.IsCrouching);

            // 保存原始颜色
            if (StanceImage != null)
            {
                originalColor = StanceImage.color;
                Debug.Log($"StanceHUD 初始化：原始颜色 = {originalColor}, 速度提升颜色 = {SpeedBoostColor}");
            }
            else
            {
                Debug.LogError("StanceHUD: StanceImage 未分配！请在 Inspector 中指定。");
            }

            // 监听速度提升事件
            EventManager.AddListener<SpeedBoostEvent>(OnSpeedBoostEvent);
        }

        void OnDestroy()
        {
            // 清理事件监听器
            EventManager.RemoveListener<SpeedBoostEvent>(OnSpeedBoostEvent);
        }

        void OnSpeedBoostEvent(SpeedBoostEvent evt)
        {
            SetSpeedBoostEffect(evt.IsActive);
        }

        void OnStanceChanged(bool crouched)
        {
            StanceImage.sprite = crouched ? CrouchingSprite : StandingSprite;
        }

        /// <summary>
        /// 设置速度提升效果的视觉反馈
        /// </summary>
        /// <param name="active">是否激活速度提升效果</param>
        public void SetSpeedBoostEffect(bool active)
        {
            if (StanceImage != null)
            {
                Color targetColor = active ? SpeedBoostColor : originalColor;
                StanceImage.color = targetColor;
                Debug.Log($"StanceHUD: 速度提升效果 {(active ? "激活" : "结束")}，颜色: {targetColor}");
            }
            else
            {
                Debug.LogWarning("StanceHUD: StanceImage 为空！");
            }
        }
    }
}