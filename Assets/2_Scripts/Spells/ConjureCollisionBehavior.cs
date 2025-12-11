using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ConjureCollisionBehavior
{
    protected Rigidbody projectileRb;
    protected ICombatTarget casterSource;
    
    public abstract ConjureCollisionBehavior Clone();
    public abstract void Initialize(Rigidbody rigidBody, ICombatTarget source);
    public abstract void OnCollision(Conjure conjure, Collision other);


}


[System.Serializable]
[ProjectileCollision("Destroy")]
public class DestroyBehavior  : ConjureCollisionBehavior
{
    public override ConjureCollisionBehavior Clone()
    {
        return new DestroyBehavior { };
    }

    public override void Initialize(Rigidbody rigidBody, ICombatTarget source)
    {
        
        projectileRb = rigidBody;
        casterSource = source;
    }
    
    public override void OnCollision(Conjure conjure, Collision other)
    {
        conjure.DestroyProjectile();
    }
}

[System.Serializable]
[ProjectileCollision("Pierce")]
public class PierceBehavior  : ConjureCollisionBehavior
{
    public override ConjureCollisionBehavior Clone()
    {
        return new PierceBehavior { };
    }

    public override void Initialize(Rigidbody rigidBody, ICombatTarget source)
    {
        
        projectileRb = rigidBody;
        casterSource = source;

        if (rigidBody.TryGetComponent(out Collider collider))
        {
            collider.isTrigger = true;
        }
    }
    
    public override void OnCollision(Conjure conjure, Collision other)
    {

    }
}

[ProjectileCollision("Stick")]
public class StickBehavior : ConjureCollisionBehavior
{
    private Transform _stuckToTransform;
    private Vector3 _stuckLocalPosition;
    private Quaternion _stuckLocalRotation;
    private bool _isStuck;

    public override ConjureCollisionBehavior Clone()
    {
        return new StickBehavior();
    }

    public override void Initialize(Rigidbody rigidBody, ICombatTarget source)
    {
        projectileRb = rigidBody;
        casterSource = source;
        _isStuck = false;
    }

    public override void OnCollision(Conjure conjure, Collision other)
    {
        if (_isStuck) return;


        projectileRb.linearVelocity = Vector3.zero;
        projectileRb.angularVelocity = Vector3.zero;
        projectileRb.isKinematic = true;

        if (other.gameObject.TryGetComponent(out ICombatTarget combatTarget))
        {
            _stuckToTransform = combatTarget.Transform;
            _stuckLocalPosition = _stuckToTransform.InverseTransformPoint(projectileRb.position);
            _stuckLocalRotation = Quaternion.Inverse(_stuckToTransform.rotation) * projectileRb.rotation;
        }
        else
        {
            _stuckToTransform = other.transform;
            _stuckLocalPosition = _stuckToTransform.InverseTransformPoint(projectileRb.position);
            _stuckLocalRotation = Quaternion.Inverse(_stuckToTransform.rotation) * projectileRb.rotation;
        }

        _isStuck = true;
    }

    public void UpdateStickPosition()
    {
        if (!_isStuck || !_stuckToTransform) return;

        projectileRb.position = _stuckToTransform.TransformPoint(_stuckLocalPosition);
        projectileRb.rotation = _stuckToTransform.rotation * _stuckLocalRotation;
    }
}