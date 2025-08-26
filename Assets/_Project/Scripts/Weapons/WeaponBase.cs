using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")]
    public string weaponName;
    public Sprite icon;

    public abstract void Fire();
    public abstract void Reload();
    public abstract bool HasAmmo();

    // Optional: override for melee/fuel-based weapons
    public virtual void StopFire() { }
}