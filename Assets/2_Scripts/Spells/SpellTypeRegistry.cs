using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class SpellTypeRegistry
{
    private static List<Type> effectTypes;
    private static List<Type> motionTypes;
    private static List<Type> impactTypes;
    private static List<Type> augmentTypes;
    
    public static List<Type> EffectTypes
    {
        get
        {
            if (effectTypes == null) CacheTypes();
            return effectTypes;
        }
    }
    
    public static List<Type> MotionTypes
    {
        get
        {
            if (motionTypes == null) CacheTypes();
            return motionTypes;
        }
    }
    
    public static List<Type> ImpactTypes
    {
        get
        {
            if (impactTypes == null) CacheTypes();
            return impactTypes;
        }
    }
    
    public static List<Type> AugmentTypes
    {
        get
        {
            if (augmentTypes == null) CacheTypes();
            return augmentTypes;
        }
    }
    
    private static void CacheTypes()
    {
        effectTypes = new List<Type>();
        motionTypes = new List<Type>();
        impactTypes = new List<Type>();
        augmentTypes = new List<Type>();
        
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
                

                if (type.GetCustomAttribute<SpellEffectAttribute>() != null && typeof(SpellEffect).IsAssignableFrom(type))
                {
                    effectTypes.Add(type);
                }

                if (type.GetCustomAttribute<AugmentAttribute>() != null && typeof(Augment).IsAssignableFrom(type))
                {
                    augmentTypes.Add(type);
                }

                
                if (type.GetCustomAttribute<ProjectileMovementAttribute>() != null && typeof(ConjureMotionBehavior).IsAssignableFrom(type))
                {
                    motionTypes.Add(type);
                }
                
                if (type.GetCustomAttribute<ProjectileCollisionAttribute>() != null && 
                    typeof(ConjureImpactBehavior).IsAssignableFrom(type))
                {
                    impactTypes.Add(type);
                }
            }
        }
        

        effectTypes = effectTypes.OrderBy(t => t.GetCustomAttribute<SpellEffectAttribute>().DisplayName).ToList();
        augmentTypes = augmentTypes
            .OrderBy(t => t.Name == "NoneAugment" ? 0 : 1)
            .ThenBy(t => t.GetCustomAttribute<AugmentAttribute>()?.DisplayName ?? "")
            .ToList();
        motionTypes = motionTypes.OrderBy(t => t.GetCustomAttribute<ProjectileMovementAttribute>().DisplayName).ToList();
        impactTypes = impactTypes.OrderBy(t => t.GetCustomAttribute<ProjectileCollisionAttribute>().DisplayName).ToList();
    }
    
    public static string GetEffectDisplayName(Type type)
    {
        return type.GetCustomAttribute<SpellEffectAttribute>()?.DisplayName ?? type.Name;
    }
    
    public static float GetEffectManaCost(Type type)
    {
        return type.GetCustomAttribute<SpellEffectAttribute>()?.ManaCost ?? 10f;
    }

    public static Domain[] GetEffectDomains(Type type)
    {
        var attr = type.GetCustomAttribute<SpellEffectAttribute>();
        return attr?.AvailableDomains ?? System.Array.Empty<Domain>();
    }

    public static bool IsEffectValidForDomain(Type effectType, Domain domain)
    {
        var domains = GetEffectDomains(effectType);
        return domains.Length == 0 || domains.Contains(domain);
    }
    
    public static string GetMotionDisplayName(Type type)
    {
        return type.GetCustomAttribute<ProjectileMovementAttribute>()?.DisplayName ?? type.Name;
    }
    
    public static string GetImpactDisplayName(Type type)
    {
        return type.GetCustomAttribute<ProjectileCollisionAttribute>()?.DisplayName ?? type.Name;
    }
    
    public static SpellEffect CreateEffect(Type type)
    {
        return Activator.CreateInstance(type) as SpellEffect;
    }
    
    public static ConjureMotionBehavior CreateMotion(Type type)
    {
        return Activator.CreateInstance(type) as ConjureMotionBehavior;
    }
    
    public static ConjureImpactBehavior CreateImpact(Type type)
    {
        return Activator.CreateInstance(type) as ConjureImpactBehavior;
    }
    
    public static string GetAugmentDisplayName(Type type)
    {
        return type.GetCustomAttribute<AugmentAttribute>()?.DisplayName ?? type.Name;
    }

    public static float GetAugmentManaCost(Type type)
    {
        return type.GetCustomAttribute<AugmentAttribute>()?.ManaCost ?? 0f;
    }

    public static Augment CreateAugment(Type type)
    {
        return Activator.CreateInstance(type) as Augment;
    }
    
}