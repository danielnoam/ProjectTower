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
    [SerializeField] private SOSpell spell1;
    [SerializeField] private SOSpell spell2;
    [SerializeField] private SOSpell spell3;
    [SerializeField] private SOSpell spell4;
    [SerializeField] private SOSpell spell5;
    
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
        
        // Change spell
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpell(spell1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpell(spell2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpell(spell3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetSpell(spell4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetSpell(spell5);
        
        
        if (!currentSpell || !isCasting) return;
        
        // Charge spell
        if (currentSpell.castMethod == CastMethod.Charge)
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
        if (currentSpell.castMethod == CastMethod.Channel)
        {
            if (castingTime < currentSpell.channelRate)
            {
                castingTime += Time.deltaTime;
            }
            
            if (castingTime >= currentSpell.channelRate)
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
            switch (currentSpell.castMethod)
            {
                case CastMethod.Instant:
                    ICombatTarget target = GetTarget();
                    spellCaster.CastSpell(currentSpell, target);
                    break;
                    
                case CastMethod.Channel:
                    isCasting = true;
                    castingTime = 0f;
                    break;
                    
                case CastMethod.Charge:
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
            if (currentSpell.castMethod == CastMethod.Charge && castingTime >= currentSpell.chargeTime)
            {
                ICombatTarget target = GetTarget();
                spellCaster.CastSpell(currentSpell, target);
            }
            
            // Stop casting
            isCasting = false;
            finishedCasting = false; 
            castingTime = 0f;
        }
        

    }
    
    
    private ICombatTarget GetTarget()
    {
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