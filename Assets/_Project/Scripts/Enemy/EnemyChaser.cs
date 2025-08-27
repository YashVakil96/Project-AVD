using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 3.2f;
    public float stopRange = 0.75f;        // stop before overlapping
    public float steeringLerp = 12f;       // smoothing for turning

    private Transform target;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void FixedUpdate()
    {
        if (target == null) { rb.linearVelocity = Vector2.zero; return; }

        Vector2 toPlayer = (target.position - transform.position);
        float dist = toPlayer.magnitude;

        Vector2 desired = dist > stopRange ? toPlayer.normalized * moveSpeed : Vector2.zero;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desired, 1f - Mathf.Exp(-steeringLerp * Time.fixedDeltaTime));

        // face movement (optional)
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float ang = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
    }
}