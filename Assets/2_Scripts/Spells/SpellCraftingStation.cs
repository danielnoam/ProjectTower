using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Data structure for creating spells - no cast method, that's decided at cast time
public class SpellCraftingData
{
    public SpellForm spellForm = SpellForm.Invoke;
    public readonly List<Domain> domains = new();
    public readonly List<Type> effectTypes = new();
    
    public Type augmentType;
    public Type movementType;
    public Type collisionType;
    public SOConjureGeometric geometric;
    
    public SpellCraftingData()
    {
        augmentType = SpellTypeRegistry.AugmentTypes.FirstOrDefault(t => t == typeof(NoneAugment));
        if (augmentType == null && SpellTypeRegistry.AugmentTypes.Count > 0)
        {
            augmentType = SpellTypeRegistry.AugmentTypes[0];
        }
        
        if (SpellTypeRegistry.MotionTypes.Count > 0)
        {
            movementType = SpellTypeRegistry.MotionTypes[0];
        }
        
        if (SpellTypeRegistry.ImpactTypes.Count > 0)
        {
            collisionType = SpellTypeRegistry.ImpactTypes[0];
        }
    }
}


public class SpellCraftingStation : MonoBehaviour
{
    [Header("Spell Form")]
    [SerializeField] private float imbueCost = 5f;
    [SerializeField] private float invokeCost = 7f;
    [SerializeField] private float conjureCost = 9f;
    
    [Header("Geometric")]
    [SerializeField] private List<SOConjureGeometric> availableGeometrics;
    
    [Header("Domain")]
    [SerializeField] private int maxDomains = 4;

    [Header("Effects")]
    [SerializeField] private int maxEffects = 4;
    
    [Header("References")]
    [SerializeField] private Interactable interactable;

    public int MaxEffects => maxEffects;
    public int MaxDomains => maxDomains;
    public List<SOConjureGeometric> AvailableGeometrics => availableGeometrics;
    
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
    
    // Calculate BASE mana cost - no cast method multipliers
    public float CalculateManaCost(SpellCraftingData data)
    {
        float cost = 0;
        
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
                if (data.geometric) cost *= data.geometric.costMultiplier;
                break;
        }

        foreach (var effectType in data.effectTypes)
        {
            cost += SpellTypeRegistry.GetEffectManaCost(effectType);
        }
        
        cost += SpellTypeRegistry.GetAugmentManaCost(data.augmentType);
        
        return cost;
    }

    public float CalculateConjureDuration(SpellCraftingData data)
    {
        var duration = 0f;
        
        var movement = SpellTypeRegistry.CreateMotion(data.movementType);
        if (movement != null)
        {
            duration += movement.Duration;
        }
    
        return duration;
    }
    
    private string GenerateSpellName(SpellCraftingData data)
    {
        string spellName = "";
        
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

    // Create effects at base strength - no cast method multipliers
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
        spell.form = data.spellForm;
        spell.domains = new List<Domain>(data.domains);
        spell.baseCost = CalculateManaCost(data); 
        spell.conjureLifeTime = CalculateConjureDuration(data);
        
        // Create effects at base strength
        spell.effects = CreateEffects(data.effectTypes);
        spell.augment = SpellTypeRegistry.CreateAugment(data.augmentType);

        if (data.spellForm == SpellForm.Conjure)
        {
            if (!data.geometric || !data.geometric.prefab)
            {
                Debug.LogError("No geometric selected or geometric has no prefab!");
                return null;
            }
            
            spell.conjurePrefab = data.geometric.prefab;
            spell.conjureMotion = SpellTypeRegistry.CreateMotion(data.movementType);
            spell.conjureImpact = SpellTypeRegistry.CreateImpact(data.collisionType);
        }
        
        SpellCrafted?.Invoke(spell);
        return spell;
    }
}