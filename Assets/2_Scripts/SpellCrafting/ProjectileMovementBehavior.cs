using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ProjectileMovementBehavior
{
    protected Rigidbody projectileRb;
    protected ICombatTarget casterSource;

    public abstract ProjectileMovementBehavior Clone();
    public abstract void Initialize(Rigidbody projectileTransform, ICombatTarget source, ICombatTarget target);
    public abstract void UpdateMovement(float delta);
}

[System.Serializable]
[ProjectileMovement("Stationary")]
public class StationaryMovement : ProjectileMovementBehavior
{
    public override ProjectileMovementBehavior Clone()
    {
        return new StationaryMovement();
    }

    public override void Initialize(Rigidbody rigidbody, ICombatTarget source, ICombatTarget target)
    {
        projectileRb = rigidbody;
        casterSource = source;
    }

    public override void UpdateMovement(float delta)
    {

    }
}


[System.Serializable]
[ProjectileMovement("Straight")]
public class StraightMovement  : ProjectileMovementBehavior
{
    [Min(0)] public float moveSpeed = 5f;
    
    private Vector3 _moveDirection;
    
    public override ProjectileMovementBehavior Clone()
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
public class HomingMovement : ProjectileMovementBehavior
{
    [Min(0)] public float moveSpeed = 5f;
    [Min(0)] public float turnSpeed = 5f;

    private ICombatTarget _target;
    private Vector3 _moveDirection;

    public override ProjectileMovementBehavior Clone()
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
public class BoomerangMovement : ProjectileMovementBehavior
{
        
    [Min(0)] public float moveSpeed = 5f;
    [Min(0)] public float turnSpeed = 5f;
    [Min(0)] public float returnAfterInSeconds = 5f;

    private Vector3 _moveDirection;
    private float _timeSinceSpawn;

    public override ProjectileMovementBehavior Clone()
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