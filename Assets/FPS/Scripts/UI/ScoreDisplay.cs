using Unity.FPS.Game;
using UnityEngine;
using TMPro;

namespace Unity.FPS.UI
{
    /// <summary>
    /// UI组件，显示玩家的当前分数
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        [Header("UI引用")]
        [Tooltip("显示分数的TextMeshPro组件")]
        public TextMeshProUGUI ScoreText;

        [Header("显示设置")]
        [Tooltip("分数文本前缀（例如：'Score: '）")]
        public string ScorePrefix = "Score: ";

        [Tooltip("是否在获得分数时播放动画")]
        public bool PlayAnimationOnScoreChange = true;

        [Tooltip("分数增加时的缩放动画持续时间")]
        public float ScaleAnimationDuration = 0.3f;

        [Tooltip("分数增加时的最大缩放比例")]
        public float ScaleMultiplier = 1.2f;

        private int m_CurrentDisplayedScore = 0;
        private float m_AnimationTimer = 0f;
        private Vector3 m_OriginalScale;
        private ScoreManager m_ScoreManager;

        void Awake()
        {
            if (ScoreText == null)
            {
                ScoreText = GetComponent<TextMeshProUGUI>();
                if (ScoreText == null)
                {
                    Debug.LogError("ScoreDisplay: 未找到TextMeshProUGUI组件！请在Inspector中指定ScoreText。");
                }
            }

            if (ScoreText != null)
            {
                m_OriginalScale = ScoreText.transform.localScale;
            }
        }

        void Start()
        {
            m_ScoreManager = FindObjectOfType<ScoreManager>();
            if (m_ScoreManager == null)
            {
                Debug.LogWarning("ScoreDisplay: 场景中未找到ScoreManager，分数显示可能无法正常工作。");
            }

            UpdateScoreDisplay(0);
        }

        void OnEnable()
        {
            EventManager.AddListener<ScoreChangedEvent>(OnScoreChanged);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<ScoreChangedEvent>(OnScoreChanged);
        }

        void Update()
        {
            // 处理缩放动画
            if (m_AnimationTimer > 0f)
            {
                m_AnimationTimer -= Time.deltaTime;
                float progress = 1f - (m_AnimationTimer / ScaleAnimationDuration);
                
                // 使用Sin曲线实现弹性效果
                float scale = 1f + (ScaleMultiplier - 1f) * Mathf.Sin(progress * Mathf.PI);
                
                if (ScoreText != null)
                {
                    ScoreText.transform.localScale = m_OriginalScale * scale;
                }

                if (m_AnimationTimer <= 0f && ScoreText != null)
                {
                    ScoreText.transform.localScale = m_OriginalScale;
                }
            }
        }

        void OnScoreChanged(ScoreChangedEvent evt)
        {
            UpdateScoreDisplay(evt.TotalScore);

            // 如果分数增加了，播放动画
            if (PlayAnimationOnScoreChange && evt.ScoreAdded > 0)
            {
                m_AnimationTimer = ScaleAnimationDuration;
            }
        }

        void UpdateScoreDisplay(int score)
        {
            m_CurrentDisplayedScore = score;

            if (ScoreText != null)
            {
                ScoreText.text = ScorePrefix + m_CurrentDisplayedScore.ToString();
            }
        }

        /// <summary>
        /// 手动刷新分数显示（用于初始化或调试）
        /// </summary>
        public void RefreshDisplay()
        {
            if (m_ScoreManager != null)
            {
                UpdateScoreDisplay(m_ScoreManager.TotalScore);
            }
        }
    }
}

