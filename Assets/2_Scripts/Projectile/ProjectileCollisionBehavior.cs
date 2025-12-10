using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class ProjectileCollisionBehavior
{
    protected Transform projectileTransform;
    protected ICombatTarget casterSource;

    public abstract ProjectileCollisionBehavior Clone();
    public abstract void Collision(Projectile projectile, Collision other);
}


[System.Serializable]
public class DestroyBehavior  : ProjectileCollisionBehavior
{
    public override ProjectileCollisionBehavior Clone()
    {
        return new DestroyBehavior { };
    }

    public override void Collision(Projectile projectile, Collision other)
    {
        projectile.DestroyProjectile();
    }
}