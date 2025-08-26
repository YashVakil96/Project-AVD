using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")] [SerializeField] private float dashSpeedMultiplier = 2.0f;
    [SerializeField] private float dashDuration = 0.22f;
    [SerializeField] private float dashCooldown = 2.0f;
    [SerializeField] private bool dashGrantsIFrames = true;

    [Header("References")] [SerializeField]
    private Transform weaponPivot; // assign your child "WeaponPivot"

    [SerializeField] private PlayerHealth playerHealth;

    private Rigidbody2D rb;
    private InputSystem_Actions input;
    private Vector2 moveInput;
    private Camera cam;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private WeaponBase currentWeapon;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();

        input = new InputSystem_Actions();
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        input.Player.Dash.performed += ctx => TryDash();

        // input.Player.Dash.performed += _ => TryDash();
    }

    private void Start()
    {
        currentWeapon = GetComponentInChildren<WeaponBase>(); // grabs first child weapon
    }

    private void Update()
    {
        var lmbHeld = Mouse.current.leftButton.isPressed;
        var lmbDown = Mouse.current.leftButton.wasPressedThisFrame;
        if (Mouse.current.leftButton.isPressed)
        {
            if (currentWeapon is BoomstickWeapon shotgun)
            {
                if (lmbDown) shotgun.Fire(); // try to fire on click
                if (lmbHeld) shotgun.MarkHeldThisFrame(); // tells the weapon the button is still held
                else shotgun.StopFire();
            }
        }


        if (Keyboard.current.rKey.wasPressedThisFrame)
            currentWeapon.Reload();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void FixedUpdate()
    {
        // Movement
        Vector2 desiredVel = moveInput.normalized * moveSpeed;

        if (isDashing)
            desiredVel *= dashSpeedMultiplier;

        rb.linearVelocity = desiredVel;

        // Dash timers
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
                EndDash();
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;

        // Aim the body (optional): flip sprite based on mouse
        if (weaponPivot != null && cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 lookDir = (mouseWorld - weaponPivot.position);
            float ang = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
        }
    }

    private void TryDash()
    {
        if (isDashing || dashCooldownTimer > 0f) return;

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (dashGrantsIFrames && playerHealth != null)
            playerHealth.SetInvulnerable(true);
    }

    private void EndDash()
    {
        isDashing = false;
        if (dashGrantsIFrames && playerHealth != null)
            playerHealth.SetInvulnerable(false);
    }

    // Optional helper for other systems to know if we can take damage
    public bool IsDashing => isDashing;
}