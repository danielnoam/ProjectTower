using System;
using DNExtensions;
using UnityEngine;


[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Spell", order = 1)]
public class SOSpell : ScriptableObject
{
    [Header("Spell Info")]
    public string label = "New Spell";
    public Sprite icon;
    public string description = "Spell Description";
    
    [Header("Casting")]
    public CastMethod castMethod = CastMethod.Instant;
    public float manaCost = 5f;
    [ShowIf("castMethod", CastMethod.Channel)] public float channelRate = 0.15f;
    [ShowIf("castMethod", CastMethod.Charge)] public float chargeTime = 1.5f;
    public SpellForm spellForm = SpellForm.Invoke;
    public Domain domain = Domain.Arcane;
    [SerializeReference] public SpellEffect[] effects = Array.Empty<SpellEffect>();
    
    [Header("Conjure")]
    public Projectile projectilePrefab;
    [SerializeReference] public ProjectileMovementBehavior projectileMovement = new StraightMovement();
    [SerializeReference] public ProjectileCollisionBehavior projectileCollision = new DestroyBehavior();

    
    public void Cast(ICombatTarget source, ICombatTarget target, Transform castPoint)
    {
        
        switch (spellForm)
        {
            case SpellForm.Imbue:
                ApplyEffects(source,source);
                break;
            
            case SpellForm.Invoke:
                if (target != null) ApplyEffects(source, target);
                break;
                
            case SpellForm.Conjure:
                SpawnProjectile(source, target, castPoint);
                break;
        }
    }
    
    
    private void ApplyEffects(ICombatTarget source, ICombatTarget target)
    {
        if (effects == null || effects.Length == 0) return;
        
        foreach (var effect in effects)
        {
            effect?.Apply(source, target);
        }
    }
    
    private void SpawnProjectile(ICombatTarget source, ICombatTarget target, Transform castPoint)
    {
        if (!projectilePrefab)
        {
            Debug.LogError($"Spell {label} is set to Projectile but has no prefab!");
            return;
        }
        
        SpellEffect[] hitEffectsClone = CloneEffects(effects);
        ProjectileMovementBehavior movementClone = projectileMovement?.Clone();
        ProjectileCollisionBehavior collisionClone = projectileCollision?.Clone();
        
        Projectile projectile = Instantiate(projectilePrefab, castPoint.position, Quaternion.identity);
        projectile.Initialize(hitEffectsClone, movementClone, collisionClone, source, target);
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