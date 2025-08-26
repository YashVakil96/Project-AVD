using UnityEngine;

public class Pellet : MonoBehaviour
{
    public float lifetime = 0.5f;
    public float damage = 6f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // if Enemy has Health script:
            EnemyHealth e = other.GetComponent<EnemyHealth>();
            if (e != null) e.TakeDamage((int)damage);
            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}