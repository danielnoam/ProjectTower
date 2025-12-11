using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ConjureMovementBehavior
{
    protected Rigidbody projectileRb;
    protected ICombatTarget casterSource;
    
    public virtual float Lifetime => 5f;

    public abstract ConjureMovementBehavior Clone();
    public abstract void Initialize(Rigidbody projectileTransform, ICombatTarget source, ICombatTarget target);
    public abstract void UpdateMovement(float delta);
}

[System.Serializable]
[ProjectileMovement("Stationary")]
public class StationaryMovement : ConjureMovementBehavior
{
    public override float Lifetime => 15f;

    public override ConjureMovementBehavior Clone()
    {
        return new StationaryMovement();
    }

    public override void Initialize(Rigidbody rigidbody, ICombatTarget source, ICombatTarget target)
    {
        projectileRb = rigidbody;
        casterSource = source;
        projectileRb.isKinematic = true;
    }

    public override void UpdateMovement(float delta)
    {

    }
    
}


[System.Serializable]
[ProjectileMovement("Straight")]
public class StraightMovement  : ConjureMovementBehavior
{
    [Min(0)] public float moveSpeed = 5f;
    public override float Lifetime => 7f;
    
    
    private Vector3 _moveDirection;
    
    public override ConjureMovementBehavior Clone()
    {
        return new StraightMovement 
        { 
            moveSpeed = moveSpeed 
        };
    }
    
    public override void Initialize(Rigidbody rigidbody, ICombatTarget source, ICombatTarget target)
    {
        projectileRb = rigidbody;
        casterSource = source;
        _moveDirection = casterSource.LookDirection;
    }
    
    public override void UpdateMovement(float delta)
    {
        projectileRb.position += _moveDirection * (moveSpeed * delta);
        projectileRb.rotation = Quaternion.LookRotation(_moveDirection);
    }
    
}

[System.Serializable]
[ProjectileMovement("Homing")]
public class HomingMovement : ConjureMovementBehavior
{
    [Min(0)] public float moveSpeed = 5f;
    [Min(0)] public float turnSpeed = 5f;
    public override float Lifetime => 5f;

    private ICombatTarget _target;
    private Vector3 _moveDirection;

    public override ConjureMovementBehavior Clone()
    {
        return new HomingMovement
        {
            moveSpeed = moveSpeed,
            turnSpeed = turnSpeed
        };
    }

    public override void Initialize(Rigidbody rigidbody, ICombatTarget source, ICombatTarget target)
    {
        projectileRb = rigidbody;
        casterSource = source;
        _target = target;
        Debug.Log(_target);
        _moveDirection = casterSource.LookDirection;
    }

    public override void UpdateMovement(float delta)
    {
        if (_target != null)
        {
            _moveDirection = Vector3.RotateTowards(_moveDirection, _target.Transform.position - projectileRb.position, turnSpeed * delta, 0);
        } 
        projectileRb.position += _moveDirection * (moveSpeed * delta);
        projectileRb.rotation = Quaternion.LookRotation(_moveDirection);
    }
    
}


[System.Serializable]
[ProjectileMovement("Boomerang")]
public class BoomerangMovement : ConjureMovementBehavior
{
        
    [Min(0)] public float moveSpeed = 5f;
    [Min(0)] public float turnSpeed = 5f;
    [Min(0)] public float returnAfterInSeconds = 5f;
    public override float Lifetime => returnAfterInSeconds * 1.5f;

    private Vector3 _moveDirection;
    private float _timeSinceSpawn;

    public override ConjureMovementBehavior Clone()
    {
        return new BoomerangMovement
        {
            moveSpeed = moveSpeed,
            turnSpeed = turnSpeed,
            returnAfterInSeconds = returnAfterInSeconds
        };
    }

    public override void Initialize(Rigidbody rigidbody, ICombatTarget source, ICombatTarget target)
    {
        projectileRb = rigidbody;
        casterSource = source;
        _timeSinceSpawn = 0;
        _moveDirection = casterSource.LookDirection;
    }

    public override void UpdateMovement(float delta)
    {
        _timeSinceSpawn += delta;
        if (casterSource != null)
        {
            if (_timeSinceSpawn >= returnAfterInSeconds)
            {
                _moveDirection = Vector3.RotateTowards(_moveDirection, casterSource.Transform.position - projectileRb.position, turnSpeed * delta, 0);
            }
            else 
            {
                _moveDirection = Vector3.RotateTowards(_moveDirection, casterSource.LookDirection, turnSpeed * delta, 0);
            }

        }

        
        projectileRb.position += _moveDirection * (moveSpeed * delta);
        projectileRb.rotation = Quaternion.LookRotation(_moveDirection);
    }
    
}