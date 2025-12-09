using System;
using DNExtensions;
using UnityEngine;


public enum TargetingType { Self, Other }
public enum DeliveryMethod { Instant, Projectile }
public enum CastType { Instant, Channeled, Charged }


[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/Spell", order = 1)]
public class SOSpell : ScriptableObject
{
    [Header("Spell Info")]
    public string label = "New Spell";
    [TextArea] public string description = "Spell Description";
    public Sprite icon;
    
    [Header("Casting")]
    public CastType castType = CastType.Instant;
    public float manaCost = 5f;
    [ShowIf("castType", CastType.Channeled)] public float channelRate = 0.3f;
    [ShowIf("castType", CastType.Charged)] public float chargeTime = 1f;
    
    [Header("Targeting")]
    public TargetingType targetingType = TargetingType.Other;
    public DeliveryMethod deliveryMethod = DeliveryMethod.Instant;
    [ShowIf("deliveryMethod", DeliveryMethod.Projectile)] public Projectile projectilePrefab;
    [ShowIf("deliveryMethod", DeliveryMethod.Projectile)] public float projectileSpeed = 20f;
    
    [Header("Effects")]
    [SerializeReference] public SpellEffect[] effects = Array.Empty<SpellEffect>();
    

    

    
    public void Cast(ICombatTarget source, ICombatTarget target)
    {
        ICombatTarget resolvedTarget = ResolveTarget(source, target);
        
        switch (deliveryMethod)
        {
            case DeliveryMethod.Instant:
                if (resolvedTarget != null)
                {
                    ApplyEffects(source, resolvedTarget);
                }
                break;
                
            case DeliveryMethod.Projectile:
                SpawnProjectile(source, resolvedTarget);
                break;
        }
    }
    
    private ICombatTarget ResolveTarget(ICombatTarget source, ICombatTarget target)
    {
        switch (targetingType)
        {
            case TargetingType.Self:
                return source; 
                
            case TargetingType.Other:
                return target;
                
            default:
                return null;
        }
    }
    
    private void ApplyEffects(ICombatTarget source, ICombatTarget target)
    {
        foreach (var effect in effects)
        {
            effect?.Apply(source, target);
        }
    }
    
    private void SpawnProjectile(ICombatTarget source, ICombatTarget target)
    {
        if (!projectilePrefab)
        {
            Debug.LogError($"Spell {label} is set to Projectile delivery but has no projectile prefab!");
            return;
        }
        
        Vector3 spawnPos = source.Transform.position + source.Transform.forward * 2f;
        Vector3 direction = target != null 
            ? (target.Transform.position - source.Transform.position).normalized 
            : source.Transform.forward;
        
        Projectile projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.Initialize(effects, direction, projectileSpeed, source);
    }
}