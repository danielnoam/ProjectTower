using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class BasicEnemy : MonoBehaviour, ICombatTarget
{
    [Header("AI")]
    [SerializeField] private AIBehavior aiBehavior = AIBehavior.Chase;
    
    [Header("Movement")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderInterval = 3f;
    
    [Header("Combat")]
    [SerializeField] private float attackInterval = 3f;
    [SerializeField] private float playerCheckRadius = 10f;
    [SerializeField] private float playerCheckInterval = 5f;
    [SerializeField] private SOSpell attackSpell;
    [SerializeField] private CastMethod attackCastMethod = CastMethod.Instant;
    
    [Header("References")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private SpellCasterComponent spellCasterComponent;
    [SerializeField] private StatusEffectComponent statusEffectComponent;

    private enum AIBehavior { Idle, Wander, Chase }
    private ICombatTarget _currentTarget;
    private float _nextWanderTime;
    private float _nextAttackTime;
    private float _playerCheckTime;
    
    private void Awake()
    {
        healthComponent.Died += OnDied;
    }

    private void OnDied()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        switch(aiBehavior)
        {
            case AIBehavior.Idle:
                CheckForPlayer();
                break;
                
            case AIBehavior.Wander:
                if (Time.time > _nextWanderTime)
                {
                    Vector3 randomDir = UnityEngine.Random.insideUnitSphere * wanderRadius;
                    randomDir += transform.position;
                    randomDir.y = transform.position.y;
                    
                    agent.SetDestination(randomDir);
                    _nextWanderTime = Time.time + wanderInterval;
                }
                CheckForPlayer();
                break;
                
            case AIBehavior.Chase:
                if (_currentTarget != null)
                {
                    if (Vector3.Distance(agent.destination, _currentTarget.Transform.position) > 1f)
                    {
                       if (agent.isOnNavMesh) agent.SetDestination(_currentTarget.Transform.position);
                    }
        
                    if (Time.time > _nextAttackTime)
                    {
                        _nextAttackTime = Time.time + attackInterval;
                        Attack(_currentTarget);
                    }
                }
                break;
        }
    }

    private void CheckForPlayer()
    {
        if (Time.time > _playerCheckTime)
        {
            _playerCheckTime = Time.time + playerCheckInterval;

            var colliders = Physics.OverlapSphere(transform.position, playerCheckRadius);

            foreach (var col in colliders)
            {
                if (col.TryGetComponent(out FPCManager player))
                {
                    _currentTarget = player;
                    aiBehavior = AIBehavior.Chase;
                }
            }
        }
    }
    
    private void Attack(ICombatTarget target)
    {
        if (target == null || !attackSpell) return;
        
        if (!spellCasterComponent.CanCastWithMethod(attackSpell, attackCastMethod))
        {
            if (spellCasterComponent.CanCastWithMethod(attackSpell, CastMethod.Instant))
            {
                spellCasterComponent.CastInstant(attackSpell, target);
            }
            return;
        }
        
        spellCasterComponent.CastWithMethod(attackSpell, target, attackCastMethod);
    }
    
    public void TakeDamage(float damage, ICombatTarget damageDealer)
    {
        healthComponent.TakeDamage(damage, damageDealer);
        
        if (damageDealer != null)
        {
            _currentTarget = damageDealer;
            aiBehavior = AIBehavior.Chase;
        }
    }

    public void Heal(float amount)
    {
        healthComponent.Heal(amount);
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        agent.enabled = false;
        rigidBody.AddForce(direction * force, ForceMode.Impulse);
        agent.enabled = true;
    }

    public void ApplyStatus(StatusEffect status)
    {
        statusEffectComponent.ApplyStatus(status);
    }

    public Transform Transform => transform;
    public Vector3 LookDirection => transform.forward;
    
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {

        var info = "";
        
        info += $"Behavior: {aiBehavior}\n";
        info += $"Attack Spell: {attackSpell?.label ?? "None"}\n";
        info += $"Cast Method: {attackCastMethod}\n";
        if (_currentTarget != null) info += $"Target: {_currentTarget.Transform.name}\n";
        
        Handles.Label(transform.position + Vector3.up * 2f, info);

    }
#endif
}