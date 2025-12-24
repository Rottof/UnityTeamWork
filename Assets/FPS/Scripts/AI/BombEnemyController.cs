// csharp
using UnityEngine;
using UnityEngine.AI;
using Unity.FPS.Game;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class BombEnemyController : EnemyController
    {
        [Header("Explosion")]
        public float ExplosionRadius = 4f;
        public float ExplosionDamage = 80f;
        public GameObject ExplosionVFX;

        [Tooltip("必须接近目标触发自爆（米）")]
        public float ExplosionTriggerDistance = 1.5f;

        bool m_HasExploded = false;
        
        public override bool TryAtack(Vector3 enemyPosition)
        {
            if (m_HasExploded)
                return false;

            float sqrDist = (transform.position - enemyPosition).sqrMagnitude;
            float triggerSqr = ExplosionTriggerDistance * ExplosionTriggerDistance;

            if (sqrDist <= triggerSqr)
            {
                Explode();
                return true;
            }

            return false;
        }

        void Explode()
        {
            m_HasExploded = true;

            // 触发攻击事件（让 DetectionModule / 动画知道）
            onAttack?.Invoke();

            // 爆炸特效
            if (ExplosionVFX)
            {
                Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
            }

            // 范围伤害
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                ExplosionRadius,
                -1,
                QueryTriggerInteraction.Ignore
            );

            foreach (var hit in hits)
            {
                Health health = hit.GetComponent<Health>();
                if (health != null && health.gameObject != gameObject)
                {
                    health.TakeDamage(ExplosionDamage, gameObject);
                }
            }

            // 自杀
            GetComponent<Health>().Kill();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ExplosionRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, ExplosionTriggerDistance);
        }
    }
}
