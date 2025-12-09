using System;
using DNExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager), typeof(SpellCaster))]
public class FPCCaster : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxTargetRange = 50f;
    
    [Header("References")]
    [SerializeField] private FPCManager fpcManager;
    [SerializeField] private SpellCaster spellCaster;
    
    [Separator]
    [SerializeField] private SOSpell currentSpell;
    [SerializeField, ReadOnly] private bool isCasting;
    [SerializeField, ReadOnly] private bool finishedCasting;
    [SerializeField, ReadOnly] private float castingTime;
    private Camera _cam;



    private void OnValidate()
    {
        if (!fpcManager) fpcManager = this.GetOrAddComponent<FPCManager>();
        if (!spellCaster) spellCaster = this.GetOrAddComponent<SpellCaster>();
    }
    
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnEnable()
    {
        fpcManager.FpcInput.OnAttackAction += TryCastSpell;
    }

    private void OnDisable()
    {
        fpcManager.FpcInput.OnAttackAction -= TryCastSpell;
    }

    private void Update()
    {

        if (!currentSpell || !isCasting) return;
        
        // Charge spell
        if (currentSpell.castType == CastType.Charged)
        {
            if (castingTime < currentSpell.chargeTime)
            {
                castingTime += Time.deltaTime;
            } 
            
            // Finish charging
            if (!finishedCasting && castingTime >= currentSpell.chargeTime)
            {
                castingTime = currentSpell.chargeTime;
                finishedCasting = true;
            }
        }
        
        // Channel spell
        if (currentSpell.castType == CastType.Channeled)
        {
            if (castingTime < currentSpell.channelRate)
            {
                castingTime += Time.deltaTime;
            }
            
            if (castingTime >= currentSpell.chargeTime)
            {
                castingTime = 0f;
                ICombatTarget target = GetTarget();
                spellCaster.CastSpell(currentSpell, target);
            }

        }


    }

    private void TryCastSpell(InputAction.CallbackContext context)
    {
        if (!currentSpell || !spellCaster.CanCast(currentSpell)) return;
        
        // Start casting
        if ((context.started || context.performed) && !isCasting)
        {
            switch (currentSpell.castType)
            {
                case CastType.Instant:
                    ICombatTarget target = GetTarget();
                    spellCaster.CastSpell(currentSpell, target);
                    break;
                    
                case CastType.Channeled:
                    // TODO: Start channeling
                    isCasting = true;
                    castingTime = 0f;
                    break;
                    
                case CastType.Charged:
                    isCasting = true;
                    finishedCasting = false;
                    castingTime = 0f;
                    break;
            }
            
            return;
        } 
        

        if (context.canceled && isCasting)
        {
            // Cast charge
            if (currentSpell.castType == CastType.Charged && castingTime >= currentSpell.chargeTime)
            {
                finishedCasting = false;                
                ICombatTarget target = GetTarget();
                spellCaster.CastSpell(currentSpell, target);
            }
            
            // Stop casting
            isCasting = false;


            castingTime = 0f;
        }
        

    }
    
    
    private ICombatTarget GetTarget()
    {
        if (currentSpell.targetingType == TargetingType.Self) return null;

        if (_cam && Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, maxTargetRange))
        {
            return hit.collider.GetComponent<ICombatTarget>();
        }
        
        return null;
    }
    
    public void SetSpell(SOSpell spell)
    {
        if (!spell) return;
        
        currentSpell = spell;
    }
}