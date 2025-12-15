
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DNExtensions;
using DNExtensions.ControllerRumbleSystem;


[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager))]
public class FPCMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private bool canRun = true;
    [ShowIf("canRun")][SerializeField] private float runSpeed = 12f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private ControllerRumbleEffectSettings landingRumbleSettings = new ControllerRumbleEffectSettings(0.5f, 0.7f, 0.2f);
    
    [Header("Platform Movement")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float platformCheckDistance = 1.2f;

    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 1.5f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    
    
    [Header("References")] 
    [SerializeField] private FPCManager manager;
    
    [Separator]
    [SerializeField, ReadOnly] private Vector3 velocity;
    [SerializeField, ReadOnly] private Vector3 lastPlatformPosition;
    [SerializeField, ReadOnly] private float fallTime;


    private Vector3 _externalForce;
    private Vector2 _moveInput;
    private bool _wasGrounded;
    private bool _runInput;
    private float _dashTimeRemaining;
    private float _dashCooldownRemaining;
    private float _jumpBufferCounter;
    private float _coyoteTimeCounter;
    private Rigidbody _currentPlatform;


    public Vector2 MoveInput => _moveInput;
    public Vector3 Velocity => velocity;
    
    public bool IsGrounded { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsFalling { get; private set; }
    
    
    public event Action OnJump;
    public event Action<float> OnLand;


    private void OnValidate()
    {
        if (!manager) manager = GetComponent<FPCManager>();
    }

    private void OnEnable()
    {
        manager.FpcInput.OnMoveAction += GetMovementInput;
        manager.FpcInput.OnRunAction += GetRunningInput;
        manager.FpcInput.OnJumpAction += GetJumpInput;
    }

    private void OnDisable()
    {
        manager.FpcInput.OnMoveAction -= GetMovementInput;
        manager.FpcInput.OnRunAction -= GetRunningInput;
        manager.FpcInput.OnJumpAction -= GetJumpInput;
    }
    
    private void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
        ApplyMovement();
    }
    


    private void GetMovementInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
        
    private void GetRunningInput(InputAction.CallbackContext context)
    {
        _runInput = context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed;
    }
        
    private void GetJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            _jumpBufferCounter = jumpBufferTime;
        }
    }
    
    private void ApplyMovement()
    {
        if (_currentPlatform)
        {
            Vector3 platformDelta = _currentPlatform.position - lastPlatformPosition;
            manager.CharacterController.Move(platformDelta);
            lastPlatformPosition = _currentPlatform.position;
        }
    
        Vector3 finalMovement = (velocity + _externalForce) * Time.deltaTime;
        manager.CharacterController.Move(finalMovement);
    
        _externalForce = Vector3.Lerp(_externalForce, Vector3.zero, 5f * Time.deltaTime);
        
    }
    private void HandleMovement()
    {
        Vector3 cameraForward = manager.FpcCamera.GetMovementDirection();
        Vector3 cameraRight = Quaternion.Euler(0, 90, 0) * cameraForward;
        Vector3 moveDir = (cameraForward * _moveInput.y + cameraRight * _moveInput.x).normalized;

        IsRunning = _runInput && canRun;
        float targetMoveSpeed = IsRunning ? runSpeed : walkSpeed;
        if (manager.FpcInteraction.HeldObject)
        {
            targetMoveSpeed /= manager.FpcInteraction.HeldObject.ObjectWeight;
        }
        
        velocity.x = moveDir.x * targetMoveSpeed;
        velocity.z = moveDir.z * targetMoveSpeed;
    }
    
    private void HandleJump()
    {
        
        if (_jumpBufferCounter > 0f)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
        
        if (_jumpBufferCounter > 0f && (_coyoteTimeCounter > 0f || IsGrounded))
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;  
            OnJump?.Invoke();
        }
        
        velocity.y += gravity * Time.deltaTime;
        IsJumping = velocity.y > 0;
    }
    
    
    private void CheckGrounded()
    {
        _wasGrounded = IsGrounded;
        IsGrounded = manager.CharacterController.isGrounded;
        IsFalling = velocity.y < 0;
    
        if (IsGrounded && !_wasGrounded)
        {
            manager.ControllerRumbleSource?.Rumble(landingRumbleSettings);
            OnLand?.Invoke(fallTime);
            fallTime = 0;
        }
    

        if (IsGrounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, platformCheckDistance, platformLayer))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
            
                if (rb && !rb.isKinematic)
                {
                    if (_currentPlatform != rb)
                    {
                        _currentPlatform = rb;
                        lastPlatformPosition = rb.position;
                    }
                }
                else
                {
                    _currentPlatform = null;
                    lastPlatformPosition = Vector3.zero;
                }
            }
            else
            {
                _currentPlatform = null;
                lastPlatformPosition = Vector3.zero;
            }
        
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
            _coyoteTimeCounter = coyoteTime;
        }
        else
        {
            _currentPlatform = null;
            lastPlatformPosition = Vector3.zero;
            if (IsFalling) fallTime += Time.deltaTime;
        
            if (_wasGrounded)
            {
                _coyoteTimeCounter = coyoteTime;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }
    }


    public void ApplyForce(Vector3 direction, float force)
    {
        _externalForce += direction * force;
    }


    private void OnDrawGizmos()
    {

        if (IsGrounded)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector3.down * platformCheckDistance);
        }
    }
}