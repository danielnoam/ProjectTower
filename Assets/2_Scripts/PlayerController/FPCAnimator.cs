using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager))]
public class FPCAnimator : MonoBehaviour
{


    [Header("Movement Tilt")]
    [SerializeField] private bool enableMovementTilt = true;
    [SerializeField] private float movementTiltAmount = 5f;
    [SerializeField] private float movementTiltSmoothing = 5f;
    
    [Header("Look Tilt")]
    [SerializeField] private bool enableLookTilt = true;
    [SerializeField] private float lookTiltAmount = -0.1f;
    [SerializeField] private float lookPanAmount = -0.1f;
    [SerializeField] private float lookTiltSmoothing = 5f;
    
    [Header("References")]
    [SerializeField] private FPCManager manager;
    [SerializeField] private FPCCaster caster;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform wandPivot;

    
    private static readonly int startedInstant = Animator.StringToHash("StartedInstant");
    private static readonly int startedChannel = Animator.StringToHash("StartedChannel");
    private static readonly int startedCharge = Animator.StringToHash("StartedCharge");
    
    private float _movementTilt;
    private float _lookTilt;
    private float _lookPan;
    private Quaternion _baseRotation;
    

    private void OnValidate()
    {
        if (!manager) manager = GetComponent<FPCManager>();
        if (!animator) animator = GetComponent<Animator>();
        if (!caster) caster = GetComponent<FPCCaster>();
    }

    private void OnEnable()
    {
        caster.StartedSpellCast += OnStartedSpellCast;
    }
    
    private void OnDisable()
    {
        caster.StartedSpellCast -= OnStartedSpellCast;
    }
    
    private void OnStartedSpellCast(CastMethod castMethod)
    {
        switch (castMethod)
        {
            case CastMethod.Instant:
                animator.SetTrigger(startedInstant);
                break;
            case CastMethod.Channel:
                animator.SetTrigger(startedChannel);
                break;
            case CastMethod.Charge:
                animator.SetTrigger(startedCharge);
                break;
        }
    }


    private void Awake()
    {
        if (wandPivot)
        {
            _baseRotation = wandPivot.localRotation;
        }
    }


    private void Update()
    {
        UpdateWandRotation();
    }

    private void UpdateWandRotation()
    {
        if (!wandPivot) return;
        
        UpdateMovementTilt();
        UpdateLookTilt();
        
        Quaternion movementOffset = Quaternion.Euler(0, 0, _movementTilt);
        Quaternion lookOffset = Quaternion.Euler(_lookPan, 0, _lookTilt);
        
        wandPivot.localRotation = _baseRotation * movementOffset * lookOffset;
    }

    private void UpdateMovementTilt()
    {
        if (!enableMovementTilt || !manager.FpcMovement)
        {
            _movementTilt = Mathf.Lerp(_movementTilt, 0f, Time.deltaTime * movementTiltSmoothing);
            return;
        }

        Vector2 moveInput = manager.FpcMovement.MoveInput;
        float targetTilt = -moveInput.x * movementTiltAmount;
        
        if (manager.FpcMovement.Velocity.sqrMagnitude < 0.1f)
        {
            targetTilt = 0f;
        }
        
        _movementTilt = Mathf.Lerp(_movementTilt, targetTilt, Time.deltaTime * movementTiltSmoothing);
    }

    private void UpdateLookTilt()
    {
        if (!enableLookTilt || !manager.FpcCamera)
        {
            _lookTilt = Mathf.Lerp(_lookTilt, 0f, Time.deltaTime * lookTiltSmoothing);
            _lookPan = Mathf.Lerp(_lookPan, 0f, Time.deltaTime * lookTiltSmoothing);
            return;
        }

        Vector2 lookInput = manager.FpcCamera.LookInput;
        
        float targetTilt = -lookInput.x * lookTiltAmount;
        float targetPan = lookInput.y * lookPanAmount;
        
        _lookTilt = Mathf.Lerp(_lookTilt, targetTilt, Time.deltaTime * lookTiltSmoothing);
        _lookPan = Mathf.Lerp(_lookPan, targetPan, Time.deltaTime * lookTiltSmoothing);
    }
}