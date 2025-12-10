using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 8f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private Rigidbody rigidBody;
    
    private bool _isInitialized;
    private float _currentLifeTime;
    private ICombatTarget _source;
    private List<SpellEffect> _hitEffects;
    private ProjectileMovementBehavior _projectileMovementBehavior;
    private ProjectileCollisionBehavior _projectileCollisionBehavior;
    
    
    private void Update()
    {
        if (!_isInitialized) return;

        _currentLifeTime -= Time.deltaTime;
        if (_currentLifeTime <= 0)
        {
            DestroyProjectile();
        }

    }

    private void FixedUpdate()
    {
        if (!_isInitialized) return;
        
        _projectileMovementBehavior.UpdateMovement(Time.fixedDeltaTime);
    }
    
    
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
         
         _projectileCollisionBehavior?.OnCollision(this,other);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;
        if (collisionLayers != (collisionLayers | (1 << other.gameObject.layer))) return;
        
    }



    public void Initialize(SpellEffect[] hitEffects, ProjectileMovementBehavior movementBehavior, ProjectileCollisionBehavior collisionBehavior, ICombatTarget source, ICombatTarget target = null)
    {
        _projectileMovementBehavior = movementBehavior;
        _projectileCollisionBehavior = collisionBehavior;
        _hitEffects = new List<SpellEffect>(hitEffects);
        _source = source;
        _projectileMovementBehavior.Initialize(rigidBody, _source, target);
        _projectileCollisionBehavior.Initialize(rigidBody,_source);
        _currentLifeTime = lifeTime;
        _isInitialized = true;
    }

    public void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}