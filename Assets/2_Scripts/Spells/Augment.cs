using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class Augment
{
    
    public virtual List<SpellForm> CompatibleForms => new List<SpellForm> 
    { 
        SpellForm.Imbue, 
        SpellForm.Invoke, 
        SpellForm.Conjure 
    };
    
    public abstract Augment Clone();
    public abstract void ApplyStrengthMultiplier(float multiplier);
    public abstract string GetDescription();
    
    public abstract void Apply(List<SpellEffect> effects, List<Domain> domains, ICombatTarget source, ICombatTarget primaryTarget, Vector3 impactPoint);
    
    protected void ApplyEffectsToTarget(List<SpellEffect> effects, ICombatTarget source, ICombatTarget target, float strengthMultiplier)
    {
        foreach (var effect in effects)
        {
            var clonedEffect = effect.Clone();
            clonedEffect.ApplyStrengthMultiplier(strengthMultiplier);
            clonedEffect.Apply(source, target);
        }
    }
    
    protected void ApplyDomainStatusEffects(List<Domain> domains, ICombatTarget source, ICombatTarget target)
    {
        if (domains == null || domains.Count == 0) return;
        
        float statusChance = DomainProperties.GetStatusChance(domains.Count);
        
        foreach (var domain in domains)
        {
            if (Random.value <= statusChance)
            {
                var status = DomainProperties.GetStatusEffect(domain);
                if (status != null)
                {
                    target.ApplyStatus(status);
                }
            }
            
            DomainProperties.ApplyDomainKnockback(domain, source, target);
        }
    }
}



[System.Serializable]
[Augment("None", 0f)]
public class NoneAugment : Augment
{
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
    }
    
    public override string GetDescription()
    {
        return "";
    }
    
    public override Augment Clone()
    {
        return new NoneAugment();
    }
    
    public override void Apply(List<SpellEffect> effects, List<Domain> domains, ICombatTarget source, ICombatTarget primaryTarget, Vector3 impactPoint)
    {
        ApplyEffectsToTarget(effects, source, primaryTarget, 1f);
        ApplyDomainStatusEffects(domains, source, primaryTarget);
    }
    
    
}

[System.Serializable]
[Augment("Chain", 10f)]
public class ChainAugment : Augment
{
    [Min(1)] public int chainCount = 3;
    [Min(0)] public float chainRange = 10f;
    [Range(0f, 1f)] public float damageReductionPerChain = 0.3f;
    
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        chainCount = Mathf.Clamp(Mathf.RoundToInt(chainCount * multiplier), 1, int.MaxValue);
        chainRange *= multiplier;
        damageReductionPerChain *= multiplier;
    }
    
    public override string GetDescription()
    {
        return $"Chains to {chainCount} targets within {chainRange}m, losing {damageReductionPerChain:P0} strength per chain";
    }
    
    public override Augment Clone()
    {
        return new ChainAugment 
        { 
            chainCount = chainCount,
            chainRange = chainRange,
            damageReductionPerChain = damageReductionPerChain
        };
    }
    
    public override void Apply(List<SpellEffect> effects, List<Domain> domains, ICombatTarget source, ICombatTarget primaryTarget, Vector3 impactPoint)
    {
        ApplyEffectsToTarget(effects, source, primaryTarget, 1f);
        ApplyDomainStatusEffects(domains, source, primaryTarget);
        
        HashSet<ICombatTarget> hitTargets = new HashSet<ICombatTarget> { primaryTarget };
        ICombatTarget currentTarget = primaryTarget;
        float currentMultiplier = 1f;
        
        for (int i = 0; i < chainCount; i++)
        {
            ICombatTarget nextTarget = FindNearestTarget(
                currentTarget.Transform.position, 
                chainRange, 
                hitTargets, 
                source
            );
            
            if (nextTarget == null) break;
            
            currentMultiplier *= (1f - damageReductionPerChain);
            ApplyEffectsToTarget(effects, source, nextTarget, currentMultiplier);
            ApplyDomainStatusEffects(domains, source, nextTarget);
            
            hitTargets.Add(nextTarget);
            currentTarget = nextTarget;
        }
    }
    
    private ICombatTarget FindNearestTarget(Vector3 fromPosition, float range, HashSet<ICombatTarget> exclude, ICombatTarget source)
    {
        Collider[] colliders = Physics.OverlapSphere(fromPosition, range);
        ICombatTarget nearest = null;
        float nearestDist = float.MaxValue;
        
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out ICombatTarget target))
            {
                if (exclude.Contains(target) || target == source) 
                    continue;
                
                float dist = Vector3.Distance(fromPosition, target.Transform.position);
                if (dist < nearestDist)
                {
                    nearest = target;
                    nearestDist = dist;
                }
            }
        }
        
        return nearest;
    }
}

[System.Serializable]
[Augment("Area of Effect", 15f)]
public class AreaOfEffectAugment : Augment
{
    [Min(0)] public float radius = 5f;
    [Range(0f, 1f)] public float edgeFalloff = 0.5f;
    
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        radius *= multiplier;
    }
    
    public override string GetDescription()
    {
        return $"Affects all targets in {radius}m radius, {edgeFalloff:P0} falloff at edges";
    }
    
    public override Augment Clone()
    {
        return new AreaOfEffectAugment 
        { 
            radius = radius,
            edgeFalloff = edgeFalloff
        };
    }
    
    public override void Apply(List<SpellEffect> effects, List<Domain> domains, ICombatTarget source, ICombatTarget primaryTarget, Vector3 impactPoint)
    {
        ApplyEffectsToTarget(effects, source, primaryTarget, 1f);
        ApplyDomainStatusEffects(domains, source, primaryTarget);
        
        Collider[] colliders = Physics.OverlapSphere(impactPoint, radius);
    
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out ICombatTarget target))
            {
                // Skip source and primary target (already hit)
                if (target == source || target == primaryTarget) 
                    continue;
            
                float distance = Vector3.Distance(impactPoint, target.Transform.position);
                float normalizedDistance = Mathf.Clamp01(distance / radius);
                float strengthMultiplier = Mathf.Lerp(1f, 1f - edgeFalloff, normalizedDistance);
            
                ApplyEffectsToTarget(effects, source, target, strengthMultiplier);
                ApplyDomainStatusEffects(domains, source, target);
            }
        }
    }
}