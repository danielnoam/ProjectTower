using System;
using DNExtensions;
using UnityEngine;


[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/Spell", order = 1)]
public class SOSpell : ScriptableObject
{
    [Header("Spell Info")]
    public string label = "New Spell";
    public Sprite icon;
    public string description = "Spell Description";
    
    [Header("Casting")]
    public CastType castType = CastType.Instant;
    public float manaCost = 5f;
    [ShowIf("castType", CastType.Channeled)] public float channelRate = 0.15f;
    [ShowIf("castType", CastType.Charged)] public float chargeTime = 1.5f;
    
    [Header("Targeting")]
    public TargetingType targetingType = TargetingType.Other;
    public DeliveryMethod deliveryMethod = DeliveryMethod.Instant;
    [SerializeReference] public SpellEffect[] effects = Array.Empty<SpellEffect>();
    
    [Header("Projectile")]
    public Projectile projectilePrefab;
    [SerializeReference] public ProjectileMovementBehavior projectileMovement = new NormalMovement();
    [SerializeReference] public SpellEffect[] spawnEffects = Array.Empty<SpellEffect>();
    [SerializeReference] public SpellEffect[] hitEffects = Array.Empty<SpellEffect>();

    
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
        
        SpellEffect[] spawnEffectsClone = CloneEffects(spawnEffects);
        SpellEffect[] hitEffectsClone = CloneEffects(hitEffects);
        ProjectileMovementBehavior movementClone = projectileMovement?.Clone();
        Vector3 spawnPos = source.Transform.position + source.Transform.forward * 2f;
        
        Projectile projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.Initialize(spawnEffectsClone, hitEffectsClone, movementClone, source);
    }
    
    private SpellEffect[] CloneEffects(SpellEffect[] effectsToClone)
    {
        if (effectsToClone == null || effectsToClone.Length == 0) return Array.Empty<SpellEffect>();
    
        SpellEffect[] clones = new SpellEffect[effectsToClone.Length];
        for (int i = 0; i < effectsToClone.Length; i++)
        {
            clones[i] = effectsToClone[i]?.Clone();
        }
        return clones;
    }
}