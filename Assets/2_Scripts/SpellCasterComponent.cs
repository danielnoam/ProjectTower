using System;
using System.Collections;
using DNExtensions;
using UnityEngine;

public class SpellCasterComponent : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenPerSecond = 15f;
    [SerializeField] private float cooldownBeforeRegen = 1;
    
    [Header("References")]
    [SerializeField] private Transform castPoint;
    
    [Separator]
    [SerializeField, ReadOnly] private float currentMana;
    [SerializeField, ReadOnly] private float regenCooldownTimer;
    [SerializeField, ReadOnly] private bool isRegenerating;
    
    
    private ICombatTarget _combatTarget;
    private Coroutine _manaRegenCoroutine;
    
    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    public bool IsFull => Mathf.Approximately(currentMana, maxMana);
    public bool IsEmpty => Mathf.Approximately(currentMana, 0f);
    
    public event Action<float> ManaChanged;
    public event Action ManaEmpty;
    public event Action ManaFull;

    
    
    
    private void Awake()
    {
        _combatTarget = GetComponent<ICombatTarget>();
        if (_combatTarget == null)
        {
            Debug.Log("SpellCasterComponent requires an ICombatTarget component.");
            enabled = false;
            return;
        }

        RestoreToMax();
    }
    
    private void Update()
    {
        if (regenCooldownTimer > 0f && !isRegenerating)
        {
            regenCooldownTimer -= Time.deltaTime;
            if (regenCooldownTimer <= 0f)
            {
                regenCooldownTimer = 0f;
                StartManaRegen();
            }
        }
    }
    
    private void StartManaRegen()
    {
        isRegenerating = true;
        _manaRegenCoroutine = StartCoroutine(ManaRegenCoroutine());
    }
    
    private void ResetManaRegenTimer()
    {
        if (_manaRegenCoroutine != null)
        {
            StopCoroutine(_manaRegenCoroutine);
            _manaRegenCoroutine = null;
        }
        regenCooldownTimer = cooldownBeforeRegen;
        isRegenerating = false;
    }
    
    private void Restore(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        ManaChanged?.Invoke(currentMana);

        if (currentMana >= maxMana)
        {
            RestoreToMax();
        }
    }
    
    private void RestoreToMax()
    {
        ResetManaRegenTimer();
        currentMana = maxMana;
        ManaFull?.Invoke();
    }
    
    private IEnumerator ManaRegenCoroutine()
    {
        while (!IsFull)
        {
            yield return new WaitForSeconds(1f);
            Restore(manaRegenPerSecond);
        }
        
        RestoreToMax();
        isRegenerating = false;
        _manaRegenCoroutine = null;
    }
    
    public void CastSpell(SOSpell spell, ICombatTarget target)
    {
        if (!CanCast(spell))
        {
            return;
        }
        
        spell.Cast(_combatTarget, target, castPoint ? castPoint : transform);
        
        if (TryConsume(spell.manaCost))
        {
            ResetManaRegenTimer();
        }
    }
    
    public bool CanCast(SOSpell spell)
    {
        return CurrentMana >= spell.manaCost;
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
    
}