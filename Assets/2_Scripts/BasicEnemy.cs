using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Rigidbody))]
public class BasicEnemy : MonoBehaviour, ICombatTarget
{

    [Header("Behavior")]
    [SerializeField] private AIBehavior aiBehavior = AIBehavior.Chase;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderInterval = 3f;
    [SerializeField] private float attackInterval = 3f;
    [SerializeField] private SOSpell attackSpell;
    
    [Header("References")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private SpellCasterComponent spellCasterComponent;


    private enum AIBehavior { Idle, Wander, Chase }
    private ICombatTarget _currentTarget;
    private float _nextWanderTime;
    private float _nextAttackTime;
    
    
    
    
    private void Awake()
    {
        healthComponent.Died += OnDied;
    }

    private void OnDied()
    {
        Destroy(gameObject);
    }
    

    private void Update() {
        
        switch(aiBehavior) {
            
            case AIBehavior.Idle:

                break;
                
            case AIBehavior.Wander:
                if (Time.time > _nextWanderTime) {
                    Vector3 randomDir = UnityEngine.Random.insideUnitSphere * wanderRadius;
                    randomDir += transform.position;
                    randomDir.y = transform.position.y;
                    
                    agent.SetDestination(randomDir);
                    _nextWanderTime = Time.time + wanderInterval;
                }
                break;
                
            case AIBehavior.Chase:
                if (_currentTarget != null)
                {
                    if (Vector3.Distance(agent.destination, _currentTarget.Transform.position) > 0.5f)
                    {
                       if (agent.isOnNavMesh) agent.SetDestination(_currentTarget.Transform.position);
                    }
        
                    if (Time.time > _nextAttackTime) {
                        _nextAttackTime = Time.time + attackInterval;
                        Attack(_currentTarget);
                    }
                }
                
                
                break;
        }
    }
    
    private void Attack(ICombatTarget target)
    {
        if (target == null) return;
        
        spellCasterComponent.CastSpell(attackSpell, target);
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR

        var info = "";
        
        info += $"Behavior: {aiBehavior}\n";
        info += $"Attack Spell: {attackSpell}\n";
        if (_currentTarget != null) info += $"Target: {_currentTarget.Transform.name}\n";
        
        Handles.Label(transform.position + Vector3.up * 2f, info);
        #endif
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

    public void ApplyForce(Vector3 force)
    {
        agent.enabled = false;
        rigidBody.AddForce(force, ForceMode.Impulse);
    }

    public Transform Transform => transform;
    public Vector3 LookDirection => transform.forward;
}


