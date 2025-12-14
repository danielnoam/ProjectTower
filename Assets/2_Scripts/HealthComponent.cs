using System;
using DNExtensions;
using UnityEngine;
using UnityEngine.Events;

public struct HealthChangeData 
{
    public float NewHealth;
    public float Delta;
    public ICombatTarget DamageDealer;
}

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [Separator]
    [SerializeField, ReadOnly] private bool isDead;
    [SerializeField, ReadOnly] private float currentHealth;
    
    

    public bool IsDead => isDead;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    
    public event Action Died;
    public event Action<HealthChangeData> HealthChanged;

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            currentHealth = maxHealth;
        }
    }

    private void Awake()
    {
        Reset();
    }
    
    public void TakeDamage(float damage, ICombatTarget damageDealer)
    {
        if (isDead) return;
        
        float oldHealth = currentHealth;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        

        var changeData = new HealthChangeData
        {
            NewHealth = currentHealth,
            Delta = currentHealth - oldHealth,
            DamageDealer = damageDealer
        };
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            HealthChanged?.Invoke(changeData);
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
    
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    
        HealthChanged?.Invoke(new HealthChangeData
        {
            NewHealth = currentHealth,
            Delta = currentHealth - oldHealth,
        });
    }
    
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Died?.Invoke();
    }
    
    public void Reset()
    {
        isDead = false;
        currentHealth = maxHealth;
        HealthChanged?.Invoke(new HealthChangeData
        {
            NewHealth = currentHealth,
            Delta = 0,
        });
    }
}