using DNExtensions;
using UnityEngine;
using UnityEngine.Events;

public class FpcHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [Separator]
    [SerializeField, ReadOnly] private float currentHealth;
    
    
    private bool _isDead;
    
    private void Awake()
    {
        Respawn();
    }
    
    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (_isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    
    private void Die()
    {
        if (_isDead) return;
        
        _isDead = true;
    }
    
    private void Respawn()
    {
        currentHealth = maxHealth;
        _isDead = false;
    }
}