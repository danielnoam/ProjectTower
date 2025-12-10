using System;
using System.Collections.Generic;
using UnityEngine;

public class Conjure : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private Rigidbody rigidBody;
    
    private bool _isInitialized;
    private float _currentLifeTime;
    private ICombatTarget _source;
    private List<SpellEffect> _hitEffects;
    private ConjureMovementBehavior _conjureMovementBehavior;
    private ConjureCollisionBehavior _conjureCollisionBehavior;
    
    
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
        
        _conjureMovementBehavior.UpdateMovement(Time.fixedDeltaTime);
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
         
         _conjureCollisionBehavior?.OnCollision(this,other);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;
        if (collisionLayers != (collisionLayers | (1 << other.gameObject.layer))) return;
        
    }



    public void Initialize(SpellEffect[] hitEffects, ConjureMovementBehavior movementBehavior, ConjureCollisionBehavior collisionBehavior, float lifetime, ICombatTarget source, ICombatTarget target = null)
    {
        _conjureMovementBehavior = movementBehavior;
        _conjureCollisionBehavior = collisionBehavior;
        _hitEffects = new List<SpellEffect>(hitEffects);
        _source = source;
        _conjureMovementBehavior.Initialize(rigidBody, _source, target);
        _conjureCollisionBehavior.Initialize(rigidBody,_source);
        _currentLifeTime = lifetime;
        _isInitialized = true;
    }

    public void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}