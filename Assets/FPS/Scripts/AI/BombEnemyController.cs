using System.Collections;
using System.Collections.Generic;
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

        [Header("Arming / Visuals")]
        [Tooltip("靠近玩家后等待多少秒才爆炸")]
        public float ArmDelay = 3f;

        [Tooltip("头顶感叹号粒子")]
        public ParticleSystem AlertVFX;

        [Tooltip("感叹号闪烁初始间隔")]
        public float ExclamationStartInterval = 0.5f;

        [Tooltip("感叹号最小闪烁间隔")]
        public float ExclamationMinInterval = 0.05f;

        [Tooltip("全身闪烁起始频率")]
        public float BodyFlashStartFrequency = 1f;

        [Tooltip("全身闪烁结束频率")]
        public float BodyFlashEndFrequency = 8f;

        [Tooltip("Emission 强度（HDR白闪倍数）")]
        public float BodyFlashStrength = 2f;
        
        [Header("Idle Settings")]
        public Animator AnimatorController; // 可选：挂 Animator
        public string IdleTriggerName = "Idle"; // Animator 里的 Idle Trigger

        bool m_HasExploded;
        bool m_IsArming;
        Coroutine m_ArmRoutine;

        readonly List<Material> m_Materials = new();
        readonly List<Color> m_OriginalColors = new();
        readonly List<Color> m_OriginalEmissions = new();

        [Header("White Flash Settings")]
        public float WhiteFlashPower = 2f;       // 指数控制闪烁曲线
        public float WhiteFlashIntensity = 1.5f; // 白光权重

        public override bool TryAtack(Vector3 enemyPosition)
        {
            if (m_HasExploded || m_IsArming)
                return false;

            float sqrDist = (transform.position - enemyPosition).sqrMagnitude;
            float triggerSqr = ExplosionTriggerDistance * ExplosionTriggerDistance;

            if (sqrDist <= triggerSqr)
            {
                m_ArmRoutine = StartCoroutine(ArmAndExplode());
                return true;
            }

            return false;
        }

        IEnumerator ArmAndExplode()
        {
            m_IsArming = true;
            
            var agent = GetComponent<NavMeshAgent>();
            if (agent)
                agent.isStopped = true;

            if (AnimatorController)
                AnimatorController.SetTrigger(IdleTriggerName);
            
            // ▶ 感叹号启动
            if (AlertVFX)
            {
                AlertVFX.gameObject.SetActive(true);
                AlertVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                AlertVFX.Play(true);
            }

            CacheMaterials();

            float elapsed = 0f;
            float exTimer = 0f;
            float exInterval = ExclamationStartInterval;
            bool exVisible = true;
            float bodyFlashTimer = 0f;

            while (elapsed < ArmDelay && !m_HasExploded)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / ArmDelay);

                // =====================
                // 1️⃣ 感叹号闪烁
                // =====================
                exInterval = Mathf.Lerp(ExclamationStartInterval, ExclamationMinInterval, progress);
                exTimer += Time.deltaTime;
                if (exTimer >= exInterval)
                {
                    exTimer = 0f;
                    exVisible = !exVisible;

                    if (AlertVFX)
                    {
                        if (exVisible)
                            AlertVFX.Play(true);
                        else
                            AlertVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }

                // =====================
                // 2️⃣ 全身白光闪烁（Albedo + Emission HDR）
                // =====================
                float flashFreq = Mathf.Lerp(BodyFlashStartFrequency, BodyFlashEndFrequency, progress);
                bodyFlashTimer += Time.deltaTime * flashFreq;
                float pulse = Mathf.Abs(Mathf.Sin(bodyFlashTimer));

                // 白光指数控制
                float whiteAmount = Mathf.Pow(pulse, WhiteFlashPower) * WhiteFlashIntensity;
                whiteAmount = Mathf.Clamp01(whiteAmount);

                for (int i = 0; i < m_Materials.Count; i++)
                {
                    var mat = m_Materials[i];
                    if (!mat) continue;
                    if (progress > 0.8f)
                    {
                        mat.color = Color.white;
                        mat.SetColor("_EmissionColor", Color.white * 10f);
                        continue;
                    }

                    // Albedo 插值到白
                    mat.color = Color.Lerp(m_OriginalColors[i], Color.white, whiteAmount);

                    // Emission HDR 白闪
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        float boost = progress > 0.8f ? 10f : 4f; // 后期加倍
                        mat.SetColor("_EmissionColor", m_OriginalEmissions[i] + Color.white * pulse * BodyFlashStrength * boost);
                    }
                }
             
                yield return null;
            }

            if (!m_HasExploded)
                Explode();
        }

        void CacheMaterials()
        {
            m_Materials.Clear();
            m_OriginalColors.Clear();
            m_OriginalEmissions.Clear();

            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in r.materials)
                {
                    if (!mat.HasProperty("_Color"))
                        continue;

                    m_Materials.Add(mat);
                    m_OriginalColors.Add(mat.color);

                    if (mat.HasProperty("_EmissionColor"))
                    {
                        m_OriginalEmissions.Add(mat.GetColor("_EmissionColor"));
                        mat.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        m_OriginalEmissions.Add(Color.black);
                    }
                }
            }
        }

        void RestoreMaterials()
        {
            for (int i = 0; i < m_Materials.Count; i++)
            {
                var mat = m_Materials[i];
                if (!mat) continue;

                mat.color = m_OriginalColors[i];

                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", m_OriginalEmissions[i]);
            }

            m_Materials.Clear();
            m_OriginalColors.Clear();
            m_OriginalEmissions.Clear();

            if (AlertVFX)
                AlertVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void Explode()
        {
            if (m_HasExploded) return;
            m_HasExploded = true;

            if (m_ArmRoutine != null)
            {
                StopCoroutine(m_ArmRoutine);
                m_ArmRoutine = null;
            }

            RestoreMaterials();
            onAttack?.Invoke();

            if (ExplosionVFX)
                Instantiate(ExplosionVFX, transform.position, Quaternion.identity);

            foreach (var hit in Physics.OverlapSphere(transform.position, ExplosionRadius, -1, QueryTriggerInteraction.Ignore))
            {
                Health h = hit.GetComponent<Health>();
                if (h && h.gameObject != gameObject)
                    h.TakeDamage(ExplosionDamage, gameObject);
            }

            GetComponent<Health>().Kill();
        }

        void OnDisable()
        {
            if (m_ArmRoutine != null)
                StopCoroutine(m_ArmRoutine);

            RestoreMaterials();
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
