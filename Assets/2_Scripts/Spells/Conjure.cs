using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conjure : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private Rigidbody rigidBody;
    
    private bool _isInitialized;
    private float _currentLifeTime;
    private SOSpell _spell;
    private ICombatTarget _source;
    private List<SpellEffect> _hitEffects;
    private ConjureMovementBehavior _conjureMovementBehavior;
    private ConjureCollisionBehavior _conjureCollisionBehavior;
    private List<Domain> _domains;
    private Augment _augment;
    private bool _isStuck;
    
    
    private void Update()
    {
        if (!_isInitialized) return;

        _currentLifeTime -= Time.deltaTime;
        if (_currentLifeTime <= 0)
        {
            DestroyProjectile();
        }
        
        if (_conjureCollisionBehavior is StickBehavior stickBehavior)
        {
            stickBehavior.UpdateStickPosition();
        }

    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _isStuck) return;
        
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
                Vector3 impactPoint = other.contacts[0].point;
                _augment.Apply(_hitEffects, _domains, _source, hitTarget, impactPoint);
            }
        }
     
        _conjureCollisionBehavior?.OnCollision(this,other);
        
        if (_conjureCollisionBehavior is StickBehavior)
        {
            _isStuck = true;
        }
    }
    
    

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;
        if (collisionLayers != (collisionLayers | (1 << other.gameObject.layer))) return;
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

    public void Initialize(SOSpell spell, ICombatTarget source, ICombatTarget target = null)
    {
        _spell = spell;
        _source = source;
        
        _hitEffects = CloneEffects(spell.effects).ToList();
        _domains = new List<Domain>(spell.domains);
        _augment = spell.augment?.Clone() ?? new NoneAugment();
        _conjureMovementBehavior = spell.conjureMovement?.Clone();
        _conjureCollisionBehavior = spell.conjureCollision?.Clone();
        _currentLifeTime = spell.conjureLifeTime;
        
        _conjureMovementBehavior?.Initialize(rigidBody, _source, target);
        _conjureCollisionBehavior?.Initialize(rigidBody, _source);
        _isInitialized = true;
    }

    public void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}