using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviour
    {
        [Tooltip("Maximum amount of health")] public float MaxHealth = 10f;

        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction OnDie;

        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }
        public bool CanPickup() => CurrentHealth < MaxHealth;

        // 伤害减免系数（0-0.9，表示减免0%-90%伤害）
        private float m_DamageReduction = 0f;
        private const float MAX_DAMAGE_REDUCTION = 0.9f; // 最大90%减免

        public float DamageReduction
        {
            get => m_DamageReduction;
            set => m_DamageReduction = Mathf.Clamp(value, 0f, MAX_DAMAGE_REDUCTION);
        }

        public float GetRatio() => CurrentHealth / MaxHealth;
        public bool IsCritical() => GetRatio() <= CriticalHealthRatio;

        bool m_IsDead;

        void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // call OnHeal action
            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        public void IncreaseMaxHealth(float amount)
        {
            MaxHealth += amount;
            // 同时增加当前生命值，保持生命值比例或直接加满
            CurrentHealth += amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible)
                return;

            // 应用伤害减免
            float reducedDamage = damage * (1f - m_DamageReduction);

            float healthBefore = CurrentHealth;
            CurrentHealth -= reducedDamage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
            {
                OnDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (m_IsDead)
                return;

            // call OnDie action
            if (CurrentHealth <= 0f)
            {
                m_IsDead = true;
                OnDie?.Invoke();
            }
        }
    }
}