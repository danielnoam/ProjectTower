using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthComponent))]
public class Box : MonoBehaviour, ICombatTarget
{
    private Rigidbody _rigidbody;
    private HealthComponent _healthComponent;
    private StatusEffectComponent _statusEffectComponent;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _healthComponent = GetComponent<HealthComponent>();
        _statusEffectComponent = GetComponent<StatusEffectComponent>();
        _healthComponent.Died += OnDeath;
        _healthComponent.Reset();
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float damage, ICombatTarget damageDealer)
    {
        _healthComponent.TakeDamage(damage, damageDealer);
        
    }

    public void Heal(float amount)
    {
        _healthComponent.Heal(amount);
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        force = Mathf.Clamp(force, 0, 35);
        _rigidbody.AddForce(direction * force, ForceMode.Impulse);
    }

    public void ApplyStatus(StatusEffect status)
    {
        _statusEffectComponent.ApplyStatus(status);
    }

    public Transform Transform => transform;
    public Vector3 LookDirection => transform.forward;
}
