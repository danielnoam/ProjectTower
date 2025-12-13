using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrainingDummy : MonoBehaviour, ICombatTarget
{
    private StatusEffectComponent _statusEffectComponent;

    private void Awake()
    {
        _statusEffectComponent = GetComponent<StatusEffectComponent>();
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float damage, ICombatTarget damageDealer)
    {

        
    }

    public void Heal(float amount)
    {

    }

    public void ApplyForce(Vector3 direction, float force)
    {

    }

    public void ApplyStatus(StatusEffect status)
    {
        
        _statusEffectComponent.ApplyStatus(status);
    }

    public Transform Transform => transform;
    public Vector3 LookDirection => transform.forward;
}
