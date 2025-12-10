using UnityEngine;

[System.Serializable]
public abstract class SpellEffect
{
    public abstract SpellEffect Clone();
    public abstract void Apply(ICombatTarget source, ICombatTarget target);
}

[System.Serializable]
[SpellEffect("Damage")]
public class DamageHealthEffect : SpellEffect
{
    [Min(0)] public float damage = 15f;
    
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
[SpellEffect("Heal")]
public class HealHealthEffect : SpellEffect
{
    [Min(0)] public float healAmount = 15f;

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
[SpellEffect("Push")]
public class PushEffect : SpellEffect
{
    [Min(0)] public float force = 125f;
    
    public enum LookDirection { Source, Target }
    
    public override SpellEffect Clone()
    {
        return new PushEffect { force = force};
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        var direction = source == target
            ? source.LookDirection
            : (target.Transform.position - source.Transform.position).normalized;

        Vector3 forceVector = direction * force;
        target.ApplyForce(forceVector);
    
    }
}

[System.Serializable]
[SpellEffect("Pull")]
public class PullEffect : SpellEffect
{
    [Min(0)] public float force = 10f;
    
    public enum LookDirection { Source, Target }
    
    public override SpellEffect Clone()
    {
        return new PullEffect { force = force };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        var direction = source == target
            ? -source.LookDirection
            : (source.Transform.position - target.Transform.position).normalized;

        Vector3 forceVector = direction * force;
        target.ApplyForce(forceVector);
    }
}

[System.Serializable]
[SpellEffect("Leech")]
public class LeechEffect : SpellEffect
{
    [Min(0)] public float damage = 10f;
    [Range(0f, 1f)] public float lifestealPercent = 0.5f;

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
[SpellEffect("Burn Mana")]
public class BurnManaEffect : SpellEffect
{
    [Min(0)] public float amount = 10f;
    
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