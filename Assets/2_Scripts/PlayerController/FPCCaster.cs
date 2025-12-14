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
    [SerializeField] private LayerMask targetingLayers = ~0;
    
    [Header("Debug")]
    [SerializeField] private bool showTargetingRay = true;
    
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
    private ICombatTarget _lockedTarget;
    private Camera _cam;

    public SOSpell CurrentSpell => currentSpell;
    public IReadOnlyList<SOSpell> SpellsList => spellsList;
    
    public event Action<SOSpell> SpellChanged;
    public event Action<SOSpell> SpellAdded;
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
        HandleSpellSwitch();
        
        if (!currentSpell || !isCasting) return;
        
        // Charge spell
        if (currentSpell.castMethod == CastMethod.Charge)
        {
            HandleChargeSpell();
        }
        
        // Channel spell
        if (currentSpell.castMethod == CastMethod.Channel)
        {
            HandleChannelSpell();
        }
    }



    private ICombatTarget GetTargetAtCrosshair()
    {
        if (!_cam)
        {
            return null;
        }

        Vector3 rayOrigin = _cam.transform.position;
        Vector3 rayDirection = _cam.transform.forward;
    
        if (!Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxTargetRange, targetingLayers))
        {
            return null;
        }
    
        return hit.collider.GetComponent<ICombatTarget>();
    }
    
    

    private void HandleSpellSwitch()
    {
        if (spellsList.Count <= 1) return;
        
        if (Input.mouseScrollDelta.y > 0)
        {
            int currentIndex = spellsList.IndexOf(currentSpell);
            int nextIndex = (currentIndex + 1) % spellsList.Count;
            SetSpell(spellsList[nextIndex]);
        } 
        else if (Input.mouseScrollDelta.y < 0)
        {
            int currentIndex = spellsList.IndexOf(currentSpell);
            int nextIndex = currentIndex - 1;
            if (nextIndex < 0) nextIndex = spellsList.Count - 1;
            SetSpell(spellsList[nextIndex]);
        }
    }

    private void HandleChargeSpell()
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

    private void HandleChannelSpell()
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
    
            spellCasterComponent.CastSpell(currentSpell, _lockedTarget);
        }
        
        SpellCastingProgressChanged?.Invoke(castingTime, currentSpell.channelRate);
    }

    private void TryCastSpell(InputAction.CallbackContext context)
    {
        if (!currentSpell || !spellCasterComponent.CanCast(currentSpell)) return;
        
        // Start casting
        if (context.started && !isCasting)
        {
            // Lock target when casting starts
            _lockedTarget = GetTargetAtCrosshair();
            
            switch (currentSpell.castMethod)
            {
                case CastMethod.Instant:
                    spellCasterComponent.CastSpell(currentSpell, _lockedTarget);
                    _lockedTarget = null; // Clear immediately for instant casts
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
                spellCasterComponent.CastSpell(currentSpell, _lockedTarget);
            }
            
            StopCasting();
        }
    }
    
    private void StopCasting()
    {
        isCasting = false;
        finishedCasting = false;
        castingTime = 0f;
        _lockedTarget = null; 
        SpellCastingProgressChanged?.Invoke(castingTime, 0);
    }
    
    private void AddSpell(SOSpell spell)
    {
        if (!spell) return;

        if (!spellsList.Contains(spell)) spellsList.Add(spell);
        
        SpellAdded?.Invoke(spell);
        
        if (!currentSpell) SetSpell(spell);
    }
    
    private void SetSpell(SOSpell spell)
    {
        if (!spell) return;
        
        currentSpell = spell;
        SpellChanged?.Invoke(currentSpell);
        StopCasting();
    }
    
    private void OnDrawGizmos()
    {
        if (!showTargetingRay || !_cam) return;

        Vector3 rayOrigin = _cam.transform.position;
        Vector3 rayDirection = _cam.transform.forward;
    
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxTargetRange, targetingLayers))
        {
            var target = hit.collider.GetComponent<ICombatTarget>();
        
            // Choose color based on what we hit
            if (target != null)
            {
                Gizmos.color = Color.green; // Valid target
            }
            else
            {
                Gizmos.color = Color.cyan; // Hit something without ICombatTarget
            }
        
            // Draw line to hit point
            Gizmos.DrawLine(rayOrigin, hit.point);
        
            // Draw sphere at hit point
            Gizmos.DrawWireSphere(hit.point, 0.1f);
        }
        else
        {
            // No hit - draw red line to max range
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * maxTargetRange);
        }
    }
}