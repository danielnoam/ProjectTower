using DNExtensions.CinemachineImpulseSystem;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager))]
public class FPCCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Range(0, 0.1f)] private float mouseLookSensitivity = 0.04f;
    [SerializeField] [Range(0, 300f)] private float gamepadLookSensitivity = 3f;
    [SerializeField] [Range(0, 0.1f)] private float lookSmoothing;
    [SerializeField] private Vector2 verticalAxisRange = new(-90, 90);
    [SerializeField] private bool invertHorizontal;
    [SerializeField] private bool invertVertical;
    
    [Header("FOV")]
    [SerializeField] protected float baseFov = 60f;
    [SerializeField] protected float runFovMultiplier = 1.3f;
    [SerializeField] protected float fovChangeSmoothing = 5;
    
    [Header("Shake")]
    [SerializeField] private ImpulseSettings landImpulseSettings;
    
    [Header("Movement Tilt")]
    [SerializeField] private bool enableMovementTilt = true;
    [SerializeField] private float horizontalTiltAmount = 2f;
    [SerializeField] private float forwardPanAmount = 1f;  
    [SerializeField] private float tiltSmoothing = 5f;
    
    [Header("References")]
    [SerializeField] protected FPCManager manager;
    [SerializeField] protected Transform playerHead;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private float _currentPanAngle;
    private float _currentTiltAngle;
    private float _targetPanAngle;
    private float _targetTiltAngle;
    private Vector2 _rotationVelocity;
    private Vector2 _lookInput;
    private float _baseLandIntensity;
    private float _movementTilt;
    private float _movementPan;
    

    private void OnValidate()
    {
        if (!manager) manager = GetComponent<FPCManager>();
        
        UpdateFovInEditor();
    }

    private void Awake()
    {
        _currentPanAngle = transform.eulerAngles.y;
        _currentTiltAngle = playerHead.localEulerAngles.x;
        _targetPanAngle = _currentPanAngle;
        _targetTiltAngle = _currentTiltAngle;
        _baseLandIntensity = landImpulseSettings.intensity;
    }

    private void OnEnable()
    {
        manager.FpcInput.OnLookAction += OnLook;
        manager.FpcMovement.OnLand += OnLand;
        manager.FpcMovement.OnJump += OnJump;
    }
    

    private void OnDisable()
    {
        manager.FpcInput.OnLookAction -= OnLook;
        manager.FpcMovement.OnLand -= OnLand;
        manager.FpcMovement.OnJump -= OnJump;
    }
    
    private void OnJump()
    {

    }

    private void OnLand(float fallTime)
    {
        if (fallTime <= 0.3) return;
        
        var intensity = Mathf.Clamp(_baseLandIntensity + (fallTime * 0.1f), _baseLandIntensity, 0.4f);
        impulseSource?.GenerateImpulseWithIntensity(landImpulseSettings, intensity);
    }

    private void Update()
    {
        HandleLookInput();
        UpdateFov();
        UpdateMovementTilt();
        UpdateHeadRotation();
    }
    
    private void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    private void HandleLookInput()
    {
        if (!playerHead) return;
        
        float sensitivity = mouseLookSensitivity;
        if (manager.FpcInput.IsCurrentDeviceGamepad)
        {
            sensitivity = gamepadLookSensitivity * Time.deltaTime;
        }

        float horizontalInput = invertHorizontal ? -_lookInput.x : _lookInput.x;
        float verticalInput = invertVertical ? _lookInput.y : -_lookInput.y;
        
        _targetPanAngle += horizontalInput * sensitivity;
        _targetTiltAngle += verticalInput * sensitivity;
        _targetTiltAngle = Mathf.Clamp(_targetTiltAngle, verticalAxisRange.x, verticalAxisRange.y);

        if (lookSmoothing <= 0) 
        {
            _currentPanAngle = _targetPanAngle;
            _currentTiltAngle = _targetTiltAngle;
        }
    }
    
    private void UpdateMovementTilt()
    {
        if (!enableMovementTilt || !manager.FpcMovement)
        {
            _movementTilt = Mathf.Lerp(_movementTilt, 0f, Time.deltaTime * tiltSmoothing);
            _movementPan = Mathf.Lerp(_movementPan, 0f, Time.deltaTime * tiltSmoothing);
            return;
        }

        Vector2 moveInput = manager.FpcMovement.MoveInput;
        float targetTilt = -moveInput.x * horizontalTiltAmount;
        float targetPan = moveInput.y * forwardPanAmount;
        
        if (manager.FpcMovement.Velocity.sqrMagnitude < 0.1f)
        {
            targetTilt = 0f;
            targetPan = 0f;
        }
    
        _movementTilt = Mathf.Lerp(_movementTilt, targetTilt, Time.deltaTime * tiltSmoothing);
        _movementPan = Mathf.Lerp(_movementPan, targetPan, Time.deltaTime * tiltSmoothing);
    }

    private void UpdateHeadRotation()   
    {
        if (!playerHead) return;

        if (lookSmoothing > 0)
        {
            _currentPanAngle = Mathf.SmoothDampAngle(_currentPanAngle, _targetPanAngle, ref _rotationVelocity.x, lookSmoothing);
            _currentTiltAngle = Mathf.SmoothDamp(_currentTiltAngle, _targetTiltAngle, ref _rotationVelocity.y, lookSmoothing);
        }
    
        // Body only rotates based on look input (controls movement direction)
        transform.rotation = Quaternion.Euler(0, _currentPanAngle, 0);
    
        // Head gets ALL the visual effects: vertical look + movement pan + movement tilt
        playerHead.localRotation = Quaternion.Euler(_currentTiltAngle, _movementPan, _movementTilt);
    }
    
    private void UpdateFov()
    {
        float targetFov = baseFov;
        if (manager.FpcMovement.IsRunning)
        {
            targetFov *= runFovMultiplier;
        }

        SetFieldOfView(targetFov);
    }
    
    private void UpdateFovInEditor()
    {
        if (!cinemachineCamera) return;
        
        cinemachineCamera.Lens.FieldOfView = baseFov;
    }

    private void SetFieldOfView(float targetFov)
    {
        if (!cinemachineCamera) return;
        
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(
            cinemachineCamera.Lens.FieldOfView, 
            targetFov, 
            Time.deltaTime * fovChangeSmoothing
        );
    }
    
    
    public Vector3 GetMovementDirection()
    {
        Vector3 direction = Quaternion.Euler(0, _currentPanAngle, 0) * Vector3.forward;
        return direction.normalized;
    }

    public Vector3 GetAimDirection()
    {
        return playerHead ? playerHead.forward : transform.forward;
    }

}