using UnityEngine;

[System.Serializable]
public abstract class SpellEffect
{
    public abstract SpellEffect Clone();
    public abstract void Apply(ICombatTarget source, ICombatTarget target);
}

[System.Serializable]
public class DamageHealthEffect : SpellEffect
{
    [Min(0)] public float damage = 5f;
    
    public override SpellEffect Clone()
    {
        return new DamageHealthEffect { damage = damage };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target?.TakeDamage(damage);
    }
}

[System.Serializable]
public class HealHealthEffect : SpellEffect
{
    [Min(0)] public float healAmount = 5f;

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
public class PushEffect : SpellEffect
{
    [Min(0)] public float force = 5f;
    
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
public class PullEffect : SpellEffect
{
    [Min(0)] public float force = 5f;
    
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
public class LeechEffect : SpellEffect
{
    [Min(0)] public float damage = 5f;
    [Range(0f, 1f)] public float lifestealPercent = 0.5f;

    public override SpellEffect Clone()
    {
        return new LeechEffect { damage = damage, lifestealPercent = lifestealPercent };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target.TakeDamage(damage);
        float healAmount = damage * lifestealPercent;
        source.Heal(healAmount);
    }
}

[System.Serializable]
public class BurnManaEffect : SpellEffect
{
    [Min(0)] public float amount = 5f;
    
    public override SpellEffect Clone()
    {
        return new BurnManaEffect { amount = amount };
    }
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        MonoBehaviour targetMono = target as MonoBehaviour;
        if (targetMono)
        {
            if (targetMono.TryGetComponent(out ManaSourceComponent manaSource))
            {
                manaSource.TryConsume(amount);
            }
        }
    }
}