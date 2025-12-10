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
    [Header("Settings")]
    [SerializeField] private FPCCameraMode cameraMode = FPCCameraMode.CinemachineCamera;
    
    [Header("References")]
    [SerializeField] private FPCMovement fpcMovement;
    [SerializeField] private FPCInteraction fpcInteraction;
    [SerializeField] private FPCCameraBase fpcCamera;
    [SerializeField] private FPCInput fpcInput;
    [SerializeField] private FPCRigidBodyPush fpcRigidBodyPush;
    [SerializeField] private FPCCaster fpcCaster;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ControllerRumbleSource controllerRumbleSource;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private InventoryComponent inventoryComponent;
    [SerializeField] private ManaSourceComponent manaSourceComponent;

    private enum FPCCameraMode { NormalCamera, CinemachineCamera }
    
    public FPCMovement FpcMovement => fpcMovement;
    public FPCInteraction FpcInteraction => fpcInteraction;
    public FPCCameraBase FpcCamera => fpcCamera;
    public FPCInput FpcInput => fpcInput;
    public FPCRigidBodyPush FpcRigidBodyPush => fpcRigidBodyPush;
    public FPCCaster FpcCaster => fpcCaster;
    public CharacterController CharacterController => characterController;
    public ControllerRumbleSource ControllerRumbleSource => controllerRumbleSource;
    public HealthComponent HealthComponent => healthComponent;
    public InventoryComponent InventoryComponent => inventoryComponent;
    public ManaSourceComponent ManaSourceComponent => manaSourceComponent;

    private void OnValidate()
    {
        if (!fpcMovement) fpcMovement = gameObject.GetOrAddComponent<FPCMovement>();
        if (!fpcInteraction) fpcInteraction = gameObject.GetOrAddComponent<FPCInteraction>();
        if (!fpcInput) fpcInput = gameObject.GetOrAddComponent<FPCInput>();
        if (!fpcRigidBodyPush) fpcRigidBodyPush = gameObject.GetOrAddComponent<FPCRigidBodyPush>();
        if (!characterController) characterController = gameObject.GetOrAddComponent<CharacterController>();
        HandleCameraComponentSwitch();
    }

    private void HandleCameraComponentSwitch()
    {
        Type requiredCameraType = cameraMode switch
        {
            FPCCameraMode.NormalCamera => typeof(FPCCameraNormal),
            FPCCameraMode.CinemachineCamera => typeof(FPCCameraCinemachine),
            _ => null
        };

        if (requiredCameraType == null) return;
        
        FPCCameraBase existingCamera = GetComponent<FPCCameraBase>();

        if (existingCamera && existingCamera.GetType() != requiredCameraType)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (this != null && existingCamera != null)
                    {
                        DestroyImmediate(existingCamera);
                        if (cameraMode == FPCCameraMode.NormalCamera)
                        {
                            fpcCamera = gameObject.GetOrAddComponent<FPCCameraNormal>();
                        }
                        else if (cameraMode == FPCCameraMode.CinemachineCamera)
                        {
                            fpcCamera = gameObject.GetOrAddComponent<FPCCameraCinemachine>();
                        }
                    }
                };
            }
            else
            #endif
            {
                Destroy(existingCamera);
                if (cameraMode == FPCCameraMode.NormalCamera)
                {
                    fpcCamera = gameObject.GetOrAddComponent<FPCCameraNormal>();
                }
                else if (cameraMode == FPCCameraMode.CinemachineCamera)
                {
                    fpcCamera = gameObject.GetOrAddComponent<FPCCameraCinemachine>();
                }
            }
        }
        else if (!existingCamera)
        {
            if (cameraMode == FPCCameraMode.NormalCamera)
            {
                fpcCamera = gameObject.GetOrAddComponent<FPCCameraNormal>();
            }
            else if (cameraMode == FPCCameraMode.CinemachineCamera)
            {
                fpcCamera = gameObject.GetOrAddComponent<FPCCameraCinemachine>();
            }
        }
        else
        {
            fpcCamera = existingCamera;
        }
    }

    public void TakeDamage(float damage)
    {
        healthComponent.TakeDamage(damage);
    }

    public void Heal(float amount)
    {
        healthComponent.Heal(amount);
    }

    public void ApplyForce(Vector3 force)
    {
        fpcMovement.ApplyForce(force);
    }

    public Transform Transform => transform;
    public Vector3 LookDirection => fpcCamera.GetAimDirection();
}

