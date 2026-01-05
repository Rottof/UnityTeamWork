using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveSurviveTime : Objective
    {
        [Tooltip("存活所需的时间（秒）")]
        public float SurvivalTimeInSeconds = 600f; // 10分钟 = 600秒

        [Tooltip("开始倒计时前的延迟时间（秒）")]
        public float StartDelay = 0f;

        [Tooltip("完成目标时获得的分数")]
        public int CompletionScore = 500;

        float m_RemainingTime;
        bool m_HasStarted = false;
        float m_StartTime;
        int m_LastDisplayedSeconds = -1;

        void Awake()
        {
            // 在Awake中设置，确保在Start之前就准备好
            m_RemainingTime = SurvivalTimeInSeconds;

            // 设置标题和描述
            if (string.IsNullOrEmpty(Title))
                Title = "Survive for 10 minutes";

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounterAmount();
        }

        protected override void Start()
        {
            // 如果有延迟，记录开始时间
            m_StartTime = Time.time + StartDelay;

            // 调用base.Start()会触发UI创建
            base.Start();
        }

        void Update()
        {
            // 如果任务已完成，不再更新
            if (IsCompleted)
                return;

            // 等待延迟时间结束
            if (!m_HasStarted)
            {
                if (Time.time >= m_StartTime)
                {
                    m_HasStarted = true;
                }
                else
                {
                    return;
                }
            }

            // 更新倒计时
            m_RemainingTime -= Time.deltaTime;

            // 获取当前的总秒数
            int currentSeconds = Mathf.CeilToInt(m_RemainingTime);

            // 检查是否完成任务
            if (m_RemainingTime <= 0f)
            {
                m_RemainingTime = 0f;
                
                // 添加完成分数
                ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddScore(CompletionScore);
                }
                
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete: " + Title);
                return;
            }

            // 只在秒数变化时更新UI
            if (currentSeconds != m_LastDisplayedSeconds)
            {
                m_LastDisplayedSeconds = currentSeconds;

                // 根据剩余时间显示不同的通知
                string notificationText = string.Empty;

                if (currentSeconds <= 10)
                {
                    notificationText = currentSeconds + " seconds left!";
                }
                else if (currentSeconds == 30)
                {
                    notificationText = "30 seconds left!";
                }
                else if (currentSeconds == 60)
                {
                    notificationText = "1 minute left!";
                }

                // 更新Counter来显示倒计时（与ObjectiveKillEnemies保持一致）
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }

        string GetUpdatedCounterAmount()
        {
            return FormatTime(m_RemainingTime);
        }

        string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return string.Format("{0}:{1:00}", minutes, seconds);
        }
    }
}

