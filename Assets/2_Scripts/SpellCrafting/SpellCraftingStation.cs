using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellCraftingData
{
    public CastMethod castMethod = CastMethod.Instant;
    public SpellForm spellForm = SpellForm.Invoke;
    public Domain domain = Domain.Arcane;
    public readonly List<Type> effectTypes = new();
    
    public Type movementType;
    public Type collisionType;
    
    public SpellCraftingData()
    {
        if (SpellTypeRegistry.MovementTypes.Count > 0)
        {
            movementType = SpellTypeRegistry.MovementTypes[0];
        }
        
        if (SpellTypeRegistry.CollisionTypes.Count > 0)
        {
            collisionType = SpellTypeRegistry.CollisionTypes[0];
        }
    }
    
}


public class SpellCraftingStation : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private int maxEffects = 3;
    
    [Header("Cast Method")]
    [SerializeField] private float defaultChannelRate = 0.15f;
    [SerializeField] private float defaultChargeRate = 1.5f;
    
    [Header("Cost Calculation")]
    [SerializeField] private float baseManaCost = 5f;
    [SerializeField] private float manaCostPerEffect = 3f;
    [Space(10f)]
    [SerializeField] private float imbueCost;
    [SerializeField] private float invokeCost = 2f;
    [SerializeField] private float conjureCost = 4f;
    [Space(10f)]
    [SerializeField] private float instantCostMultiplier = 1f;
    [SerializeField] private float chargeCostMultiplier = 0.5f;
    [SerializeField] private float channelCostMultiplier = 1.5f;
    
    
    [Header("References")]
    [SerializeField] private Interactable interactable;
    [SerializeField] private Projectile defaultProjectilePrefab;

    
    public int MaxEffects => maxEffects;
    public event Action Opened;
    public event Action<SOSpell> SpellCrafted;
    

    private void OnEnable()
    {
        interactable.OnInteract += OnInteract;
    }

    private void OnDisable()
    {
        interactable.OnInteract -= OnInteract;
    }
    
    private void OnInteract(FPCInteraction interactor)
    {
        Opened?.Invoke();
    }
    
    public float CalculateManaCost(SpellCraftingData data)
    {
        float cost = baseManaCost;
        cost += data.effectTypes.Count * manaCostPerEffect;
        
        switch (data.spellForm)
        {
            case SpellForm.Imbue:
                cost += imbueCost;
                break;
            case SpellForm.Invoke:
                cost += invokeCost;
                break;
            case SpellForm.Conjure:
                cost += conjureCost;
                break;
        }

        switch (data.castMethod)
        {
            case CastMethod.Instant:
                cost *= instantCostMultiplier;
                break;
            case CastMethod.Charge:
                cost *= chargeCostMultiplier;
                break;
            case CastMethod.Channel:
                cost *= channelCostMultiplier;
                break;
        }

        return cost;
    }

    
    
    private string GenerateSpellName(SpellCraftingData data)
    {
        string spellName = "";

        switch (data.castMethod)
        {
            case CastMethod.Instant:
                spellName += "Instant ";
                break;
            case CastMethod.Charge:
                spellName += "Charged ";
                break;
            case CastMethod.Channel:
                spellName += "Channeled ";
                break;
        }
        
        switch (data.spellForm)
        {
            case SpellForm.Imbue:
                spellName += "Imbued ";
                break;
            case SpellForm.Invoke:
                spellName += "Invoked ";
                break;
            case SpellForm.Conjure:
                spellName += "Conjured ";
                break;
        }
        
        switch (data.effectTypes.Count)
        {
            case 0:
                spellName += "Empty Spell";
                break;
            case 1:
                
                string effectName = SpellTypeRegistry.GetEffectDisplayName(data.effectTypes[0]);
                spellName += $"{effectName} Spell";
                break;
            
            default:
                spellName += $"Multi-Effect Spell ({data.effectTypes.Count})";
                break;
        }


        return spellName;

    }
    
    private string GenerateSpellDescription(SpellCraftingData data)
    {
        string description = "";

        switch (data.castMethod)
        {
            case CastMethod.Instant:
                description += "Instant ";
                break;
            case CastMethod.Charge:
                description += "Charged ";
                break;
            case CastMethod.Channel:
                description += "Channeled ";
                break;
        }
        
        switch (data.spellForm)
        {
            case SpellForm.Imbue:
                description += "Imbued ";
                break;
            case SpellForm.Invoke:
                description += "Invoked ";
                break;
            case SpellForm.Conjure:
                description += "Conjured ";
                break;
        }

        foreach (var effectType in data.effectTypes)
        {
            description += $"{SpellTypeRegistry.GetEffectDisplayName(effectType)} ";
        }
        
        return description;

    }

    private SpellEffect[] CreateEffects(List<Type> effectTypes)
    {
        SpellEffect[] effects = new SpellEffect[effectTypes.Count];
        
        for (int i = 0; i < effectTypes.Count; i++)
        {
            effects[i] = SpellTypeRegistry.CreateEffect(effectTypes[i]);
        }
        
        return effects;
    }
    
    public SOSpell CreateSpell(SpellCraftingData data)
    {
        SOSpell spell = ScriptableObject.CreateInstance<SOSpell>();
        

        var spellName = GenerateSpellName(data);
        spell.name = spellName;
        spell.label = spellName;
        spell.description = GenerateSpellDescription(data);
        spell.castMethod = data.castMethod;
        spell.spellForm = data.spellForm;
        spell.domain = data.domain;
        spell.manaCost = CalculateManaCost(data);
        

        switch (data.castMethod)
        {
            case CastMethod.Channel:
                spell.channelRate = defaultChannelRate;
                break;
            case CastMethod.Charge:
                spell.chargeTime = defaultChargeRate;
                break;
        }


        spell.effects = CreateEffects(data.effectTypes);
        

        if (data.spellForm == SpellForm.Conjure)
        {
            if (!defaultProjectilePrefab)
            {
                Debug.LogError("No default projectile prefab assigned!");
            }
            
            spell.projectilePrefab = defaultProjectilePrefab;
            spell.projectileMovement = SpellTypeRegistry.CreateMovement(data.movementType);
            spell.projectileCollision = SpellTypeRegistry.CreateCollision(data.collisionType);
        }
        
        SpellCrafted?.Invoke(spell);
        return spell;
    }
    

    
}