using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class SpellTypeRegistry
{
    private static List<Type> effectTypes;
    private static List<Type> movementTypes;
    private static List<Type> collisionTypes;
    
    public static List<Type> EffectTypes
    {
        get
        {
            if (effectTypes == null) CacheTypes();
            return effectTypes;
        }
    }
    
    public static List<Type> MovementTypes
    {
        get
        {
            if (movementTypes == null) CacheTypes();
            return movementTypes;
        }
    }
    
    public static List<Type> CollisionTypes
    {
        get
        {
            if (collisionTypes == null) CacheTypes();
            return collisionTypes;
        }
    }
    
    private static void CacheTypes()
    {
        effectTypes = new List<Type>();
        movementTypes = new List<Type>();
        collisionTypes = new List<Type>();
        
        // Get all types in all assemblies
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (Assembly assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                continue; 
            }
            
            foreach (Type type in types)
            {
                if (type.IsAbstract || type.IsInterface) continue;
                
                // Check for effect attribute
                if (type.GetCustomAttribute<SpellEffectAttribute>() != null && 
                    typeof(SpellEffect).IsAssignableFrom(type))
                {
                    effectTypes.Add(type);
                }
                
                // Check for movement attribute
                if (type.GetCustomAttribute<ProjectileMovementAttribute>() != null && 
                    typeof(ProjectileMovementBehavior).IsAssignableFrom(type))
                {
                    movementTypes.Add(type);
                }
                
                // Check for collision attribute
                if (type.GetCustomAttribute<ProjectileCollisionAttribute>() != null && 
                    typeof(ProjectileCollisionBehavior).IsAssignableFrom(type))
                {
                    collisionTypes.Add(type);
                }
            }
        }
        
        // Sort alphabetically by display name
        effectTypes = effectTypes.OrderBy(t => t.GetCustomAttribute<SpellEffectAttribute>().DisplayName).ToList();
        movementTypes = movementTypes.OrderBy(t => t.GetCustomAttribute<ProjectileMovementAttribute>().DisplayName).ToList();
        collisionTypes = collisionTypes.OrderBy(t => t.GetCustomAttribute<ProjectileCollisionAttribute>().DisplayName).ToList();
    }
    
    public static string GetEffectDisplayName(Type type)
    {
        return type.GetCustomAttribute<SpellEffectAttribute>()?.DisplayName ?? type.Name;
    }
    
    public static string GetMovementDisplayName(Type type)
    {
        return type.GetCustomAttribute<ProjectileMovementAttribute>()?.DisplayName ?? type.Name;
    }
    
    public static string GetCollisionDisplayName(Type type)
    {
        return type.GetCustomAttribute<ProjectileCollisionAttribute>()?.DisplayName ?? type.Name;
    }
    
    public static SpellEffect CreateEffect(Type type)
    {
        return Activator.CreateInstance(type) as SpellEffect;
    }
    
    public static ProjectileMovementBehavior CreateMovement(Type type)
    {
        return Activator.CreateInstance(type) as ProjectileMovementBehavior;
    }
    
    public static ProjectileCollisionBehavior CreateCollision(Type type)
    {
        return Activator.CreateInstance(type) as ProjectileCollisionBehavior;
    }
}