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