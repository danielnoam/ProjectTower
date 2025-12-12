using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ConjureImpactBehavior
{
    
    public abstract ConjureImpactBehavior Clone();
    public abstract void Initialize(Rigidbody rigidBody, Collider[] colliders, ICombatTarget source);
    public abstract void OnCollision(Conjure conjure, Collision other);


}


[System.Serializable]
[ProjectileCollision("Destroy")]
public class DestroyBehavior  : ConjureImpactBehavior
{
    public override ConjureImpactBehavior Clone()
    {
        return new DestroyBehavior { };
    }

    public override void Initialize(Rigidbody rigidBody, Collider[] colliders, ICombatTarget source)
    {
        
    }
    
    public override void OnCollision(Conjure conjure, Collision other)
    {
        conjure.DestroyProjectile();
    }
}

[System.Serializable]
[ProjectileCollision("Pierce")]
public class PierceBehavior  : ConjureImpactBehavior
{
    public override ConjureImpactBehavior Clone()
    {
        return new PierceBehavior { };
    }

    public override void Initialize(Rigidbody rigidBody, Collider[] colliders, ICombatTarget source)
    {
        
        foreach (var collider in colliders)
        {
            if (!collider || collider.isTrigger) continue;
            collider.isTrigger = true;
        }
    }
    
    public override void OnCollision(Conjure conjure, Collision other)
    {

    }
}

[ProjectileCollision("Stick")]
public class StickBehavior : ConjureImpactBehavior
{
    private Transform _stuckToTransform;
    private Vector3 _stuckLocalPosition;
    private Quaternion _stuckLocalRotation;
    private bool _isStuck;
    private Rigidbody _projectileRb;

    public override ConjureImpactBehavior Clone()
    {
        return new StickBehavior();
    }

    public override void Initialize(Rigidbody rigidBody, Collider[] colliders, ICombatTarget source)
    {
        _isStuck = false;
        _projectileRb = rigidBody;
    }

    public override void OnCollision(Conjure conjure, Collision other)
    {
        if (_isStuck) return;
        

        if (other.gameObject.TryGetComponent(out ICombatTarget combatTarget))
        {
            _stuckToTransform = combatTarget.Transform;
            _stuckLocalPosition = _stuckToTransform.InverseTransformPoint(_projectileRb.position);
            _stuckLocalRotation = Quaternion.Inverse(_stuckToTransform.rotation) * _projectileRb.rotation;
        }
        else
        {
            _stuckToTransform = other.transform;
            _stuckLocalPosition = _stuckToTransform.InverseTransformPoint(_projectileRb.position);
            _stuckLocalRotation = Quaternion.Inverse(_stuckToTransform.rotation) * _projectileRb.rotation;
        }
        
        _projectileRb.linearVelocity = Vector3.zero;
        _projectileRb.angularVelocity = Vector3.zero;
        _projectileRb.isKinematic = true;

        _isStuck = true;
    }

    public void UpdateStickPosition()
    {
        if (!_isStuck || !_stuckToTransform) return;

        _projectileRb.position = _stuckToTransform.TransformPoint(_stuckLocalPosition);
        _projectileRb.rotation = _stuckToTransform.rotation * _stuckLocalRotation;
    }
}