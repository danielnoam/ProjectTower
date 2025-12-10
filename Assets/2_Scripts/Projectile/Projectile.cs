using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private Rigidbody rigidBody;
    
    private bool _isInitialized;
    private ICombatTarget _source;
    private List<SpellEffect> _hitEffects;
    private ProjectileMovementBehavior _projectileMovementBehavior;
    private ProjectileCollisionBehavior _projectileCollisionBehavior;
    
    
    private void OnCollisionEnter(Collision other)
    {
        if (!_isInitialized) return;
        if (collisionLayers != (collisionLayers | (1 << other.gameObject.layer))) return;
        
         if (other.gameObject.TryGetComponent(out ICombatTarget hitTarget))
         {
             if (hitTarget != null && hitTarget != _source)
             {
                 foreach (SpellEffect spellEffect in _hitEffects)
                 {
                     spellEffect?.Apply(_source, hitTarget);
                 }
             }
         }
         
         _projectileCollisionBehavior?.Collision(this,other);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;
        if (collisionLayers != (collisionLayers | (1 << other.gameObject.layer))) return;
        
    }

    private void Update()
    {
        if (!_isInitialized) return;

        _projectileMovementBehavior.UpdateMovement();

    }

    public void Initialize(SpellEffect[] hitEffects, ProjectileMovementBehavior movementBehavior, ProjectileCollisionBehavior collisionBehavior, ICombatTarget source)
    {
        _projectileMovementBehavior = movementBehavior;
        _projectileCollisionBehavior = collisionBehavior;
        _hitEffects = new List<SpellEffect>(hitEffects);
        _source = source;
        _projectileMovementBehavior.Initialize(rigidBody, _source);
        _isInitialized = true;
    }

    public void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}