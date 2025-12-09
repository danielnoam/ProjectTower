using UnityEngine;

[System.Serializable]
public abstract class SpellEffect
{
    public abstract void Apply(ICombatTarget source, ICombatTarget target);
}

[System.Serializable]
public class DealDamageEffect : SpellEffect
{
    [Min(0)] public float damage = 5f;
    
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target.TakeDamage(damage);
    }
}

[System.Serializable]
public class HealEffect : SpellEffect
{
    [Min(0)] public float healAmount = 5f;

    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target.Heal(healAmount);
    }
}

[System.Serializable]
public class ApplyForceEffect : SpellEffect
{
    [Min(0)] public float force = 5f;
    public Vector3 direction = Vector3.forward;

    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        Vector3 forceVector = direction.normalized * force;
        target.ApplyForce(forceVector);
    }
}

[System.Serializable]
public class ApplyForceInLookDirectionEffect : SpellEffect
{
    [Min(0)] public float force = 5f;
    public LookDirection lookDirectionOf = LookDirection.Source;
    
    public enum LookDirection { Source, Target }
    
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        Vector3 direction = (lookDirectionOf == LookDirection.Source) ? source.Transform.forward : target.Transform.forward;
        Vector3 forceVector = direction.normalized * force;
        target.ApplyForce(forceVector);
    }
}

[System.Serializable]
public class LifeStealEffect : SpellEffect
{
    [Min(0)] public float damage = 5f;
    [Range(0f, 1f)] public float lifestealPercent = 0.5f;

    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        target.TakeDamage(damage);
        int healAmount = Mathf.RoundToInt(damage * lifestealPercent);
        source.Heal(healAmount);
    }
}

[System.Serializable]
public class ManaBurnEffect : SpellEffect
{
    [Min(0)] public float damage = 5f;
    
    public override void Apply(ICombatTarget source, ICombatTarget target)
    {
        MonoBehaviour targetMono = target as MonoBehaviour;
        if (targetMono)
        {
            if (targetMono.TryGetComponent(out SpellCaster spellCaster))
            {
                spellCaster.ConsumeMana(damage);
            }
        }
    }
}