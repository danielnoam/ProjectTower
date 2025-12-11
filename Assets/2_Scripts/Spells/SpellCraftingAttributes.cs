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
public class AugmentAttribute : Attribute
{
    public string DisplayName { get; }
    public float ManaCost { get; }
    public SpellForm[] CompatibleForms { get; }
    
    public AugmentAttribute(string displayName, float manaCost, SpellForm[] compatibleForms = null)
    {
        DisplayName = displayName;
        ManaCost = manaCost;
        CompatibleForms = compatibleForms;
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