using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillEnemies : Objective
    {
        [Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
        public bool MustKillAllEnemies = true;

        [Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
        public int KillsToCompleteObjective = 5;

        [Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
        public int NotificationEnemiesRemainingThreshold = 3;

        [Tooltip("完成目标时获得的分数")]
        public int CompletionScore = 100;

        [Tooltip("只统计包含此关键字的敌人（留空则统计所有敌人）")]
        public string TargetEnemyKeyword = "turret";

        int m_KillTotal;
        int m_TotalTargetEnemies = 0;

        protected override void Start()
        {
            // 如果需要击杀所有目标敌人，先统计数量
            if (MustKillAllEnemies)
            {
                m_TotalTargetEnemies = CountTargetEnemiesInScene();
                KillsToCompleteObjective = m_TotalTargetEnemies;
            }

            // set a title and description specific for this type of objective, if it hasn't one
            string originalTitle = Title;
            if (string.IsNullOrEmpty(Title))
            {
                string enemyType = !string.IsNullOrEmpty(TargetEnemyKeyword) ? TargetEnemyKeyword + " " : "";
                originalTitle = "Eliminate " + (MustKillAllEnemies ? "all the " : KillsToCompleteObjective.ToString() + " ") +
                        enemyType + "enemies";
                Title = originalTitle;
            }

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounterAmount();

            // 临时清空Title，避免在屏幕中央显示消息
            Title = string.Empty;
            
            // 调用base.Start()来触发OnObjectiveCreated事件
            base.Start();
            
            // 恢复Title，这样目标列表中会显示正确的标题
            Title = originalTitle;

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted)
                return;

            // 如果设置了目标敌人关键字，检查是否匹配
            if (!string.IsNullOrEmpty(TargetEnemyKeyword))
            {
                if (evt.Enemy == null)
                    return;

                string enemyName = evt.Enemy.name.ToLower();
                enemyName = enemyName.Replace("(clone)", "").Trim();

                // 如果不包含目标关键字，不统计
                if (!enemyName.Contains(TargetEnemyKeyword.ToLower()))
                    return;
            }

            m_KillTotal++;

            if (MustKillAllEnemies)
            {
                // 只统计目标类型的敌人总数
                if (m_TotalTargetEnemies == 0)
                {
                    m_TotalTargetEnemies = CountTargetEnemiesInScene();
                }
                KillsToCompleteObjective = m_TotalTargetEnemies;
            }

            int targetRemaining = KillsToCompleteObjective - m_KillTotal;

            // update the objective text according to how many enemies remain to kill
            if (targetRemaining == 0)
            {
                // 添加完成分数
                ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddScore(CompletionScore);
                }
                
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete : " + Title);
            }
            else if (targetRemaining == 1)
            {
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? "One enemy left"
                    : string.Empty;
                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
            else
            {
                // create a notification text if needed, if it stays empty, the notification will not be created
                string notificationText = NotificationEnemiesRemainingThreshold >= targetRemaining
                    ? targetRemaining + " enemies to kill left"
                    : string.Empty;

                UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
            }
        }

        string GetUpdatedCounterAmount()
        {
            return m_KillTotal + " / " + KillsToCompleteObjective;
        }

        int CountTargetEnemiesInScene()
        {
            // 统计场景中包含目标关键字的敌人数量
            // 查找所有包含Health和Actor组件的GameObject（这是敌人的标志）
            var healthComponents = FindObjectsOfType<Health>();
            int count = 0;
            
            foreach (var health in healthComponents)
            {
                // 确保这是一个敌人（通过Actor组件和Affiliation判断）
                var actor = health.GetComponent<Actor>();
                if (actor != null && actor.Affiliation != 0) // 0通常是玩家，其他值是敌人
                {
                    string enemyName = health.gameObject.name.ToLower();
                    enemyName = enemyName.Replace("(clone)", "").Trim();
                    
                    // 如果没有指定关键字，或者包含目标关键字，就计数
                    if (string.IsNullOrEmpty(TargetEnemyKeyword) || 
                        enemyName.Contains(TargetEnemyKeyword.ToLower()))
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
    }
}