using UnityEngine;

public interface ISpellTarget
{
    void TakeDamage(float damage);
    void Heal(float amount);
    void ApplyForce(Vector3 force);
    Transform Transform { get; }
}