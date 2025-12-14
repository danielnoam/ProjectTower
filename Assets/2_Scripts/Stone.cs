using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpawnPointReset))]
public class Stone : MonoBehaviour, ICombatTarget
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
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
        force = Mathf.Clamp(force, 0, 35);
        _rigidbody.AddForce(direction * force, ForceMode.Impulse);
    }

    public void ApplyStatus(StatusEffect status)
    {

    }

    public Transform Transform => transform;
    public Vector3 LookDirection => transform.forward;
}
