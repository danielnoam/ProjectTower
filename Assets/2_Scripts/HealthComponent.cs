using System;
using DNExtensions;
using UnityEngine;
using UnityEngine.Events;

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
    
    public event Action Death;
    
    
    private void Awake()
    {
        Reset();
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Death?.Invoke();
    }
    
    public void Reset()
    {
        currentHealth = maxHealth;
        isDead = false;
    }
}