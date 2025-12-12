using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Conjure : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private Rigidbody rigidBody;

    
    private Collider[] _colliders = Array.Empty<Collider>();
    private bool _isInitialized;
    private float _currentLifeTime;
    private SOSpell _spell;
    private ICombatTarget _source;
    private List<SpellEffect> _hitEffects;
    private ConjureMotionBehavior _conjureMotionBehavior;
    private ConjureImpactBehavior _conjureImpactBehavior;
    private List<Domain> _domains;
    private Augment _augment;
    private bool _isStuck;

    private void Awake()
    {
        if (!rigidBody)
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        if (_colliders.Length == 0)
        {
            _colliders = GetComponentsInChildren<Collider>();
        }
    }

    private void Update()
    {
        if (!_isInitialized) return;

        _currentLifeTime -= Time.deltaTime;
        if (_currentLifeTime <= 0)
        {
            DestroyProjectile();
        }
        
        if (_conjureImpactBehavior is StickBehavior stickBehavior)
        {
            stickBehavior.UpdateStickPosition();
        }

    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _isStuck) return;
        
        _conjureMotionBehavior.UpdateMovement(Time.fixedDeltaTime);
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
                _conjureImpactBehavior?.OnCollision(this,other);
            }
        }
        else
        {
            _conjureImpactBehavior?.OnCollision(this,other);
        }
     
        
        if (_conjureImpactBehavior is StickBehavior)
        {
            _isStuck = true;
        }
    }
    
    

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;
        if (collisionLayers != (collisionLayers | (1 << other.gameObject.layer))) return;
        
        if (other.gameObject.TryGetComponent(out ICombatTarget hitTarget))
        {
            if (hitTarget != null && hitTarget != _source)
            {
                Vector3 impactPoint = other.ClosestPoint(transform.position);
                _augment.Apply(_hitEffects, _domains, _source, hitTarget, impactPoint);
            }
        }
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
        _conjureMotionBehavior = spell.conjureMotion?.Clone();
        _conjureImpactBehavior = spell.conjureImpact?.Clone();
        _currentLifeTime = spell.conjureLifeTime;
        
        _conjureMotionBehavior?.Initialize(rigidBody, _source, target);
        _conjureImpactBehavior?.Initialize(rigidBody,_colliders, _source);
        _isInitialized = true;
    }

    public void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}