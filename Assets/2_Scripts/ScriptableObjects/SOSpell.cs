using System;
using System.Collections.Generic;
using DNExtensions;
using UnityEngine;


[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Spell", order = 1)]
public class SOSpell : ScriptableObject
{
    [Header("Info")]
    public string label = "New Spell";
    public Sprite icon;
    public string description = "Spell Description";
    
    [Header("Spell")]
    public CastMethod castMethod = CastMethod.Instant;
    public float manaCost = 5f;
    public float channelRate = 0.15f;
    public float chargeTime = 1.5f;
    public SpellForm spellForm = SpellForm.Invoke;
    [SerializeReference] public Augment augment = new NoneAugment();
    public Conjure conjurePrefab;
    public float conjureLifeTime = 5f;
    [SerializeReference] public ConjureMovementBehavior conjureMovement = new StraightMovement();
    [SerializeReference] public ConjureCollisionBehavior conjureCollision = new DestroyBehavior();
    public List<Domain> domains = new List<Domain>();
    [SerializeReference] public SpellEffect[] effects = Array.Empty<SpellEffect>();

    private void OnValidate()
    {
        if (domains.Count == 0)
        {
            domains.Add(Domain.Arcane);
        }
    }

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
    
        Vector3 impactPoint = target.Transform.position;
        List<SpellEffect> effectsList = new List<SpellEffect>(effects);
    
        augment ??= new NoneAugment();
    
        augment.Apply(effectsList, domains, source, target, impactPoint);
    }
    
    private void SpawnProjectile(ICombatTarget source, ICombatTarget target, Transform castPoint)
    {
        if (!conjurePrefab)
        {
            Debug.LogError($"Spell {label} is set to Projectile but has no prefab!");
            return;
        }
        
        
        
        Conjure conjure = Instantiate(conjurePrefab, castPoint.position, Quaternion.identity);
        conjure.Initialize(this, source, target);
    }
    

}