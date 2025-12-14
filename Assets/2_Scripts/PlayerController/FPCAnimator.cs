using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager))]
public class FPCAnimator : MonoBehaviour
{
    [Header("Movement Tilt")]
    [SerializeField] private bool enableMovementTilt = true;
    [SerializeField] private float horizontalTiltAmount = 2f;
    [SerializeField] private float forwardPanAmount = 1f;  
    [SerializeField] private float tiltSmoothing = 5f;
    
    [Header("References")]
    [SerializeField] private FPCManager manager;
    [SerializeField] private Transform wandPivot;

    private float _movementTilt;
    private float _movementPan;
    private Quaternion _baseRotation;


    private void OnValidate()
    {
        if (!manager) manager = GetComponent<FPCManager>();
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
        
        Quaternion movementOffset = Quaternion.Euler(_movementPan, 0, _movementTilt);
        wandPivot.localRotation = _baseRotation * movementOffset;
    }
    
    
}