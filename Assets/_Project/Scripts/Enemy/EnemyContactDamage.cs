using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : MonoBehaviour
{
    public int damage = 10;
    public float hitCooldown = 0.6f;  // per-enemy cooldown
    public float knockbackForce = 6f;

    private float timer;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = false; // we want solid collision with Player
    }

    private void Update() { if (timer > 0f) timer -= Time.deltaTime; }

    private void OnCollisionEnter2D(Collision2D col) { TryHit(col.collider); }
    private void OnCollisionStay2D(Collision2D col)  { TryHit(col.collider); }

    private void TryHit(Collider2D other)
    {
        if (timer > 0f) return;
        if (!other.CompareTag("Player")) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            // knockback
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
            timer = hitCooldown;
        }
    }
}