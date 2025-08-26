using UnityEngine;

public class BoomstickWeapon : WeaponBase
{
    [Header("Shotgun")] public int pellets = 8;
    public float spread = 15f;
    public float pelletDamage = 6f;
    public float reloadTime = 1.2f;
    public int clipSize = 2;
    public int currentAmmo;
    public int reserveAmmo = 20;

    [Header("Fire Control")] public float shotsPerSecond = 2f; // 2 clicks/sec
    public bool isAutomatic = false; // keep false for shotgun
    private float _nextFireTime = 0f;
    private bool _wasHeldLastFrame = false;

    [Header("Refs")] public Transform firePoint;
    public GameObject pelletPrefab;
    public float pelletSpeed = 12f;

    private bool _isReloading;

    private void Start()
    {
        currentAmmo = Mathf.Clamp(currentAmmo == 0 ? clipSize : currentAmmo, 0, clipSize);
    }

    public override void Fire()
    {
        // block if reloading or during fire-rate cooldown
        if (_isReloading || Time.time < _nextFireTime) return;

        // if not automatic, only allow a single shot per button press
        // (requires controller to STOP calling Fire() when button not held)
        if (!isAutomatic && _wasHeldLastFrame) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadCo());
            return;
        }

        ShootPellets();
        currentAmmo--;

        // set fire-rate gate
        _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond);
    }

    public override void Reload()
    {
        if (_isReloading) return;
        _isReloading = true;
        currentAmmo = clipSize; // simple reload
        _isReloading = false;
    }

    public override bool HasAmmo()
    {
        return currentAmmo > 0 || reserveAmmo > 0;
    }

    public override void StopFire()
    {
        // mark that button is no longer held (enables next semi-auto shot)
        _wasHeldLastFrame = false;
    }

    // call this once per frame from controller when LMB is held
    public void MarkHeldThisFrame()
    {
        _wasHeldLastFrame = true;
    }

    private void ShootPellets()
    {
        for (int i = 0; i < pellets; i++)
        {
            float angle = Random.Range(-spread * 0.5f, spread * 0.5f);
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);
            var pellet = Instantiate(pelletPrefab, firePoint.position, rot);
            var rb = pellet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = pellet.transform.right * pelletSpeed;
        }
    }

    private System.Collections.IEnumerator ReloadCo()
    {
        if (_isReloading) yield break;
        _isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int need = clipSize - currentAmmo;
        int load = Mathf.Min(need, reserveAmmo);
        currentAmmo += load;
        reserveAmmo -= load;
        _isReloading = false;
    }
}