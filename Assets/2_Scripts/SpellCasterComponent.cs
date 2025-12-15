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
    
    [Header("Cast Method Multipliers")]
    [SerializeField] private float instantStrengthMultiplier = 1f;
    [SerializeField] private float chargeStrengthMultiplier = 2f;
    [SerializeField] private float channelStrengthMultiplier = 0.3f;
    [Space(5)]
    [SerializeField] private float instantCostMultiplier = 1f;
    [SerializeField] private float chargeCostMultiplier = 0.75f;
    [SerializeField] private float channelCostMultiplier = 1.25f;
    
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
            Debug.LogError("SpellCasterComponent requires an ICombatTarget component.");
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
    

    
    public bool CastWithMethod(SOSpell spell, ICombatTarget target, CastMethod method)
    {
        if (!spell) return false;
        
        float costMultiplier = method switch
        {
            CastMethod.Instant => instantCostMultiplier,
            CastMethod.Charge => chargeCostMultiplier,
            CastMethod.Channel => channelCostMultiplier,
            _ => 1f
        };
        
        float strengthMultiplier = method switch
        {
            CastMethod.Instant => instantStrengthMultiplier,
            CastMethod.Charge => chargeStrengthMultiplier,
            CastMethod.Channel => channelStrengthMultiplier,
            _ => 1f
        };
        
        float actualCost = spell.baseCost * costMultiplier;
        
        if (!CanCast(actualCost))
        {
            return false;
        }
        
        spell.Cast(_combatTarget, target, castPoint ? castPoint : transform, strengthMultiplier);
        
        if (TryConsume(actualCost))
        {
            ResetManaRegenTimer();
            return true;
        }
        
        return false;
    }
    

    public float GetCostForMethod(SOSpell spell, CastMethod method)
    {
        if (!spell) return 0f;
        
        float costMultiplier = method switch
        {
            CastMethod.Instant => instantCostMultiplier,
            CastMethod.Charge => chargeCostMultiplier,
            CastMethod.Channel => channelCostMultiplier,
            _ => 1f
        };
        
        return spell.baseCost * costMultiplier;
    }
    
    public bool CanCast(float cost)
    {
        return CurrentMana >= cost;
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
    
    public bool CanCastWithMethod(SOSpell spell, CastMethod method)
    {
        return CanCast(GetCostForMethod(spell, method));
    }
    
    public bool CastInstant(SOSpell spell, ICombatTarget target)
    {
        return CastWithMethod(spell, target, CastMethod.Instant);
    }
    
    public bool CastCharged(SOSpell spell, ICombatTarget target)
    {
        return CastWithMethod(spell, target, CastMethod.Charge);
    }
    
    public bool CastChanneled(SOSpell spell, ICombatTarget target)
    {
        return CastWithMethod(spell, target, CastMethod.Channel);
    }

    
}