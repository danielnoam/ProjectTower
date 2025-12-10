using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthComponent))]
public class Box : MonoBehaviour, ICombatTarget
{
    private Rigidbody _rigidbody;
    private HealthComponent _healthComponent;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _healthComponent = GetComponent<HealthComponent>();
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

    public void ApplyForce(Vector3 force)
    {
        _rigidbody.AddForce(force, ForceMode.Impulse);
    }

    public Transform Transform => transform;
    public Vector3 LookDirection => transform.forward;
}
