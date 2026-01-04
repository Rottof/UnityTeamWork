using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 怪物波次数据组件
    /// 记录怪物所属波次和属性增益信息
    /// </summary>
    public class EnemyWaveData : MonoBehaviour
    {
        [Header("波次信息")]
        [Tooltip("怪物所属波次")]
        public int waveNumber = 0;

        [Tooltip("是否为大型怪物")]
        public bool isLargeEnemy = false;

        [Header("属性加成")]
        [Tooltip("血量倍率")]
        public float healthMultiplier = 1f;

        [Tooltip("伤害倍率")]
        public float damageMultiplier = 1f;

        /// <summary>
        /// 获取怪物类型描述
        /// </summary>
        public string GetEnemyTypeDescription()
        {
            return isLargeEnemy ? "大型怪" : "小型怪";
        }

        /// <summary>
        /// 获取详细信息
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"[{GetEnemyTypeDescription()}] 波次:{waveNumber} 血量x{healthMultiplier:F2} 伤害x{damageMultiplier:F2}";
        }
    }
}




