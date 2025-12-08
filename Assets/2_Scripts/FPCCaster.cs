
using System.Collections;
using DNExtensions;
using UnityEngine;
using UnityEngine.InputSystem;



[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager))]
public class FPCCaster : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenPerSecond = 5f;
    [SerializeField] private float cooldownBeforeRegen = 2f;
    
    [Header("References")]
    [SerializeField] private FPCManager fpcManager;
    
    [Separator]
    [SerializeField] private SOSpell currentSpell;
    [SerializeField, ReadOnly] private float currentMana;
    [SerializeField, ReadOnly] private float regenCooldownTimer;
    [SerializeField, ReadOnly] private float castingTimer;
    
    private Coroutine _manaRegenCoroutine;
    private bool _isCasting;
    private bool _isRegenerating;
    
    
    
    private void OnValidate()
    {
        if (!fpcManager) fpcManager = GetComponent<FPCManager>();
    }

    private void Awake()
    {
        currentMana = maxMana;
        regenCooldownTimer = 0f;
        _isRegenerating = false;
    }

    private void OnEnable()
    {
        fpcManager.FPCInput.OnAttackAction += CastSelectedSpell;
    }

    private void OnDisable()
    {
        fpcManager.FPCInput.OnAttackAction -= CastSelectedSpell;
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

    private void CastSelectedSpell(InputAction.CallbackContext context)
    {
        if (!currentSpell || currentMana < currentSpell.manaCost|| currentMana <= 0) return;
        
        
        if (context.started && !_isCasting)
        {
            
            _isCasting = true;
        

            switch (currentSpell.castType)
            {
                case SOSpell.CastType.Instant:
                    
                    currentSpell.Cast(fpcManager, null);
                    currentMana -= currentSpell.manaCost;
                    ResetManaRegenTimer();
                    _isCasting = false;
                    
                    break;
                case SOSpell.CastType.Channeled:
                
                
                    break;
                case SOSpell.CastType.Charged:
                
                
                    break;
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
        if (_manaRegenCoroutine != null) StopCoroutine(_manaRegenCoroutine);
        _manaRegenCoroutine = null;
        regenCooldownTimer = cooldownBeforeRegen;
        _isRegenerating = false;
    }
    
    private IEnumerator ManaRegenCoroutine()
    {
        while (currentMana < maxMana)
        {
            yield return new WaitForSeconds(1f);
            currentMana += manaRegenPerSecond;
        }
        
        currentMana = maxMana;
        _manaRegenCoroutine = null;
    }
    
    
    
}