

using UnityEngine;

public enum Domain
{
    Arcane = 0, 
    Fire = 1,
    Water = 2,
    Air = 3,
    Earth = 4,
    Lightning = 5,
}

public static class DomainProperties
{
    public static StatusEffect GetStatusEffect(Domain domain)
    {
        switch (domain)
        {
            case Domain.Fire:
                return new BurningStatus 
                { 
                    duration = 3f,
                    damagePerSecond = 5f
                };
                
            case Domain.Lightning:
                return new StunnedStatus 
                { 
                    duration = 1f
                };
                
            case Domain.Water:
                return new SlowedStatus 
                { 
                    duration = 4f,
                    slowPercent = 0.4f
                };
                
            case Domain.Earth:
                return new WeakenedStatus 
                { 
                    duration = 5f,
                    damageReductionPercent = 0.3f
                };
                
            case Domain.Air:
            case Domain.Arcane:
            default:
                return null;
        }
    }
    
    public static float GetStatusChance(int totalDomains)
    {
        switch (totalDomains)
        {
            case 1: return 0.6f;
            case 2: return 0.3f;
            case 3: return 0.15f;
            case 4: return 0.1f;
            default: return 0.1f;
        }
    }
    
    public static void ApplyDomainKnockback(Domain domain, ICombatTarget source, ICombatTarget target)
    {
        if (domain == Domain.Air)
        {
            Vector3 direction = (target.Transform.position - source.Transform.position).normalized;
            float force = 5f;
            target.ApplyForce(direction, force);
        }
    }
}