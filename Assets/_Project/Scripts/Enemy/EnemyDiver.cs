using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyDiver : MonoBehaviour
{
    [Header("Targets")]
    public string playerTag = "Player";
    private Transform target;

    [Header("Cruise (Orbit)")]
    [Tooltip("Desired orbit radius around the player.")]
    public float cruiseRadius = 3.5f;              // <— controls the circular diameter (radius)
    [Tooltip("How fast we go around the circle tangentially.")]
    public float tangentialSpeed = 2.8f;
    [Tooltip("How strongly we correct back to the desired radius.")]
    public float radialGain = 2.0f;                // proportional gain (pull inward/outward)
    [Tooltip("Max movement speed clamp.")]
    public float maxSpeed = 9f;

    [Header("Dive")]
    public float diveSpeed = 8f;
    public float diveCooldown = 2.2f;
    public float diveRange = 6f;
    public float stopRange = 0.6f;

    private Rigidbody2D rb;
    private float cdTimer;

    private enum State { Cruise, Dive }
    private State state;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        state = State.Cruise;
    }

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        cdTimer = Random.Range(0.2f, 0.9f); // desync
    }

    private void FixedUpdate()
    {
        if (!target) { rb.linearVelocity = Vector2.zero; return; }

        cdTimer -= Time.fixedDeltaTime;

        Vector2 toP = (target.position - transform.position);
        float dist = toP.magnitude;
        Vector2 dirToP = (dist > 0.001f) ? toP / dist : Vector2.right;

        switch (state)
        {
            case State.Cruise:
            {
                // --- Orbit Controller ---
                // Tangent direction (perpendicular to player vector); choose a consistent side (CCW)
                Vector2 tangent = new Vector2(-dirToP.y, dirToP.x); // CCW tangent

                // Radial error: positive if outside the desired circle, negative if inside
                float radialError = dist - cruiseRadius;

                // Velocity = tangential motion + radial correction (push inward if too far out, outward if too far in)
                Vector2 vTangential = tangent * tangentialSpeed;
                Vector2 vRadialCorrection = -dirToP * (radialError * radialGain);

                Vector2 desiredVel = vTangential + vRadialCorrection;
                if (desiredVel.magnitude > maxSpeed)
                    desiredVel = desiredVel.normalized * maxSpeed;

                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, 0.35f);

                // Transition to Dive
                if (cdTimer <= 0f && dist <= diveRange)
                {
                    state = State.Dive;
                    cdTimer = diveCooldown;
                }
                break;
            }

            case State.Dive:
            {
                // Dash straight toward the player, then return to cruise
                if (dist > stopRange)
                    rb.linearVelocity = dirToP * diveSpeed;
                else
                    state = State.Cruise;
                break;
            }
        }

        // Face velocity (optional)
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float ang = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
    }

    // --------------- CRUISE GIZMOS (only affected lines) ---------------
    private void OnDrawGizmosSelected()
    {
        if (!target) return;

        Vector3 p = transform.position;
        Vector3 c = target.position;

        // Desired cruise circle
        Gizmos.color = new Color(0f, 1f, 1f, 0.8f); // cyan
        Gizmos.DrawWireSphere(c, cruiseRadius);

        // Radial line (center -> diver)
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(c, p);

        // Cruise tangent at current position (only show during Cruise)
        Vector3 toP = p - c;
        float dist = toP.magnitude;
        if (dist > 0.001f && state == State.Cruise)
        {
            Vector2 dirToP = (Vector2)toP / dist;
            Vector2 tangent = new Vector2(-dirToP.y, dirToP.x); // CCW tangent

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(p, p + (Vector3)(tangent * 2f));
        }

        // Optional: dive trigger ring (uncomment if needed)
        // Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        // Gizmos.DrawWireSphere(c, diveRange);
    }
}
