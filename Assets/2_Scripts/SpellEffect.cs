using UnityEngine;

public abstract class SpellEffect
{
    public abstract void Apply(ISpellTarget caster, ISpellTarget target);
}


[System.Serializable]
public class DealDamageEffect : SpellEffect
{
    public float damage = 5f;
    
    public override void Apply(ISpellTarget caster, ISpellTarget target)
    {
        target.TakeDamage(damage);
    }
}

[System.Serializable]
public class HealEffect : SpellEffect
{
    public float healAmount = 5f;

    public override void Apply(ISpellTarget caster, ISpellTarget target)
    {
        
        target.Heal(healAmount);
    }
}

[System.Serializable]
public class PushEffect : SpellEffect
{
    public int force;
    public Vector3 direction;

    public override void Apply(ISpellTarget caster, ISpellTarget target)
    {
        target.ApplyForce(direction * force);
    }
}