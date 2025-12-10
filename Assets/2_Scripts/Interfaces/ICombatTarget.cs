using UnityEngine;

public interface ICombatTarget
{
    void TakeDamage(float damage, ICombatTarget damageDealer);
    void Heal(float amount);
    void ApplyForce(Vector3 force);
    Transform Transform { get; }
    Vector3 LookDirection { get; }
}