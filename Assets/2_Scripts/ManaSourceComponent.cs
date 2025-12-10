using System;
using DNExtensions;
using UnityEngine;

public class ManaSourceComponent : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    
    [Separator]
    [SerializeField, ReadOnly] private float currentMana;
    
    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    public bool IsFull => Mathf.Approximately(currentMana, maxMana);
    public bool IsEmpty => Mathf.Approximately(currentMana, 0f);
    
    public event Action<float> ManaChanged;
    public event Action ManaEmpty;
    public event Action ManaFull;
    
    private void Awake()
    {
        RestoreToMax();
    }
    
    public bool TryConsume(float amount)
    {
        if (currentMana < amount) return false;
        
        currentMana = Mathf.Max(0, currentMana - amount);
        ManaChanged?.Invoke(currentMana);
        
        if (currentMana <= 0)
        {
            ManaEmpty?.Invoke();
        }
        return true;
    }
    
    public void Restore(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        ManaChanged?.Invoke(currentMana);

        if (currentMana >= maxMana)
        {
            RestoreToMax();
        }
    }
    
    public void RestoreToMax()
    {
        currentMana = maxMana;
        ManaFull?.Invoke();
    }
}