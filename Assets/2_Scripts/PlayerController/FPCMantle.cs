using System;
using System.Collections;
using UnityEngine;

namespace DNExtensions
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FPCManager))]
    public class FPCMantle : MonoBehaviour
    {
        [Header("Mantle Settings")]
        [SerializeField] private bool canMantle = true;
        [SerializeField] private LayerMask mantleRaycastLayer;
        [SerializeField] private float mantleSpeed = 12f;
        [SerializeField] private float mantleBufferTime = 0.1f;
        [SerializeField] private AnimationCurve mantleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Detection")]
        [SerializeField] private float mantleRaycastDistance = 1.5f;
        [SerializeField] private float mantleMaxHeight = 2.5f;
        [SerializeField] private float mantleMinHeight = 0.5f;
        [SerializeField] private float mantleForwardDistance = 0.5f;
        [SerializeField] private float lowerRayHeight = 0.1f;
        [SerializeField] private float upperRayHeight = 0.5f;
        
        [Header("References")]
        [SerializeField] private FPCManager manager;
        [SerializeField] private FPCMovement movement;
        
        private float _mantleBufferCounter;
        private bool _isMantling;
        
        public bool IsMantling => _isMantling;
        public bool CanMantleNow { get; private set; }
        
        private void OnValidate()
        {
            if (!manager) manager = GetComponent<FPCManager>();
            if (!movement) movement = GetComponent<FPCMovement>();
        }
        
        private void OnEnable()
        {
            manager.FpcInput.OnJumpAction += HandleJumpInput;
        }
        
        private void OnDisable()
        {
            manager.FpcInput.OnJumpAction -= HandleJumpInput;
        }
        
        private void Update()
        {
            if (!canMantle) return;
            
            CheckMantleAvailable();
            
            if (_mantleBufferCounter > 0f)
            {
                _mantleBufferCounter -= Time.deltaTime;
            }
            
            if (_isMantling) return;
            
            if (_mantleBufferCounter > 0f)
            {
                TryMantle();
            }
        }
        
        private void CheckMantleAvailable()
        {
            CharacterController cc = manager.CharacterController;
            Vector3 forward = transform.forward;
            
            Vector3 lowerOrigin = transform.position + Vector3.up * (cc.height * lowerRayHeight);
            Vector3 upperOrigin = transform.position + Vector3.up * (cc.height * upperRayHeight);
            
            bool lowerHit = Physics.Raycast(lowerOrigin, forward, mantleRaycastDistance, mantleRaycastLayer);
            bool upperHit = Physics.Raycast(upperOrigin, forward, mantleRaycastDistance, mantleRaycastLayer);
            
            // Can mantle if either ray hits (we'll check for space above in TryMantle)
            CanMantleNow = lowerHit || upperHit;
        }
        
        private void HandleJumpInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.phase == UnityEngine.InputSystem.InputActionPhase.Started && canMantle)
            {
                _mantleBufferCounter = mantleBufferTime;
            }
        }
        
        private void TryMantle()
        {
            CharacterController cc = manager.CharacterController;
            Vector3 forward = transform.forward;
            
            Vector3 lowerOrigin = transform.position + Vector3.up * (cc.height * lowerRayHeight);
            Vector3 upperOrigin = transform.position + Vector3.up * (cc.height * upperRayHeight);
            
            bool lowerHit = Physics.Raycast(lowerOrigin, forward, out RaycastHit lowerWallHit, mantleRaycastDistance, mantleRaycastLayer);
            bool upperHit = Physics.Raycast(upperOrigin, forward, out RaycastHit upperWallHit, mantleRaycastDistance, mantleRaycastLayer);
            
            // Use whichever hit we have
            if (lowerHit)
            {
                TryMantleOnTop(lowerWallHit, forward);
            }
            else if (upperHit)
            {
                TryMantleOnTop(upperWallHit, forward);
            }
        }
        
        private void TryMantleOnTop(RaycastHit wallHit, Vector3 forward)
        {
            CharacterController cc = manager.CharacterController;
            
            // Start the downward raycast from well above the obstacle
            Vector3 ledgeCheckOrigin = wallHit.point + Vector3.up * mantleMaxHeight;
            
            // First, move forward to be PAST the wall before checking down
            ledgeCheckOrigin += forward * (cc.radius + mantleForwardDistance);
            
            if (Physics.Raycast(ledgeCheckOrigin, Vector3.down, out RaycastHit ledgeHit, mantleMaxHeight * 2f, mantleRaycastLayer))
            {
                float ledgeHeight = ledgeHit.point.y - transform.position.y;
                
                // Check if ledge is within acceptable height range
                if (ledgeHeight > mantleMinHeight && ledgeHeight <= mantleMaxHeight)
                {
                    // Target position is on the ledge surface, don't add extra forward offset
                    Vector3 targetPosition = ledgeHit.point + Vector3.up;
                    
                    // Check if target position is valid (no collision)
                    if (IsPositionValid(targetPosition))
                    {
                        StartMantle(targetPosition);
                    }
                }
            }
        }
        
        private bool IsPositionValid(Vector3 position)
        {
            CharacterController cc = manager.CharacterController;
            float radius = cc.radius;
            float height = cc.height;
            
            Vector3 point1 = position + Vector3.up * radius;
            Vector3 point2 = position + Vector3.up * (height - radius);
            
            return !Physics.CheckCapsule(point1, point2, radius * 0.95f, mantleRaycastLayer);
        }
        
        private void StartMantle(Vector3 targetPosition)
        {
            StartCoroutine(MantleCoroutine(targetPosition));
            _mantleBufferCounter = 0f;
            movement.CancelJumpBuffer();
        }
        
        private IEnumerator MantleCoroutine(Vector3 targetPosition)
        {
            _isMantling = true;
            movement.ForceStop();
    
            manager.CharacterController.enabled = false;
    
            Vector3 startPos = transform.position;
            float mantleDuration = Vector3.Distance(startPos, targetPosition) / mantleSpeed;
            float elapsed = 0f;
    
            while (elapsed < mantleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / mantleDuration);
        
                // Apply curve to the interpolation
                float curvedT = mantleCurve.Evaluate(t);
                Vector3 newPos = Vector3.Lerp(startPos, targetPosition, curvedT);
        
                transform.position = newPos;
                yield return null;
            }
    
            // Ensure we end at exact target
            transform.position = targetPosition;
    
            manager.CharacterController.enabled = true;
            _isMantling = false;
        }
        
        private void OnDrawGizmos()
        {
            if (!canMantle || !manager) return;
            
            CharacterController cc = manager.CharacterController;
            Vector3 forward = transform.forward;
            
            Vector3 lowerOrigin = transform.position + Vector3.up * (cc.height * lowerRayHeight);
            Vector3 upperOrigin = transform.position + Vector3.up * (cc.height * upperRayHeight);
            
            // Draw lower raycast
            bool lowerHit = Physics.Raycast(lowerOrigin, forward, out RaycastHit lowerWallHit, mantleRaycastDistance, mantleRaycastLayer);
            Gizmos.color = lowerHit ? Color.red : Color.yellow;
            Gizmos.DrawRay(lowerOrigin, forward * mantleRaycastDistance);
            
            // Draw upper raycast
            bool upperHit = Physics.Raycast(upperOrigin, forward, out RaycastHit upperWallHit, mantleRaycastDistance, mantleRaycastLayer);
            Gizmos.color = upperHit ? Color.red : Color.yellow;
            Gizmos.DrawRay(upperOrigin, forward * mantleRaycastDistance);
            
            // Draw mantle target for whichever hit we have
            if (lowerHit)
            {
                DrawMantleGizmo(lowerWallHit, forward);
            }
            else if (upperHit)
            {
                DrawMantleGizmo(upperWallHit, forward);
            }
        }
        
        private void DrawMantleGizmo(RaycastHit wallHit, Vector3 forward)
        {
            CharacterController cc = manager.CharacterController;
            
            // Same calculation as TryMantleOnTop
            Vector3 ledgeCheckOrigin = wallHit.point + Vector3.up * mantleMaxHeight;
            ledgeCheckOrigin += forward * (cc.radius + mantleForwardDistance);
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(ledgeCheckOrigin, Vector3.down * (mantleMaxHeight * 2f));
            
            if (Physics.Raycast(ledgeCheckOrigin, Vector3.down, out RaycastHit ledgeHit, mantleMaxHeight * 2f, mantleRaycastLayer))
            {
                float ledgeHeight = ledgeHit.point.y - transform.position.y;
                Vector3 targetPosition = ledgeHit.point;
                
                bool isValidHeight = ledgeHeight > mantleMinHeight && ledgeHeight <= mantleMaxHeight;
                bool isValidPosition = IsPositionValid(targetPosition);
                
                // Color based on validity
                Gizmos.color = (isValidHeight && isValidPosition) ? Color.cyan : Color.magenta;
                
                DrawWireCapsule(
                    targetPosition + Vector3.up * cc.radius,
                    targetPosition + Vector3.up * (cc.height - cc.radius),
                    cc.radius
                );
                
                // Draw sphere at exact target position
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(targetPosition, 0.1f);
            }
        }
        
        private void DrawWireCapsule(Vector3 point1, Vector3 point2, float radius)
        {
            Gizmos.DrawWireSphere(point2, radius);
            Gizmos.DrawWireSphere(point1, radius);
            Gizmos.DrawLine(point1 + Vector3.forward * radius, point2 + Vector3.forward * radius);
            Gizmos.DrawLine(point1 - Vector3.forward * radius, point2 - Vector3.forward * radius);
            Gizmos.DrawLine(point1 + Vector3.right * radius, point2 + Vector3.right * radius);
            Gizmos.DrawLine(point1 - Vector3.right * radius, point2 - Vector3.right * radius);
        }
    }
}