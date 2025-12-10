using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ProjectileCollisionBehavior
{
    protected Rigidbody projectileRb;
    protected ICombatTarget casterSource;
    
    public abstract ProjectileCollisionBehavior Clone();
    public abstract void Initialize(Rigidbody rigidBody, ICombatTarget source);
    public abstract void OnCollision(Projectile projectile, Collision other);


}


[System.Serializable]
[ProjectileCollision("Destroy")]
public class DestroyBehavior  : ProjectileCollisionBehavior
{
    public override ProjectileCollisionBehavior Clone()
    {
        return new DestroyBehavior { };
    }

    public override void Initialize(Rigidbody rigidBody, ICombatTarget source)
    {
        
        projectileRb = rigidBody;
        casterSource = source;
    }
    
    public override void OnCollision(Projectile projectile, Collision other)
    {
        projectile.DestroyProjectile();
    }
}