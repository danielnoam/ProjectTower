using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool _isInitialized;
    private Vector3 _moveDirection;
    private float _moveSpeed;
    private ICombatTarget _source;
    private List<SpellEffect> _spellEffects;
    private void OnCollisionEnter(Collision other)
    {
        if (!_isInitialized) return;
        
        ICombatTarget target = other.gameObject.GetComponent<ICombatTarget>();
        if (target != null && target != _source)
        {
            foreach (SpellEffect spellEffect in _spellEffects)
            {
                spellEffect.Apply(_source, target);
            }
        }
        
        DestroyProjectile();
    }

    private void Update()
    {
        if (!_isInitialized) return;

        transform.position += _moveDirection * (_moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(_moveDirection);
    }

    public void Initialize(SpellEffect[] spellEffects, Vector3 moveDirection, float moveSpeed, ICombatTarget source)
    {
        _spellEffects = new List<SpellEffect>(spellEffects);
        _moveDirection = moveDirection;
        _moveSpeed = moveSpeed;
        _source = source;
        _isInitialized = true;
    }

    private void DestroyProjectile()
    {
        _isInitialized = false;
        Destroy(gameObject);
    }
}