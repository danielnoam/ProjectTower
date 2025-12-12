using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ConjureMotionBehavior
{
    protected Rigidbody projectileRb;
    protected ICombatTarget casterSource;
    
    public virtual float Duration => 5f;

    public abstract ConjureMotionBehavior Clone();
    public abstract void Initialize(Rigidbody projectileTransform, ICombatTarget source, ICombatTarget target);
    public abstract void UpdateMovement(float delta);
}

[System.Serializable]
[ProjectileMovement("Stationary")]
public class StationaryMotion : ConjureMotionBehavior
{
    public override float Duration => 15f;

    public override ConjureMotionBehavior Clone()
    {
        return new StationaryMotion();
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
public class StraightMotion  : ConjureMotionBehavior
{
    [Min(0)] public float moveSpeed = 10f;
    public override float Duration => 7f;
    
    
    private Vector3 _moveDirection;
    
    public override ConjureMotionBehavior Clone()
    {
        return new StraightMotion 
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
public class HomingMotion : ConjureMotionBehavior
{
    [Min(0)] public float moveSpeed = 13f;
    [Min(0)] public float turnSpeed = 5f;
    public override float Duration => 5f;

    private ICombatTarget _target;
    private Vector3 _moveDirection;

    public override ConjureMotionBehavior Clone()
    {
        return new HomingMotion
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
public class BoomerangMotion : ConjureMotionBehavior
{
        
    [Min(0)] public float moveSpeed = 15f;
    [Min(0)] public float turnSpeed = 5f;
    [Min(0)] public float returnAfterInSeconds = 5f;
    public override float Duration => returnAfterInSeconds * 2f;

    private Vector3 _moveDirection;
    private float _timeSinceSpawn;

    public override ConjureMotionBehavior Clone()
    {
        return new BoomerangMotion
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
    
[System.Serializable]
[ProjectileMovement("Primed")]
public class PrimedMotion : ConjureMotionBehavior
{
    [Min(0)] public float primeTime = 2f;
    [Min(0)] public float moveSpeed = 15f;
    [Min(0)] public float vibrateIntensity = 0.2f;
    [Min(0)] public float vibrateSpeed = 20f;
    
    public override float Duration => 7f;
    
    private Vector3 _launchDirection;
    private Vector3 _spawnPosition;
    private float _elapsedTime;
    private bool _hasLaunched;
    
    public override ConjureMotionBehavior Clone()
    {
        return new PrimedMotion
        {
            primeTime = primeTime,
            moveSpeed = moveSpeed,
            vibrateIntensity = vibrateIntensity,
            vibrateSpeed = vibrateSpeed
        };
    }
    
    public override void Initialize(Rigidbody rigidbody, ICombatTarget source, ICombatTarget target)
    {
        projectileRb = rigidbody;
        casterSource = source;
        _launchDirection = casterSource.LookDirection;
        _spawnPosition = rigidbody.position;
        _elapsedTime = 0f;
        _hasLaunched = false;
        projectileRb.isKinematic = true;
    }
    
    public override void UpdateMovement(float delta)
    {
        _elapsedTime += delta;
        
        if (!_hasLaunched)
        {
            if (_elapsedTime >= primeTime)
            {
                _hasLaunched = true;
                projectileRb.isKinematic = false;
            }
            else
            {
                float vibrationOffset = Mathf.Sin(_elapsedTime * vibrateSpeed) * vibrateIntensity;
                Vector3 vibrateDirection = Vector3.Cross(_launchDirection, Vector3.up).normalized;
                projectileRb.position = _spawnPosition + vibrateDirection * vibrationOffset;
                projectileRb.rotation = Quaternion.LookRotation(_launchDirection);
            }
        }
        else
        {
            projectileRb.position += _launchDirection * (moveSpeed * delta);
            projectileRb.rotation = Quaternion.LookRotation(_launchDirection);
        }
    }
}
    
}