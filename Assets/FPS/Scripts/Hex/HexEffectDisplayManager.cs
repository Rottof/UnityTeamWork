using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Hex
{
    public class HexEffectDisplayManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("UI panel containing the layout group for displaying hex effects")]
        public RectTransform HexEffectPanel;

        [Tooltip("Prefab for displaying a single hex effect")]
        public GameObject HexEffectToastPrefab;

        [Header("Settings")]
        [Tooltip("Maximum number of hex effects to display")]
        public int MaxDisplayCount = 20;

        [Tooltip("Play sound when adding hex effect")]
        public AudioClip AddEffectSound;

        List<HexEffectToast> m_ActiveToasts = new List<HexEffectToast>();
        AudioSource m_AudioSource;

        // 单例模式，方便其他脚本调用
        public static HexEffectDisplayManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 初始化音频源
            if (AddEffectSound != null)
            {
                m_AudioSource = gameObject.AddComponent<AudioSource>();
                m_AudioSource.playOnAwake = false;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 添加一个海克斯效果到显示列表
        /// </summary>
        /// <param name="hexName">海克斯效果名称</param>
        public void AddHexEffect(string hexName)
        {
            if (string.IsNullOrEmpty(hexName))
            {
                Debug.LogWarning("海克斯效果名称为空！");
                return;
            }

            if (HexEffectToastPrefab == null)
            {
                Debug.LogError("HexEffectToastPrefab 未设置！");
                return;
            }

            if (HexEffectPanel == null)
            {
                Debug.LogError("HexEffectPanel 未设置！");
                return;
            }

            // 检查是否超过最大显示数量
            if (m_ActiveToasts.Count >= MaxDisplayCount)
            {
                Debug.LogWarning($"已达到最大显示数量({MaxDisplayCount})！");
                return;
            }

            // 实例化预制体
            GameObject toastInstance = Instantiate(HexEffectToastPrefab, HexEffectPanel);
            HexEffectToast toast = toastInstance.GetComponent<HexEffectToast>();

            if (toast != null)
            {
                toast.Initialize(hexName);
                m_ActiveToasts.Add(toast);

                // 强制重建布局
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(HexEffectPanel);

                // 播放音效
                PlayAddEffectSound();

                Debug.Log($"已添加海克斯效果到显示列表: {hexName}");
            }
            else
            {
                Debug.LogError("HexEffectToastPrefab 缺少 HexEffectToast 组件！");
                Destroy(toastInstance);
            }
        }

        /// <summary>
        /// 清除所有显示的海克斯效果（可选功能）
        /// </summary>
        public void ClearAllEffects()
        {
            foreach (var toast in m_ActiveToasts)
            {
                if (toast != null)
                {
                    Destroy(toast.gameObject);
                }
            }
            m_ActiveToasts.Clear();

            Debug.Log("已清除所有海克斯效果显示");
        }

        /// <summary>
        /// 获取当前显示的效果数量
        /// </summary>
        public int GetActiveEffectCount()
        {
            // 清理已销毁的对象
            m_ActiveToasts.RemoveAll(toast => toast == null);
            return m_ActiveToasts.Count;
        }

        void PlayAddEffectSound()
        {
            if (AddEffectSound != null && m_AudioSource != null)
            {
                m_AudioSource.PlayOneShot(AddEffectSound);
            }
        }
    }
}

