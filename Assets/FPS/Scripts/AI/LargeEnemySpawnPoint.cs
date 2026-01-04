using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 大型怪物刷新点
    /// 负责管理固定点位的大型怪物刷新
    /// </summary>
    public class LargeEnemySpawnPoint : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("是否激活该刷新点")]
        public bool isActive = true;

        [Tooltip("显示刷新点范围")]
        public bool showGizmo = true;

        [Tooltip("刷新点半径（仅用于可视化）")]
        public float gizmoRadius = 2f;

        [Header("状态")]
        [Tooltip("当前刷新点的大型怪引用")]
        public GameObject currentEnemy;

        /// <summary>
        /// 检查该刷新点是否已有大型怪
        /// </summary>
        public bool HasLargeEnemy()
        {
            // 清理已销毁的引用
            if (currentEnemy == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 注册新刷新的怪物
        /// </summary>
        public void RegisterEnemy(GameObject enemy)
        {
            currentEnemy = enemy;

            // 监听怪物死亡事件
            var health = enemy.GetComponent<Unity.FPS.Game.Health>();
            if (health != null)
            {
                health.OnDie += OnEnemyDied;
            }
        }

        /// <summary>
        /// 怪物死亡回调
        /// </summary>
        void OnEnemyDied()
        {
            currentEnemy = null;
        }

        /// <summary>
        /// 手动清空刷新点
        /// </summary>
        [ContextMenu("清空刷新点")]
        public void ClearSpawnPoint()
        {
            if (currentEnemy != null)
            {
                Destroy(currentEnemy);
                currentEnemy = null;
            }
        }

        void OnDrawGizmos()
        {
            if (!showGizmo)
                return;

            // 根据是否有怪物改变颜色
            Gizmos.color = HasLargeEnemy() ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius);

            // 绘制一个箭头指示方向
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * gizmoRadius);
        }

        void OnDrawGizmosSelected()
        {
            // 选中时绘制更明显的标记
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius * 1.2f);
        }
    }
}

