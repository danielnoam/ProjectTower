using UnityEngine;

[System.Serializable]
public abstract class ProjectileMovementBehavior
{
    protected Transform projectileTransform;
    protected ICombatTarget casterSource;

    public abstract void Initialize(Transform projectileTransform, ICombatTarget source);
    public abstract void UpdateMovement();
}


[System.Serializable]
public class NormalMovement  : ProjectileMovementBehavior
{
    [Min(0)] public float moveSpeed = 5f;
    
    private Vector3 _moveDirection;
    
    public override void Initialize(Transform transform, ICombatTarget source)
    {
        projectileTransform = transform;
        casterSource = source;
        _moveDirection = casterSource.Transform.forward;
    }
    
    public override void UpdateMovement()
    {
        projectileTransform.position += _moveDirection * (moveSpeed * Time.deltaTime);
        projectileTransform.rotation = Quaternion.LookRotation(_moveDirection);
    }
}