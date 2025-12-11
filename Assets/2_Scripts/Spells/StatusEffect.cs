using UnityEngine;

[System.Serializable]
public abstract class StatusEffect
{
    public float duration;
    public virtual bool CanStack => true;
    
    public abstract void OnApply(ICombatTarget target);
    public abstract void OnTick(ICombatTarget target, float deltaTime);
    public abstract void OnRemove(ICombatTarget target);
    public abstract StatusEffect Clone();
    public abstract string GetDescription();
}

[System.Serializable]
public class BurningStatus : StatusEffect
{
    public float damagePerSecond = 5f;
    
    public override void OnApply(ICombatTarget target)
    {

    }
    
    public override void OnTick(ICombatTarget target, float deltaTime)
    {
        target.TakeDamage(damagePerSecond * deltaTime, null);
    }
    
    public override void OnRemove(ICombatTarget target)
    {

    }
    
    public override StatusEffect Clone()
    {
        return new BurningStatus 
        { 
            duration = duration,
            damagePerSecond = damagePerSecond
        };
    }
    
    public override string GetDescription()
    {
        return $"Burning for {damagePerSecond:F1} damage/sec for {duration:F1}s";
    }
}

[System.Serializable]
public class StunnedStatus : StatusEffect
{
    public override void OnApply(ICombatTarget target)
    {
        // TODO: Disable movement and actions
    }
    
    public override void OnTick(ICombatTarget target, float deltaTime)
    {
    }
    
    public override void OnRemove(ICombatTarget target)
    {

    }
    
    public override StatusEffect Clone()
    {
        return new StunnedStatus { duration = duration };
    }
    
    public override string GetDescription()
    {
        return $"Stun for {duration:F1}s";
    }
}

[System.Serializable]
public class SlowedStatus : StatusEffect
{
    [Range(0f, 1f)] public float slowPercent = 0.4f;
    
    public override void OnApply(ICombatTarget target)
    {

    }
    
    public override void OnTick(ICombatTarget target, float deltaTime)
    {
    }
    
    public override void OnRemove(ICombatTarget target)
    {
        Debug.Log($"{target} is no longer slowed");
    }
    
    public override StatusEffect Clone()
    {
        return new SlowedStatus 
        { 
            duration = duration,
            slowPercent = slowPercent
        };
    }
    
    public override string GetDescription()
    {
        return $"Slowed by {slowPercent:P0} for {duration:F1}s";
    }
}

[System.Serializable]
public class WeakenedStatus : StatusEffect
{
    [Range(0f, 1f)] public float damageReductionPercent = 0.3f;
    
    public override void OnApply(ICombatTarget target)
    {

    }
    
    public override void OnTick(ICombatTarget target, float deltaTime)
    {
    }
    
    public override void OnRemove(ICombatTarget target)
    {

    }
    
    public override StatusEffect Clone()
    {
        return new WeakenedStatus 
        { 
            duration = duration,
            damageReductionPercent = damageReductionPercent
        };
    }
    
    public override string GetDescription()
    {
        return $"Weakened by {damageReductionPercent:P0} for {duration:F1}s";
    }
}