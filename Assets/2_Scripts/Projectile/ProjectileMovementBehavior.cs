using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ProjectileMovementBehavior
{
    protected Rigidbody projectileRb;
    protected ICombatTarget casterSource;

    public abstract ProjectileMovementBehavior Clone();
    public abstract void Initialize(Rigidbody projectileTransform, ICombatTarget source);
    public abstract void UpdateMovement();
}


[System.Serializable]
public class NormalMovement  : ProjectileMovementBehavior
{
    [Min(0)] public float moveSpeed = 5f;
    
    private Vector3 _moveDirection;
    
    public override ProjectileMovementBehavior Clone()
    {
        return new NormalMovement 
        { 
            moveSpeed = moveSpeed 
        };
    }
    
    public override void Initialize(Rigidbody rigidbody, ICombatTarget source)
    {
        projectileRb = rigidbody;
        casterSource = source;
        _moveDirection = casterSource.LookDirection;
    }
    
    public override void UpdateMovement()
    {
        projectileRb.position += _moveDirection * (moveSpeed * Time.deltaTime);
        projectileRb.rotation = Quaternion.LookRotation(_moveDirection);
    }
}