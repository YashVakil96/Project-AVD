using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    [SerializeField] private bool invulnerable = false;

    public int CurrentHP { get; private set; }
    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged; // (current, max)

    private void Awake()
    {
        CurrentHP = maxHP;
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (invulnerable || amount <= 0) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        // For prototype: disable player. Replace with respawn/summary later.
        gameObject.SetActive(false);
    }

    public void SetInvulnerable(bool value) => invulnerable = value;
}