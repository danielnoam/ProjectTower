using System;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager), typeof(SpellCasterComponent))]
public class FPCCaster : MonoBehaviour
{
    [Header("Cast Method Settings")]
    [SerializeField] private float chargeTime = 1.5f; 
    [SerializeField] private float channelTickRate = 0.1f;
    
    [Header("Targeting")]
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
    [SerializeField, ReadOnly] private CastMethod currentCastMethod;
    [SerializeField, ReadOnly] private bool isCharging;
    [SerializeField, ReadOnly] private bool isChanneling;
    [SerializeField, ReadOnly] private float castHoldTime;
    [SerializeField, ReadOnly] private List<SOSpell> spellsList;
    
    private ICombatTarget _lockedTarget;
    private Camera _cam;
    private float _channelTickTimer;
    private bool _hasTransitionedToCharge;

    public SOSpell CurrentSpell => currentSpell;
    public IReadOnlyList<SOSpell> SpellsList => spellsList;

    public event Action<CastMethod> StartedSpellCast;
    public event Action<SOSpell> SpellChanged;
    public event Action<SOSpell> SpellAdded;
    public event Action<CastMethod, float, float> CastingProgressChanged;
    

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
        fpcManager.FpcInput.OnAttackAction += HandlePrimaryAttackInput; 
        fpcManager.FpcInput.OnAttack2Action += HandleSecondaryAttackInput; 
        spellCraftingStation.SpellCrafted += AddSpell;
    }

    private void OnDisable()
    {
        fpcManager.FpcInput.OnAttackAction -= HandlePrimaryAttackInput;
        fpcManager.FpcInput.OnAttack2Action -= HandleSecondaryAttackInput;
        spellCraftingStation.SpellCrafted -= AddSpell;
    }

    private void Update()
    {
        HandleSpellSwitch();
        
        if (!currentSpell) return;
        
        // Update charging progress and display state
        if (isCharging)
        {
            castHoldTime += Time.deltaTime;
            
            if (castHoldTime < 0.1f)
            {
                currentCastMethod = CastMethod.Instant;
            }
            else
            {
                currentCastMethod = CastMethod.Charge;
                
                if (!_hasTransitionedToCharge)
                {
                    _hasTransitionedToCharge = true;
                    StartedSpellCast?.Invoke(currentCastMethod);
                }
            }
            
            CastingProgressChanged?.Invoke(CastMethod.Charge, castHoldTime, chargeTime);
        }
        
        // Update channeling and cast on tick
        if (isChanneling)
        {
            castHoldTime += Time.deltaTime;
            _channelTickTimer += Time.deltaTime;
            currentCastMethod = CastMethod.Channel;
            
            if (_channelTickTimer >= channelTickRate)
            {
                spellCasterComponent.CastChanneled(currentSpell, _lockedTarget);
                _channelTickTimer = 0f;
            }
            
            CastingProgressChanged?.Invoke(CastMethod.Channel, _channelTickTimer, channelTickRate);
        }
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
    
    private void HandlePrimaryAttackInput(InputAction.CallbackContext context)
    {
        if (!currentSpell) return;
        
        if (context.started)
        {
            _lockedTarget = GetTargetAtCrosshair();
            isCharging = true;
            castHoldTime = 0f;
            currentCastMethod = CastMethod.Instant;
            StartedSpellCast?.Invoke(currentCastMethod);
        }
        
        if (context.canceled && isCharging)
        {
            if (castHoldTime < 0.1f)
            {
                spellCasterComponent.CastInstant(currentSpell, _lockedTarget);
            }
            else if (castHoldTime >= chargeTime)
            {
                spellCasterComponent.CastCharged(currentSpell, _lockedTarget);
            }
            else
            {
                // Released early - still instant
                spellCasterComponent.CastInstant(currentSpell, _lockedTarget);
            }
            
            StopCasting();
        }
    }
    
    private void HandleSecondaryAttackInput(InputAction.CallbackContext context)
    {
        if (!currentSpell) return;
        
        if (context.started)
        {
            _lockedTarget = GetTargetAtCrosshair();
            isChanneling = true;
            castHoldTime = 0f;
            _channelTickTimer = 0f;
            currentCastMethod = CastMethod.Channel;
            StartedSpellCast?.Invoke(currentCastMethod);
        }
        
        if (context.canceled && isChanneling)
        {
            StopCasting();
        }
    }
    
    private void StopCasting()
    {
        isCharging = false;
        isChanneling = false;
        castHoldTime = 0f;
        _channelTickTimer = 0f;
        _lockedTarget = null;
        _hasTransitionedToCharge = false;
        currentCastMethod = CastMethod.Instant;
        CastingProgressChanged?.Invoke(CastMethod.Instant, 0, 0);
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
    
    private ICombatTarget GetTargetAtCrosshair()
    {
        if (!_cam) return null;

        Vector3 rayOrigin = _cam.transform.position;
        Vector3 rayDirection = _cam.transform.forward;
    
        if (!Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxTargetRange, targetingLayers))
        {
            return null;
        }
    
        return hit.collider.GetComponent<ICombatTarget>();
    }
    
    private void OnDrawGizmos()
    {
        if (!showTargetingRay || !_cam) return;

        Vector3 rayOrigin = _cam.transform.position;
        Vector3 rayDirection = _cam.transform.forward;
    
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxTargetRange, targetingLayers))
        {
            var target = hit.collider.GetComponent<ICombatTarget>();
            Gizmos.color = target != null ? Color.green : Color.cyan;
            Gizmos.DrawLine(rayOrigin, hit.point);
            Gizmos.DrawWireSphere(hit.point, 0.1f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * maxTargetRange);
        }
    }
}