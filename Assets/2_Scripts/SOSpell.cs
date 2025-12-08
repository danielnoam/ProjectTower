using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/Spell", order = 1)]
public class SOSpell : ScriptableObject
{
    public string label = "New Spell";
    [TextArea] public string description = "Spell Description";
    public Sprite icon;
    public float manaCost = 5f;
    public CastType castType = CastType.Instant;
    public float castTime = 1f;
    public TargetingType targetingType = TargetingType.Other;
    public DeliveryMethod deliveryMethod = DeliveryMethod.Hitscan;

    public SpellEffect[] effects = Array.Empty<SpellEffect>();
    
    public Projectile projectilePrefab;
    
    
    public enum TargetingType { Self, Other }
    public enum DeliveryMethod { Hitscan, Projectile }
    public enum CastType { Instant, Channeled, Charged }

    
    public void Cast(ISpellTarget caster, ISpellTarget target)
    {
        ISpellTarget resolvedTarget = ResolveTarget(caster, target);
        if (resolvedTarget == null) return;
        
        switch (deliveryMethod)
        {
            case DeliveryMethod.Hitscan:
                ApplyEffects(caster, resolvedTarget);
                break;
            
            case DeliveryMethod.Projectile:
                SpawnProjectile(caster, resolvedTarget);
                break;
        }
    }

    private ISpellTarget ResolveTarget(ISpellTarget caster, ISpellTarget target)
    {
        switch (targetingType)
        {
            case TargetingType.Self:
                return caster;
            
            case TargetingType.Other:
                return target; 
            default:
                return null;
        }
    }

    private void ApplyEffects(ISpellTarget caster, ISpellTarget target)
    {
        foreach (var effect in effects)
        {
            effect?.Apply(caster, target);
        }
    }

    private void SpawnProjectile(ISpellTarget caster, ISpellTarget target)
    {
        Vector3 spawnPos = caster.Transform.position + caster.Transform.forward * 2;
        Vector3 direction = target != null ? (target.Transform.position - caster.Transform.position).normalized : caster.Transform.forward;
    
        Projectile projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.Initialize(direction, 5f);
    }


    
}