using System;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager), typeof(SpellCasterComponent))]
public class FPCCaster : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxTargetRange = 50f;
    
    [Header("References")]
    [SerializeField] private FPCManager fpcManager;
    [SerializeField] private SpellCasterComponent spellCasterComponent;
    [SerializeField] private SpellCraftingStation spellCraftingStation;
    
    [Separator]
    [SerializeField] private SOSpell currentSpell;
    [SerializeField, ReadOnly] private bool isCasting;
    [SerializeField, ReadOnly] private bool finishedCasting;
    [SerializeField, ReadOnly] private float castingTime;
    [SerializeField, ReadOnly] private List<SOSpell> spellsList;
    private Camera _cam;

    public SOSpell CurrentSpell => currentSpell;
    public event Action<SOSpell> SpellChanged;
    public event Action<float, float> SpellCastingProgressChanged;
    
    

    private void OnValidate()
    {
        if (!fpcManager) fpcManager = this.GetOrAddComponent<FPCManager>();
        if (!spellCasterComponent) spellCasterComponent = this.GetOrAddComponent<SpellCasterComponent>();
    }
    
    private void Awake()
    {
        _cam = Camera.main;
        
        if (currentSpell) AddSpell(currentSpell);
    }
    


    private void OnEnable()
    {
        fpcManager.FpcInput.OnAttackAction += TryCastSpell;
        spellCraftingStation.SpellCrafted += AddSpell;
    }

    private void OnDisable()
    {
        fpcManager.FpcInput.OnAttackAction -= TryCastSpell;
        spellCraftingStation.SpellCrafted -= AddSpell;
    }

    private void Update()
    {
        // Change spell
        if (Input.mouseScrollDelta.y > 0)
        {
            if (spellsList.Count <= 1) return;
            int currentIndex = spellsList.IndexOf(currentSpell);
            int nextIndex = currentIndex + 1;
            if (nextIndex >= spellsList.Count) nextIndex = 0;
            SetSpell(spellsList[nextIndex]);
            
        } 
        else if (Input.mouseScrollDelta.y < 0)
        {
            if (spellsList.Count <= 1) return;
            int currentIndex = spellsList.IndexOf(currentSpell);
            int nextIndex = currentIndex - 1;
            if (nextIndex < 0) nextIndex = spellsList.Count - 1;
            SetSpell(spellsList[nextIndex]);
        }
        
        if (!currentSpell || !isCasting) return;
        
        // Charge spell
        if (currentSpell.castMethod == CastMethod.Charge)
        {
            if (!spellCasterComponent.CanCast(currentSpell))
            {
                StopCasting();
                return;
            }
    
            if (castingTime < currentSpell.chargeTime)
            {
                castingTime += Time.deltaTime;
            } 
            
            if (!finishedCasting && castingTime >= currentSpell.chargeTime)
            {
                castingTime = currentSpell.chargeTime;
                finishedCasting = true;
            }
            
            SpellCastingProgressChanged?.Invoke(castingTime, currentSpell.chargeTime);
        }
        
        // Channel spell
        if (currentSpell.castMethod == CastMethod.Channel)
        {
            castingTime += Time.deltaTime;
    
            if (castingTime >= currentSpell.channelRate)
            {
                castingTime = 0f; 
                if (!isCasting || !spellCasterComponent.CanCast(currentSpell))
                {
                    StopCasting();
                    return;
                }
        
                ICombatTarget target = GetTarget();
                spellCasterComponent.CastSpell(currentSpell, target);
            }
            
            SpellCastingProgressChanged?.Invoke(castingTime, currentSpell.channelRate);
        }

    }

    private void TryCastSpell(InputAction.CallbackContext context)
    {
        if (!currentSpell || !spellCasterComponent.CanCast(currentSpell)) return;
        
        // Start casting
        if (context.started && !isCasting)
        {
            switch (currentSpell.castMethod)
            {
                case CastMethod.Instant:
                    ICombatTarget target = GetTarget();
                    spellCasterComponent.CastSpell(currentSpell, target);
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
                spellCasterComponent.CastSpell(currentSpell, target);
            }
            
            StopCasting();
        }
        

    }
    
    private void StopCasting()
    {
        isCasting = false;
        finishedCasting = false;
        castingTime = 0f;
        SpellCastingProgressChanged?.Invoke(castingTime, 0);
    }
    
    
    private ICombatTarget GetTarget()
    {
        if (_cam && Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, maxTargetRange))
        {
            var target = hit.collider.GetComponent<ICombatTarget>();
            if (target != fpcManager as ICombatTarget) return target;
        }
        
        return null;
    }
    
    private void AddSpell(SOSpell spell)
    {
        if (!spell) return;

        if (!spellsList.Contains(spell)) spellsList.Add(spell);
        
        if (!currentSpell) SetSpell(spell);
    }
    
    private void SetSpell(SOSpell spell)
    {
        if (!spell) return;
        
        currentSpell = spell;
        SpellChanged?.Invoke(currentSpell);
        StopCasting();
    }
}