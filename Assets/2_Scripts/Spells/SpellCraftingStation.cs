using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellCraftingData
{
    public CastMethod castMethod = CastMethod.Instant;
    public SpellForm spellForm = SpellForm.Invoke;
    public readonly List<Domain> domains = new();
    public readonly List<Type> effectTypes = new();
    
    public Type augmentType;
    public Type movementType;
    public Type collisionType;
    public SOConjureGeometric geometric; // NEW
    
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
        
        // Geometric will be set from SpellCraftingStation's default
    }
}


public class SpellCraftingStation : MonoBehaviour
{
    [Header("Spell Form")]
    [SerializeField] private float imbueCost = 5f;
    [SerializeField] private float invokeCost = 7f;
    [SerializeField] private float conjureCost = 9f;
    
    [Header("Cast Method")]
    [SerializeField] private float defaultChannelRate = 0.1f;
    [Space(5)]
    [SerializeField] private float defaultChargeRate = 1.5f;
    [SerializeField] private float instantCostMultiplier = 1f;
    [SerializeField] private float chargeCostMultiplier = 0.75f;
    [SerializeField] private float channelCostMultiplier = 1.25f;
    [Space(5)]
    [SerializeField] private float instantStrengthMultiplier = 1f;
    [SerializeField] private float chargeStrengthMultiplier = 2f;
    [SerializeField] private float channelStrengthMultiplier = 0.3f;

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


    private SpellEffect[] CreateEffects(List<Type> effectTypes, float strengthMultiplier)
    {
        SpellEffect[] effects = new SpellEffect[effectTypes.Count];
    
        for (int i = 0; i < effectTypes.Count; i++)
        {
            effects[i] = SpellTypeRegistry.CreateEffect(effectTypes[i]);
            effects[i].ApplyStrengthMultiplier(strengthMultiplier);
        }
    
        return effects;
    }
    
    public float GetCastMethodStrengthMultiplier(CastMethod castMethod)
    {
        switch (castMethod)
        {
            case CastMethod.Instant:
                return instantStrengthMultiplier;
            case CastMethod.Charge:
                return chargeStrengthMultiplier;
            case CastMethod.Channel:
                return channelStrengthMultiplier;
            default:
                return 1f;
        }
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
        spell.domains = new List<Domain>(data.domains);
        spell.manaCost = CalculateManaCost(data);
        spell.conjureLifeTime = CalculateConjureDuration(data);
        

        switch (data.castMethod)
        {
            case CastMethod.Channel:
                spell.channelRate = defaultChannelRate;
                break;
            case CastMethod.Charge:
                spell.chargeTime = defaultChargeRate;
                break;
        }


        float strengthMultiplier = GetCastMethodStrengthMultiplier(data.castMethod);
        spell.effects = CreateEffects(data.effectTypes, strengthMultiplier);
        
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