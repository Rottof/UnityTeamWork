using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class BulletPickup : Pickup
    {
        [Header("Parameters")] 
        [Tooltip("If true, refills all weapons' ammo; if false, only refills active weapon")]
        public bool RefillAllWeapons = true;

        protected override void OnPicked(PlayerCharacterController player)
        {
            PlayerWeaponsManager weaponsManager = player.GetComponent<PlayerWeaponsManager>();
            if (weaponsManager != null)
            {
                if (RefillAllWeapons)
                {
                    // Refill all weapons in the player's inventory
                    // Since we can't directly access the array length, we'll try all possible slots
                    for (int i = 0; i < 9; i++) // 9 is the default weapon slot count in PlayerWeaponsManager
                    {
                        WeaponController weapon = weaponsManager.GetWeaponAtSlotIndex(i);
                        if (weapon != null)
                        {
                            // Refill the weapon's ammo to max
                            // For weapons with physical bullets, increase carried bullets
                            if (weapon.HasPhysicalBullets)
                            {
                                // For weapons with physical bullets, increase carried bullets to max
                                // Note: AddCarriablePhysicalBullets uses Mathf.Max which might be a bug in the original code
                                weapon.AddCarriablePhysicalBullets(weapon.MaxAmmo);
                            }
                            // For other weapons, we can use UseAmmo with a negative value to "add" ammo
                            // This works because UseAmmo does: m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo)
                            // So if amount is negative, we effectively add to m_CurrentAmmo
                            weapon.UseAmmo(weapon.GetCurrentAmmo() - weapon.MaxAmmo);
                        }
                    }
                }
                else
                {
                    // Refill only the active weapon
                    WeaponController activeWeapon = weaponsManager.GetActiveWeapon();
                    if (activeWeapon != null)
                    {
                        // Refill the weapon's ammo to max
                        // For weapons with physical bullets, increase carried bullets
                        if (activeWeapon.HasPhysicalBullets)
                        {
                            // For weapons with physical bullets, increase carried bullets to max
                            // Note: AddCarriablePhysicalBullets uses Mathf.Max which might be a bug in the original code
                            activeWeapon.AddCarriablePhysicalBullets(activeWeapon.MaxAmmo);
                        }
                        // For other weapons, we can use UseAmmo with a negative value to "add" ammo
                        // This works because UseAmmo does: m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo)
                        // So if amount is negative, we effectively add to m_CurrentAmmo
                        activeWeapon.UseAmmo(activeWeapon.GetCurrentAmmo() - activeWeapon.MaxAmmo);
                    }
                }

                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}

