using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    [RequireComponent(typeof(FillBarColorChange))]
    public class AmmoCounter : MonoBehaviour
    {
        [Tooltip("CanvasGroup to fade the ammo UI")]
        public CanvasGroup CanvasGroup;

        [Tooltip("Image for the weapon icon")] public Image WeaponImage;

        [Tooltip("Image component for the background")]
        public Image AmmoBackgroundImage;

        [Tooltip("Image component to display fill ratio")]
        public Image AmmoFillImage;

        [Tooltip("Text for Weapon index")] 
        public TextMeshProUGUI WeaponIndexText;

        [Tooltip("Text for Bullet Counter")] 
        public TextMeshProUGUI BulletCounter;

        [Tooltip("Reload Text for Weapons with physical bullets")]
        public RectTransform Reload;

        [Header("Selection")] [Range(0, 1)] [Tooltip("Opacity when weapon not selected")]
        public float UnselectedOpacity = 0.5f;

        [Tooltip("Scale when weapon not selected")]
        public Vector3 UnselectedScale = Vector3.one * 0.8f;

        [Tooltip("Root for the control keys")] public GameObject ControlKeysRoot;

        [Header("Feedback")] [Tooltip("Component to animate the color when empty or full")]
        public FillBarColorChange FillBarColorChange;

        [Tooltip("Sharpness for the fill ratio movements")]
        public float AmmoFillMovementSharpness = 20f;

        public int WeaponCounterIndex { get; set; }

        PlayerWeaponsManager m_PlayerWeaponsManager;
        WeaponController m_Weapon;
        Vector2 m_OriginalBackgroundSize;
        Vector2 m_OriginalFillSize;

        void Awake()
        {
            EventManager.AddListener<AmmoPickupEvent>(OnAmmoPickup);
            EventManager.AddListener<AmmoUIScaleEvent>(OnAmmoUIScaleEvent);
            
            // 保存原始尺寸
            if (AmmoBackgroundImage != null)
            {
                m_OriginalBackgroundSize = AmmoBackgroundImage.rectTransform.sizeDelta;
            }
            if (AmmoFillImage != null)
            {
                m_OriginalFillSize = AmmoFillImage.rectTransform.sizeDelta;
            }
        }

        void OnAmmoPickup(AmmoPickupEvent evt)
        {
            if (evt.Weapon == m_Weapon)
            {
                BulletCounter.text = m_Weapon.GetCarriedPhysicalBullets().ToString();
            }
        }

        void OnAmmoUIScaleEvent(AmmoUIScaleEvent evt)
        {
            ScaleAmmoBar(evt.ScaleMultiplier);
        }

        public void Initialize(WeaponController weapon, int weaponIndex)
        {
            m_Weapon = weapon;
            WeaponCounterIndex = weaponIndex;
            WeaponImage.sprite = weapon.WeaponIcon;
            if (!weapon.HasPhysicalBullets)
                BulletCounter.transform.parent.gameObject.SetActive(false);
            else
                BulletCounter.text = weapon.GetCarriedPhysicalBullets().ToString();

            Reload.gameObject.SetActive(false);
            m_PlayerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, AmmoCounter>(m_PlayerWeaponsManager, this);

            WeaponIndexText.text = (WeaponCounterIndex + 1).ToString();

            FillBarColorChange.Initialize(1f, m_Weapon.GetAmmoNeededToShoot());
        }

        void Update()
        {
            float currenFillRatio = m_Weapon.CurrentAmmoRatio;
            AmmoFillImage.fillAmount = Mathf.Lerp(AmmoFillImage.fillAmount, currenFillRatio,
                Time.deltaTime * AmmoFillMovementSharpness);

            BulletCounter.text = m_Weapon.GetCarriedPhysicalBullets().ToString();

            bool isActiveWeapon = m_Weapon == m_PlayerWeaponsManager.GetActiveWeapon();

            CanvasGroup.alpha = Mathf.Lerp(CanvasGroup.alpha, isActiveWeapon ? 1f : UnselectedOpacity,
                Time.deltaTime * 10);
            transform.localScale = Vector3.Lerp(transform.localScale, isActiveWeapon ? Vector3.one : UnselectedScale,
                Time.deltaTime * 10);
            ControlKeysRoot.SetActive(!isActiveWeapon);

            FillBarColorChange.UpdateVisual(currenFillRatio);

            Reload.gameObject.SetActive(m_Weapon.GetCarriedPhysicalBullets() > 0 && m_Weapon.GetCurrentAmmo() == 0 && m_Weapon.IsWeaponActive);
        }

        void Destroy()
        {
            EventManager.RemoveListener<AmmoPickupEvent>(OnAmmoPickup);
            EventManager.RemoveListener<AmmoUIScaleEvent>(OnAmmoUIScaleEvent);
        }

        /// <summary>
        /// 调整弹药条的UI尺寸（用于弹药充裕效果）
        /// </summary>
        /// <param name="scaleMultiplier">尺寸倍数（例如1.5表示增加50%）</param>
        public void ScaleAmmoBar(float scaleMultiplier)
        {
            if (AmmoBackgroundImage != null)
            {
                // 基于当前尺寸进行缩放，支持叠加效果
                Vector2 currentSize = AmmoBackgroundImage.rectTransform.sizeDelta;
                Vector2 newSize = new Vector2(currentSize.x * scaleMultiplier, currentSize.y);
                AmmoBackgroundImage.rectTransform.sizeDelta = newSize;
                Debug.Log($"弹药UI调整：{m_Weapon.WeaponName} 背景尺寸 {currentSize.x:F1} -> {newSize.x:F1}");
            }
            
            if (AmmoFillImage != null)
            {
                // 基于当前尺寸进行缩放，支持叠加效果
                Vector2 currentSize = AmmoFillImage.rectTransform.sizeDelta;
                Vector2 newSize = new Vector2(currentSize.x * scaleMultiplier, currentSize.y);
                AmmoFillImage.rectTransform.sizeDelta = newSize;
                Debug.Log($"弹药UI调整：{m_Weapon.WeaponName} 填充尺寸 {currentSize.x:F1} -> {newSize.x:F1}");
            }
        }
    }
}