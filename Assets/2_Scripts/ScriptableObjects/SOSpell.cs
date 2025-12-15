using System;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Spell", order = 1)]
public class SOSpell : ScriptableObject
{
    [Header("Info")]
    public string label = "New Spell";
    public Sprite icon;
    public string description = "Spell Description";
    
    [Header("Spell")]
    public float baseCost = 5f; 
    public SpellForm form = SpellForm.Invoke;
    [SerializeReference] public Augment augment = new NoneAugment();
    public Conjure conjurePrefab;
    public float conjureLifeTime = 5f;
    [SerializeReference] public ConjureMotionBehavior conjureMotion = new StraightMotion();
    [SerializeReference] public ConjureImpactBehavior conjureImpact = new DestroyBehavior();
    public List<Domain> domains = new List<Domain>();
    [SerializeReference] public SpellEffect[] effects = Array.Empty<SpellEffect>();

    private void OnValidate()
    {
        if (domains.Count == 0)
        {
            domains.Add(Domain.Arcane);
        }
    }

    // Cast with specific strength multiplier (determined by cast method at runtime)
    public void Cast(ICombatTarget source, ICombatTarget target, Transform castPoint, float strengthMultiplier)
    {
        switch (form)
        {
            case SpellForm.Imbue:
                ApplyEffects(source, source, strengthMultiplier);
                break;
            
            case SpellForm.Invoke:
                if (target != null) ApplyEffects(source, target, strengthMultiplier);
                break;
                
            case SpellForm.Conjure:
                SpawnProjectile(source, target, castPoint, strengthMultiplier);
                break;
        }
    }
    
    private void ApplyEffects(ICombatTarget source, ICombatTarget target, float strengthMultiplier)
    {
        if (effects == null || effects.Length == 0) return;
    
        Vector3 impactPoint = target.Transform.position;
        
        // Clone and apply strength multiplier to effects
        List<SpellEffect> scaledEffects = new List<SpellEffect>();
        foreach (var effect in effects)
        {
            var clonedEffect = effect.Clone();
            clonedEffect.ApplyStrengthMultiplier(strengthMultiplier);
            scaledEffects.Add(clonedEffect);
        }
        
        // Clone and apply strength multiplier to augment
        Augment scaledAugment = augment?.Clone() ?? new NoneAugment();
        scaledAugment.ApplyStrengthMultiplier(strengthMultiplier);
        
        scaledAugment.Apply(scaledEffects, domains, source, target, impactPoint);
    }
    
    private void SpawnProjectile(ICombatTarget source, ICombatTarget target, Transform castPoint, float strengthMultiplier)
    {
        if (!conjurePrefab)
        {
            Debug.LogError($"Spell {label} is set to Conjure but has no prefab!");
            return;
        }
        
        Conjure conjure = Instantiate(conjurePrefab, castPoint.position, Quaternion.identity);
        conjure.Initialize(this, source, target, strengthMultiplier);
    }
}