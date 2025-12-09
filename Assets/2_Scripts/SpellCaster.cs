using System.Collections;
using DNExtensions;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenPerSecond = 5f;
    [SerializeField] private float cooldownBeforeRegen = 2f;
    
    [Separator]
    [SerializeField, ReadOnly] private float currentMana;
    [SerializeField, ReadOnly] private float regenCooldownTimer;
    
    private Coroutine _manaRegenCoroutine;
    private bool _isRegenerating;
    
    private void Awake()
    {
        currentMana = maxMana;
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
    
    public void CastSpell(SOSpell spell, ICombatTarget target)
    {
        if (!CanCast(spell))
        {
            Debug.LogWarning("Not enough mana to cast spell!");
            return;
        }
        
        ICombatTarget source = GetComponent<ICombatTarget>();
        if (source == null)
        {
            Debug.LogError("SpellCaster requires ICombatTarget component!");
            return;
        }
        
        spell.Cast(source, target);
        ConsumeMana(spell.manaCost);
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
        while (currentMana < maxMana)
        {
            yield return new WaitForSeconds(1f);
            currentMana = Mathf.Min(currentMana + manaRegenPerSecond, maxMana);
        }
        
        currentMana = maxMana;
        _isRegenerating = false;
        _manaRegenCoroutine = null;
    }
    
    public void ConsumeMana(float amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        ResetManaRegenTimer();
    }
    
    public bool CanCast(SOSpell spell)
    {
        return currentMana >= spell.manaCost;
    }
}