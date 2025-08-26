using UnityEngine;

public class ChainsawWeapon : WeaponBase
{
    public float dps = 10f;
    public float fuel = 10f;
    public float fuelDrain = 1f;
    public Collider2D hitbox;

    private bool active = false;

    public override void Fire()
    {
        if (fuel <= 0) return;
        active = true;
        hitbox.enabled = true;
        fuel -= fuelDrain * Time.deltaTime;
    }

    public override void StopFire()
    {
        active = false;
        hitbox.enabled = false;
    }

    public override void Reload()
    {
        // maybe refuel later
    }

    public override bool HasAmmo()
    {
        return fuel > 0;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!active) return;
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth e = other.GetComponent<EnemyHealth>();
            if (e != null) e.TakeDamage(Mathf.RoundToInt(dps * Time.deltaTime));
        }
    }
}