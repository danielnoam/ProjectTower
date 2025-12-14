using System;
using DNExtensions;
using DNExtensions.ControllerRumbleSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase]
[DisallowMultipleComponent]
[RequireComponent(typeof(FPCMovement))]
[RequireComponent(typeof(FPCInteraction))]
[RequireComponent(typeof(FPCInput))]
[RequireComponent(typeof(FPCRigidBodyPush))]
[RequireComponent(typeof(CharacterController))]
public class FPCManager : MonoBehaviour, ICombatTarget
{
    public static FPCManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private FPCMovement fpcMovement;
    [SerializeField] private FPCInteraction fpcInteraction;
    [SerializeField] private FPCCamera fpcCamera;
    [SerializeField] private FPCInput fpcInput;
    [SerializeField] private FPCRigidBodyPush fpcRigidBodyPush;
    [SerializeField] private FPCCaster fpcCaster;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ControllerRumbleSource controllerRumbleSource;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private InventoryComponent inventoryComponent;
    [SerializeField] private SpellCasterComponent spellCasterComponent;
    [SerializeField] private StatusEffectComponent statusEffectComponent;
    
    
    public FPCMovement FpcMovement => fpcMovement;
    public FPCInteraction FpcInteraction => fpcInteraction;
    public FPCCamera FpcCamera => fpcCamera;
    public FPCInput FpcInput => fpcInput;
    public FPCRigidBodyPush FpcRigidBodyPush => fpcRigidBodyPush;
    public FPCCaster FpcCaster => fpcCaster;
    public CharacterController CharacterController => characterController;
    public ControllerRumbleSource ControllerRumbleSource => controllerRumbleSource;
    public HealthComponent HealthComponent => healthComponent;
    public InventoryComponent InventoryComponent => inventoryComponent;
    public SpellCasterComponent SpellCasterComponent => spellCasterComponent;

    private void OnValidate()
    {
        if (!fpcMovement) fpcMovement = gameObject.GetOrAddComponent<FPCMovement>();
        if (!fpcInteraction) fpcInteraction = gameObject.GetOrAddComponent<FPCInteraction>();
        if (!fpcInput) fpcInput = gameObject.GetOrAddComponent<FPCInput>();
        if (!fpcRigidBodyPush) fpcRigidBodyPush = gameObject.GetOrAddComponent<FPCRigidBodyPush>();
        if (!characterController) characterController = gameObject.GetOrAddComponent<CharacterController>();
    }

    private void Awake()
    {
        if (!Instance || Instance == this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage, ICombatTarget damageDealer)
    {
        healthComponent.TakeDamage(damage, damageDealer);
    }

    public void Heal(float amount)
    {
        healthComponent.Heal(amount);
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        fpcMovement.ApplyForce(direction, force);
    }

    public void ApplyStatus(StatusEffect status)
    {
        statusEffectComponent.ApplyStatus(status);
    }

    public Transform Transform => transform;
    public Vector3 LookDirection => fpcCamera.GetAimDirection();
}

