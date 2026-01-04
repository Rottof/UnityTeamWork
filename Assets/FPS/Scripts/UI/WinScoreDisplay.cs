using UnityEngine;
using TMPro;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 在Win场景中显示玩家的最终分数
    /// 从PlayerPrefs读取保存的分数
    /// </summary>
    public class WinScoreDisplay : MonoBehaviour
    {
        [Header("UI引用")]
        [Tooltip("显示分数的TextMeshPro组件")]
        public TextMeshProUGUI ScoreText;

        [Header("显示设置")]
        [Tooltip("分数文本前缀（例如：'最终得分: '）")]
        public string ScorePrefix = "最终得分: ";

        [Tooltip("如果没有保存的分数，显示的默认分数")]
        public int DefaultScore = 0;

        [Header("动画设置")]
        [Tooltip("是否启用数字滚动动画")]
        public bool EnableCountAnimation = true;

        [Tooltip("数字滚动动画的持续时间（秒）")]
        public float CountAnimationDuration = 1.5f;

        [Tooltip("动画曲线，控制数字滚动的速度")]
        public AnimationCurve CountAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private int m_FinalScore;
        private float m_AnimationTimer;
        private bool m_IsAnimating;

        void Start()
        {
            // 从PlayerPrefs读取保存的分数
            m_FinalScore = PlayerPrefs.GetInt("LastGameScore", DefaultScore);

            if (ScoreText == null)
            {
                ScoreText = GetComponent<TextMeshProUGUI>();
                if (ScoreText == null)
                {
                    Debug.LogError("WinScoreDisplay: 未找到TextMeshProUGUI组件！请在Inspector中指定ScoreText。");
                    return;
                }
            }

            // 开始动画
            if (EnableCountAnimation && m_FinalScore > 0)
            {
                m_IsAnimating = true;
                m_AnimationTimer = 0f;
                UpdateScoreDisplay(0);
            }
            else
            {
                UpdateScoreDisplay(m_FinalScore);
            }
        }

        void Update()
        {
            if (m_IsAnimating)
            {
                m_AnimationTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(m_AnimationTimer / CountAnimationDuration);
                float curvedProgress = CountAnimationCurve.Evaluate(progress);
                
                int currentDisplayScore = Mathf.RoundToInt(m_FinalScore * curvedProgress);
                UpdateScoreDisplay(currentDisplayScore);

                if (progress >= 1f)
                {
                    m_IsAnimating = false;
                    UpdateScoreDisplay(m_FinalScore);
                }
            }
        }

        void UpdateScoreDisplay(int score)
        {
            if (ScoreText != null)
            {
                ScoreText.text = ScorePrefix + score.ToString();
            }
        }

        /// <summary>
        /// 手动设置显示的分数（用于调试或特殊情况）
        /// </summary>
        public void SetScore(int score)
        {
            m_FinalScore = score;
            m_IsAnimating = false;
            UpdateScoreDisplay(score);
        }

        /// <summary>
        /// 重新开始动画
        /// </summary>
        public void RestartAnimation()
        {
            if (EnableCountAnimation && m_FinalScore > 0)
            {
                m_IsAnimating = true;
                m_AnimationTimer = 0f;
                UpdateScoreDisplay(0);
            }
        }
    }
}

