using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool _isInitialized;
    private ICombatTarget _source;
    private List<SpellEffect> _spawnEffects;
    private List<SpellEffect> _hitEffects;
    private ProjectileMovementBehavior _projectileMovementBehavior;
    
    
    private void OnCollisionEnter(Collision other)
    {
        if (!_isInitialized) return;
        
        ICombatTarget target = other.gameObject.GetComponent<ICombatTarget>();
        if (target != null && target != _source)
        {
            foreach (SpellEffect spellEffect in _hitEffects)
            {
                spellEffect?.Apply(_source, target);
            }
        }
        
        DestroyProjectile();
    }

    private void Update()
    {
        if (!_isInitialized) return;

        _projectileMovementBehavior.UpdateMovement();

    }

    public void Initialize(SpellEffect[] spawnEffects,SpellEffect[] hitEffects, ProjectileMovementBehavior projectileMovementBehavior, ICombatTarget source)
    {
        _projectileMovementBehavior = projectileMovementBehavior;
        _spawnEffects = new List<SpellEffect>(spawnEffects);
        _hitEffects = new List<SpellEffect>(hitEffects);
        _source = source;
        _projectileMovementBehavior.Initialize(transform, _source);
        _isInitialized = true;
        
        foreach (SpellEffect spellEffect in _spawnEffects)
        {
            spellEffect?.Apply(_source, _source);
        }
    }

    private void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}