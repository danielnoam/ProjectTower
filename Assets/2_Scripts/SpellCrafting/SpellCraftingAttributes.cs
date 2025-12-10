using System;

[AttributeUsage(AttributeTargets.Class)]
public class SpellEffectAttribute : Attribute
{
    public string DisplayName { get; }
    
    public SpellEffectAttribute(string displayName)
    {
        DisplayName = displayName;
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