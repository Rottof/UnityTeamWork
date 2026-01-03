using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 可倒塌的柱子控制器
    /// 需要配合Health和Damageable组件使用
    /// </summary>
    public class PillarController : MonoBehaviour
    {
        [Header("倒塌设置")]
        [Tooltip("柱子的刚体组件（如果为空会自动获取）")]
        public Rigidbody pillarRigidbody;

        [Tooltip("触发倒塌所需的最小伤害阈值")]
        public float damageThreshold = 50f;

        [Tooltip("倒塌时施加的力度倍数")]
        public float collapseForceMultiplier = 2f;

        [Tooltip("倒塌时施加力的方向（相对于击中点）")]
        public Vector3 collapseForceDirection = Vector3.forward;

        [Tooltip("是否根据击中方向自动计算倒塌方向")]
        public bool useHitDirection = true;

        [Tooltip("倒塌后多久销毁柱子（0表示不销毁）")]
        public float destroyAfterCollapse = 5f;

        [Tooltip("向下的额外重力倍数（防止浮空）")]
        public float additionalDownwardForce = 2f;

        [Tooltip("强制只在水平方向倒塌（不会飞起）")]
        public bool keepGrounded = true;

        [Header("视觉效果")]
        [Tooltip("倒塌时播放的音效")]
        public AudioClip collapseSound;

        [Tooltip("倒塌时生成的粒子效果")]
        public GameObject collapseVFX;

        [Header("调试")]
        [Tooltip("显示调试信息")]
        public bool showDebugInfo = false;

        private Health health;
        private bool hasCollapsed = false;
        private float accumulatedDamage = 0f;
        private Vector3 lastHitPoint;
        private GameObject lastDamageSource;

        void Start()
        {
            // 获取或添加Rigidbody
            if (pillarRigidbody == null)
            {
                pillarRigidbody = GetComponent<Rigidbody>();
                if (pillarRigidbody == null)
                {
                    pillarRigidbody = gameObject.AddComponent<Rigidbody>();
                }
            }

            // 初始化时设置为运动学模式（不受物理影响）
            pillarRigidbody.isKinematic = true;
            pillarRigidbody.useGravity = false;
            
            // 设置一些物理属性，确保倒塌时不会异常飞起
            if (keepGrounded)
            {
                // 增加质量可以减少浮空的可能
                if (pillarRigidbody.mass < 10f)
                {
                    pillarRigidbody.mass = Mathf.Max(pillarRigidbody.mass, 30f);
                }
                // 增加阻力
                pillarRigidbody.drag = Mathf.Max(pillarRigidbody.drag, 0.5f);
                pillarRigidbody.angularDrag = Mathf.Max(pillarRigidbody.angularDrag, 0.5f);
            }

            // 获取Health组件
            health = GetComponent<Health>();
            if (health == null)
            {
                Debug.LogWarning($"[PillarController] {gameObject.name} 没有Health组件！柱子将无法接收伤害。");
            }
            else
            {
                // 监听伤害事件
                health.OnDamaged += OnPillarDamaged;
                health.OnDie += OnPillarDestroyed;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[PillarController] {gameObject.name} 初始化完成。伤害阈值: {damageThreshold}");
            }
        }

        /// <summary>
        /// 当柱子受到伤害时调用
        /// </summary>
        void OnPillarDamaged(float damage, GameObject damageSource)
        {
            if (hasCollapsed)
                return;

            accumulatedDamage += damage;
            lastDamageSource = damageSource;

            // 尝试获取击中点（从碰撞器中心作为近似）
            if (damageSource != null)
            {
                lastHitPoint = transform.position;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[PillarController] {gameObject.name} 受到 {damage} 点伤害。累计伤害: {accumulatedDamage}/{damageThreshold}");
            }

            // 检查是否达到倒塌阈值
            if (accumulatedDamage >= damageThreshold)
            {
                CollapsePillar();
            }
        }

        /// <summary>
        /// 当柱子生命值归零时调用
        /// </summary>
        void OnPillarDestroyed()
        {
            if (!hasCollapsed)
            {
                CollapsePillar();
            }
        }

        /// <summary>
        /// 触发柱子倒塌
        /// </summary>
        void CollapsePillar()
        {
            if (hasCollapsed)
                return;

            hasCollapsed = true;

            if (showDebugInfo)
            {
                Debug.Log($"[PillarController] {gameObject.name} 开始倒塌！");
            }

            // 激活物理效果
            pillarRigidbody.isKinematic = false;
            pillarRigidbody.useGravity = true;

            // 计算施加力的方向
            Vector3 forceDirection;
            if (useHitDirection && lastDamageSource != null)
            {
                // 使用伤害源方向
                forceDirection = (transform.position - lastDamageSource.transform.position).normalized;
                
                // 如果启用了保持接地，则移除或反转向上的分量
                if (keepGrounded)
                {
                    forceDirection.y = Mathf.Min(forceDirection.y, 0f); // 确保不向上
                    // 添加向下的分量，让柱子贴地倒塌
                    forceDirection.y -= 0.3f;
                }
                else
                {
                    forceDirection.y = 0.2f; // 只添加轻微向上分量
                }
            }
            else
            {
                // 使用预设方向
                forceDirection = transform.TransformDirection(collapseForceDirection);
                if (keepGrounded)
                {
                    forceDirection.y = Mathf.Min(forceDirection.y, 0f);
                }
            }

            // 归一化方向向量
            forceDirection.Normalize();

            // 施加力让柱子倒塌
            float force = accumulatedDamage * collapseForceMultiplier;
            pillarRigidbody.AddForce(forceDirection * force, ForceMode.Impulse);

            // 添加额外的向下力，确保柱子不会浮空
            if (keepGrounded)
            {
                Vector3 downwardForce = Vector3.down * (force * additionalDownwardForce);
                pillarRigidbody.AddForce(downwardForce, ForceMode.Impulse);
            }

            // 添加旋转力，让倒塌更自然（主要在水平轴上旋转）
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, forceDirection).normalized;
            if (torqueAxis.magnitude < 0.1f) // 避免零向量
            {
                torqueAxis = Vector3.right;
            }
            
            // 施加旋转力，让柱子向前倒
            pillarRigidbody.AddTorque(torqueAxis * force * 0.5f, ForceMode.Impulse);
            
            // 添加少量随机旋转，但主要在水平轴上
            Vector3 randomTorque = new Vector3(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.1f, 0.1f),  // y轴旋转减少
                Random.Range(-0.2f, 0.2f)
            ) * force * 0.3f;  // 减小随机旋转的影响
            pillarRigidbody.AddTorque(randomTorque, ForceMode.Impulse);

            // 播放音效
            if (collapseSound != null)
            {
                AudioUtility.CreateSFX(collapseSound, transform.position, AudioUtility.AudioGroups.Impact, 1f);
            }

            // 生成粒子效果
            if (collapseVFX != null)
            {
                Instantiate(collapseVFX, transform.position, Quaternion.identity);
            }

            // 如果设置了销毁时间，则在倒塌后销毁柱子
            if (destroyAfterCollapse > 0)
            {
                Destroy(gameObject, destroyAfterCollapse);
            }
        }

        /// <summary>
        /// 手动触发倒塌（可通过其他脚本调用）
        /// </summary>
        public void TriggerCollapse()
        {
            if (!hasCollapsed)
            {
                accumulatedDamage = damageThreshold; // 确保满足阈值
                CollapsePillar();
            }
        }

        /// <summary>
        /// 重置柱子状态（用于对象池或重用）
        /// </summary>
        public void ResetPillar()
        {
            hasCollapsed = false;
            accumulatedDamage = 0f;
            pillarRigidbody.isKinematic = true;
            pillarRigidbody.useGravity = false;
            pillarRigidbody.velocity = Vector3.zero;
            pillarRigidbody.angularVelocity = Vector3.zero;

            if (health != null)
            {
                health.CurrentHealth = health.MaxHealth;
            }
        }

        void FixedUpdate()
        {
            // 如果柱子已经倒塌且启用了保持接地，持续施加向下的力
            if (hasCollapsed && keepGrounded && pillarRigidbody != null && !pillarRigidbody.isKinematic)
            {
                // 持续施加向下的力，防止浮空
                pillarRigidbody.AddForce(Vector3.down * (pillarRigidbody.mass * additionalDownwardForce), ForceMode.Force);
                
                // 如果y速度向上超过阈值，强制压制
                if (pillarRigidbody.velocity.y > 2f)
                {
                    Vector3 velocity = pillarRigidbody.velocity;
                    velocity.y = Mathf.Min(velocity.y, 2f); // 限制向上速度
                    pillarRigidbody.velocity = velocity;
                }
            }
        }

        void OnDestroy()
        {
            // 移除事件监听
            if (health != null)
            {
                health.OnDamaged -= OnPillarDamaged;
                health.OnDie -= OnPillarDestroyed;
            }
        }

        // 绘制Gizmos用于调试
        void OnDrawGizmosSelected()
        {
            if (showDebugInfo)
            {
                Gizmos.color = hasCollapsed ? Color.red : Color.green;
                Gizmos.DrawWireSphere(transform.position, 0.5f);

                // 显示倒塌方向
                if (!useHitDirection)
                {
                    Gizmos.color = Color.yellow;
                    Vector3 direction = transform.TransformDirection(collapseForceDirection);
                    Gizmos.DrawRay(transform.position, direction * 2f);
                }
            }
        }
    }
}


