using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 管理玩家分数的系统
    /// 监听敌人击杀事件，根据敌人类型给予不同分数
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("分数设置")]
        [Tooltip("击杀Enemy_turret获得的分数")]
        public int ScoreForTurret = 10;

        [Tooltip("击杀Enemy_hoverbot获得的分数")]
        public int ScoreForHoverbot = 2;

        [Tooltip("默认击杀分数（用于其他敌人类型）")]
        public int DefaultKillScore = 5;

        private int m_TotalScore = 0;

        public int TotalScore => m_TotalScore;

        void OnEnable()
        {
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (evt.Enemy == null)
                return;

            int scoreToAdd = GetScoreForEnemy(evt.Enemy);
            AddScore(scoreToAdd);
        }

        /// <summary>
        /// 根据敌人名称判断给予的分数
        /// </summary>
        int GetScoreForEnemy(GameObject enemy)
        {
            // 通过GameObject名称判断敌人类型
            string enemyName = enemy.name.ToLower();
            
            // 移除Unity自动添加的(Clone)后缀
            enemyName = enemyName.Replace("(clone)", "").Trim();

            // 判断是否为炮塔类型
            if (enemyName.Contains("turret"))
            {
                return ScoreForTurret;
            }
            
            // 判断是否为飞行机器人类型
            if (enemyName.Contains("hoverbot") || enemyName.Contains("mobile") || enemyName.Contains("hover"))
            {
                return ScoreForHoverbot;
            }

            // 默认分数
            return DefaultKillScore;
        }

        /// <summary>
        /// 添加分数并广播事件
        /// </summary>
        public void AddScore(int score)
        {
            m_TotalScore += score;

            // 广播分数变化事件
            ScoreChangedEvent evt = Events.ScoreChangedEvent;
            evt.TotalScore = m_TotalScore;
            evt.ScoreAdded = score;
            EventManager.Broadcast(evt);
        }

        /// <summary>
        /// 重置分数（用于重新开始游戏）
        /// </summary>
        public void ResetScore()
        {
            m_TotalScore = 0;

            // 广播分数变化事件
            ScoreChangedEvent evt = Events.ScoreChangedEvent;
            evt.TotalScore = m_TotalScore;
            evt.ScoreAdded = 0;
            EventManager.Broadcast(evt);
        }
    }
}

