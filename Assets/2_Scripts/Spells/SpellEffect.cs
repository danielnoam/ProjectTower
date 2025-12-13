using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SpellEffect
{
    public virtual List<Domain> AvailableForDomains => new List<Domain> {};
    public abstract SpellEffect Clone();
    public abstract void Apply(ICombatTarget source, ICombatTarget target);
    public abstract void ApplyStrengthMultiplier(float multiplier);
    public abstract string GetDescription();
}

[System.Serializable]
[SpellEffect("Damage", manaCost: 10f)]
public class DamageHealthEffect : SpellEffect
{
    [Min(0)] public float damage = 15f;
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        damage *= multiplier;
    }
    
    public override string GetDescription()
    {
        return $"Deals {damage:F0} damage";
    }

    public override SpellEffect Clone()
    {
        return new DamageHealthEffect { damage = damage };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target?.TakeDamage(damage, source);
    }
    
    
}

[System.Serializable]
[SpellEffect("Heal", manaCost: 15f, AvailableDomains = new[] { Domain.Arcane })]
public class HealHealthEffect : SpellEffect
{
    [Min(0)] public float healAmount = 15f;
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        healAmount *= multiplier;
    }
    
    public override string GetDescription()
    {
        return $"Heals {healAmount:F0} health";
    }
    
    public override List<Domain> AvailableForDomains => new List<Domain> { Domain.Arcane};

    public override SpellEffect Clone()
    {
        return new HealHealthEffect { healAmount = healAmount };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target?.Heal(healAmount);
    }
}

[System.Serializable]
[SpellEffect("Push", manaCost: 25f, AvailableDomains = new[] { Domain.Arcane })]
public class PushEffect : SpellEffect
{
    [Min(0)] public float force = 25f;
    [Range(0f, 1f)] public float verticalInfluence = 0.3f; // How much vertical angle affects the push
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        force *= multiplier;
    }
    
    public override string GetDescription()
    {
        return $"Pushes {force:F0} units";
    }
    
    public override List<Domain> AvailableForDomains => new List<Domain> { Domain.Arcane};
    
    public override SpellEffect Clone()
    {
        return new PushEffect { force = force, verticalInfluence = verticalInfluence };
    }
    
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        Vector3 direction;
        
        if (source == target)
        {
            direction = source.LookDirection;
        }
        else
        {
            Vector3 sourcePos = source.Transform.position;
            Vector3 targetPos = target.Transform.position;
            
            Vector3 horizontalDir = new Vector3(
                targetPos.x - sourcePos.x,
                0,
                targetPos.z - sourcePos.z
            ).normalized;
            
            Vector3 lookDir = source.LookDirection;
            float verticalComponent = lookDir.y * verticalInfluence;
            
            direction = new Vector3(
                horizontalDir.x,
                verticalComponent,
                horizontalDir.z
            ).normalized;
        }
        
        target.ApplyForce(direction, force);
    }
}

[System.Serializable]
[SpellEffect("Pull", manaCost: 20f, AvailableDomains = new[] { Domain.Arcane })]
public class PullEffect : SpellEffect
{
    [Min(0)] public float force = 25f;
    [Range(0f, 1f)] public float verticalInfluence = 0.3f;
    
    public override string GetDescription()
    {
        return $"Pulls {force:F0} units";
    }
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        force *= multiplier;
    }
    
    public override List<Domain> AvailableForDomains => new List<Domain> { Domain.Arcane};
    
    public override SpellEffect Clone()
    {
        return new PullEffect { force = force, verticalInfluence = verticalInfluence };
    }
    
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        Vector3 direction;
        
        if (source == target)
        {
            direction = -source.LookDirection;
        }
        else
        {

            Vector3 sourcePos = source.Transform.position;
            Vector3 targetPos = target.Transform.position;
            
            Vector3 horizontalDir = new Vector3(
                sourcePos.x - targetPos.x,
                0,
                sourcePos.z - targetPos.z
            ).normalized;

            Vector3 lookDir = source.LookDirection;
            float verticalComponent = Mathf.Abs(lookDir.y * verticalInfluence);
            
            direction = new Vector3(
                horizontalDir.x,
                verticalComponent,
                horizontalDir.z
            ).normalized;
        }
        
        target.ApplyForce(direction, force);
    }
}

[System.Serializable]
[SpellEffect("Leech", ManaCost = 20f)]
public class LeechEffect : SpellEffect
{
    [Min(0)] public float damage = 10f;
    [Range(0f, 1f)] public float lifestealPercent = 0.5f;
    
    public override string GetDescription()
    {
        return $"Deals {damage:F0} damage and heals {lifestealPercent:P0} of that damage";
    }
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        damage *= multiplier;
    }

    public override SpellEffect Clone()
    {
        return new LeechEffect { damage = damage, lifestealPercent = lifestealPercent };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target.TakeDamage(damage, source);
        float healAmount = damage * lifestealPercent;
        source.Heal(healAmount);
    }
}

[System.Serializable]
[SpellEffect("Burn Mana", ManaCost = 15f)]
public class BurnManaEffect : SpellEffect
{
    [Min(0)] public float amount = 25f;
    
    public override string GetDescription()
    {
        return $"Burns {amount:F0} mana";
    }
    
    public override void ApplyStrengthMultiplier(float multiplier)
    {
        amount *= multiplier;
    }
    
    public override SpellEffect Clone()
    {
        return new BurnManaEffect { amount = amount };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        MonoBehaviour targetMono = target as MonoBehaviour;
        if (targetMono)
        {
            if (targetMono.TryGetComponent(out SpellCasterComponent spellCasterComponent))
            {
                spellCasterComponent.TryConsume(amount);
            }
        }
    }
}