using System;
using System.Collections;
using DNExtensions;
using UnityEngine;

[RequireComponent(typeof(ManaSourceComponent))]
public class SpellCaster : MonoBehaviour
{
    [Header("Regeneration")]
    [SerializeField] private float manaRegenPerSecond = 15f;
    [SerializeField] private float cooldownBeforeRegen = 1;
    
    [Header("References")]
    [SerializeField] private ManaSourceComponent manaSource;
    [SerializeField] private Transform castPoint;
    

    
    [Separator]
    [SerializeField, ReadOnly] private float regenCooldownTimer;
    
    private Coroutine _manaRegenCoroutine;
    private bool _isRegenerating;
    
    

    private void OnValidate()
    {
        if (!manaSource) { manaSource = this.GetOrAddComponent<ManaSourceComponent>();}
    }

    private void Awake()
    {
        if (!manaSource)
        {
            manaSource = GetComponent<ManaSourceComponent>();
        }
        
        regenCooldownTimer = 0f;
        _isRegenerating = false;
    }
    
    private void Update()
    {
        if (regenCooldownTimer > 0f && !_isRegenerating)
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
        _isRegenerating = true;
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
        _isRegenerating = false;
    }
    
    private IEnumerator ManaRegenCoroutine()
    {
        while (!manaSource.IsFull)
        {
            yield return new WaitForSeconds(1f);
            manaSource.Restore(manaRegenPerSecond);
        }
        
        manaSource.RestoreToMax();
        _isRegenerating = false;
        _manaRegenCoroutine = null;
    }
    
    public void CastSpell(SOSpell spell, ICombatTarget target)
    {
        if (!CanCast(spell))
        {
            return;
        }
        
        ICombatTarget source = GetComponent<ICombatTarget>();
        if (source == null) return;
        
        spell.Cast(source, target, castPoint ? castPoint : transform);
        
        if (manaSource.TryConsume(spell.manaCost))
        {
            ResetManaRegenTimer();
        }
    }
    
    public bool CanCast(SOSpell spell)
    {
        return manaSource.CurrentMana >= spell.manaCost;
    }
}