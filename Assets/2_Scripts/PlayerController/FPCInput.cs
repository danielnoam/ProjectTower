using System;
using DNExtensions.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(FPCManager))]
public class FPCInput : InputReaderBase
{
        private InputActionMap _playerActionMap;
        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _attack2Action;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _runAction;
        private InputAction _interactAction;
        private InputAction _throwAction;
        private InputAction _toggleMenu;
        
        
        public event Action<InputAction.CallbackContext> OnMoveAction;
        public event Action<InputAction.CallbackContext> OnAttackAction;
        public event Action<InputAction.CallbackContext> OnAttack2Action;
        public event Action<InputAction.CallbackContext> OnLookAction;
        public event Action<InputAction.CallbackContext> OnJumpAction;
        public event Action<InputAction.CallbackContext> OnRunAction;
        public event Action<InputAction.CallbackContext> OnInteractAction;
        public event Action<InputAction.CallbackContext> OnThrowAction;
        public event Action<InputAction.CallbackContext> OnToggleMenuAction;


        private void Awake()
        {

            _playerActionMap = PlayerInput.actions.FindActionMap("Player");

            if (_playerActionMap == null)
            {
                Debug.LogError("Player Action Map not found. Please check the action maps in the Player Input component.");
                return;
            }

            _moveAction = _playerActionMap.FindAction("Move");
            _lookAction = _playerActionMap.FindAction("Look");
            _jumpAction = _playerActionMap.FindAction("Jump");
            _runAction = _playerActionMap.FindAction("Run");
            _interactAction = _playerActionMap.FindAction("Interact");
            _throwAction = _playerActionMap.FindAction("Throw");
            _attackAction = _playerActionMap.FindAction("Attack");
            _attack2Action = _playerActionMap.FindAction("Attack2");
            _toggleMenu = _playerActionMap.FindAction("ToggleMenu");
            
            if (_moveAction == null) Debug.LogError("Move action not found in Player Action Map.");
            if (_attackAction == null) Debug.LogError("Attack action not found in Player Action Map.");
            if (_attack2Action == null) Debug.LogError("Attack action not found in Player Action Map.");
            if (_lookAction == null) Debug.LogError("Look action not found in Player Action Map.");
            if (_jumpAction == null) Debug.LogError("Jump action not found in Player Action Map.");
            if (_runAction == null) Debug.LogError("Run action not found in Player Action Map.");
            if (_interactAction == null) Debug.LogError("Interact action not found in Player Action Map.");
            if (_throwAction == null) Debug.LogError("Throw action not found in Player Action Map.");
            if (_toggleMenu == null) Debug.LogError("ToggleMenu action not found in Player Action Map.");
            
            _playerActionMap.Enable();

        }


        private void OnEnable()
        {
            SubscribeToAction(_moveAction, OnMove);
            SubscribeToAction(_attackAction, OnAttack);
            SubscribeToAction(_attack2Action, OnAttack2);
            SubscribeToAction(_lookAction, OnLook);
            SubscribeToAction(_jumpAction, OnJump);
            SubscribeToAction(_runAction, OnRun);
            SubscribeToAction(_interactAction, OnInteract);
            SubscribeToAction(_throwAction, OnThrow);
            SubscribeToAction(_toggleMenu, OnToggleMenu);


        }

        private void OnDisable()
        {
            UnsubscribeFromAction(_moveAction, OnMove);
            UnsubscribeFromAction(_attackAction, OnAttack);
            UnsubscribeFromAction(_attack2Action, OnAttack2);
            UnsubscribeFromAction(_lookAction, OnLook);
            UnsubscribeFromAction(_jumpAction, OnJump);
            UnsubscribeFromAction(_runAction, OnRun);
            UnsubscribeFromAction(_interactAction, OnInteract);
            UnsubscribeFromAction(_throwAction, OnThrow);
            UnsubscribeFromAction(_toggleMenu, OnToggleMenu);
        }

        
        private void OnAttack(InputAction.CallbackContext context)
        {
            OnAttackAction?.Invoke(context);
        }
        
        private void OnAttack2(InputAction.CallbackContext context)
        {
            OnAttack2Action?.Invoke(context);
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            OnMoveAction?.Invoke(context);
        }
        
        private void OnLook(InputAction.CallbackContext context)
        {
           

            OnLookAction?.Invoke(context);
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
           

            OnJumpAction?.Invoke(context);
        }
        
        private void OnRun(InputAction.CallbackContext context)
        {
           

            OnRunAction?.Invoke(context);
        }
        
        
        private void OnInteract(InputAction.CallbackContext context)
        {
           

            OnInteractAction?.Invoke(context);
        }
        
        private void OnThrow(InputAction.CallbackContext context)
        {

            OnThrowAction?.Invoke(context);
        }
        
        private void OnToggleMenu(InputAction.CallbackContext context)
        {
            OnToggleMenuAction?.Invoke(context);
        }
        

   
}