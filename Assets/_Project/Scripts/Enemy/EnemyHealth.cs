using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 20;
    private int currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}