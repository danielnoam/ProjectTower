using System;

[AttributeUsage(AttributeTargets.Class)]
public class SpellEffectAttribute : Attribute
{
    public string DisplayName { get; }
    public float ManaCost { get; set; }
    public Domain[] AvailableDomains { get; set; }
    
    public SpellEffectAttribute(string displayName, float manaCost = 10f)
    {
        DisplayName = displayName;
        ManaCost = manaCost;
        AvailableDomains = Array.Empty<Domain>();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ProjectileMovementAttribute : Attribute
{
    public string DisplayName { get; }
    
    public ProjectileMovementAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ProjectileCollisionAttribute : Attribute
{
    public string DisplayName { get; }
    
    public ProjectileCollisionAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}