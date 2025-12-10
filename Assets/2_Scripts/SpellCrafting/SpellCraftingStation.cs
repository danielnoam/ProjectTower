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
    [Header("Settings")]
    [SerializeField] private SpellCraftingUI spellCraftingUI;
    [SerializeField] private Interactable interactable;
    [SerializeField] private Projectile defaultProjectilePrefab;


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
        spellCraftingUI.Open();
    }
    
    private string GenerateSpellName(SpellCraftingData data)
    {
        if (data.effectTypes.Count == 0)
        {
            return "Empty Spell";
        }
        
        if (data.effectTypes.Count == 1)
        {
            string effectName = SpellTypeRegistry.GetEffectDisplayName(data.effectTypes[0]);
            return $"{effectName} Spell";
        }
        
        return $"Multi-Effect Spell ({data.effectTypes.Count})";
    }
    
    public float CalculateManaCost(SpellCraftingData data)
    {
        float cost = 5f;
        cost += data.effectTypes.Count * 3f;
        
        if (data.spellForm == SpellForm.Conjure) cost += 5f;
        if (data.spellForm == SpellForm.Invoke) cost += 2.5f;
        if (data.castMethod == CastMethod.Charge) cost *= 0.5f;
        if (data.castMethod == CastMethod.Channel) cost *= 1.5f;
        
        return cost;
    }
    

    public SOSpell CreateSpell(SpellCraftingData data)
    {
        SOSpell spell = ScriptableObject.CreateInstance<SOSpell>();
        
        // Basic properties
        var spellName = GenerateSpellName(data);
        spell.name = spellName;
        spell.label = spellName;
        spell.castMethod = data.castMethod;
        spell.spellForm = data.spellForm;
        spell.domain = data.domain;
        spell.manaCost = CalculateManaCost(data);
        
        // Channel/Charge specifics
        if (data.castMethod == CastMethod.Channel)
        {
            spell.channelRate = 0.15f; // Default value
        }
        if (data.castMethod == CastMethod.Charge)
        {
            spell.chargeTime = 1.5f; // Default value
        }
        
        // Create effects from types
        spell.effects = CreateEffects(data.effectTypes);
        
        // Conjure settings
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
        
        return spell;
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
    
}