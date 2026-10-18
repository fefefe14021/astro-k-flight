using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KANYE.Gameplay
{
    public class ShipInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;

        public Vector2 Move { get; private set; }
        public bool FireHeld { get; private set; }

        public event Action FirePressed;
        public event Action FireReleased;

        private InputAction moveAction;
        private InputAction fireAction;

        private void Awake()
        {
            if (actions == null)
                actions = Resources.Load<InputActionAsset>("PlayerControls");

            if (actions == null)
                throw new InvalidOperationException("ShipInput: не найден InputActionAsset PlayerControls.");

            actions = Instantiate(actions);

            var map = actions.FindActionMap("Gameplay", true);
            moveAction = map.FindAction("Move", true);
            fireAction = map.FindAction("Fire", true);
            fireAction.started += OnFireStarted;
            fireAction.canceled += OnFireCanceled;
            map.Enable();
        }

        private void OnDestroy()
        {
            if (fireAction != null)
            {
                fireAction.started -= OnFireStarted;
                fireAction.canceled -= OnFireCanceled;
            }

            if (actions != null)
                Destroy(actions);
        }

        private void Update()
        {
            Move = moveAction.ReadValue<Vector2>();
            FireHeld = fireAction.IsPressed();
        }

        private void OnFireStarted(InputAction.CallbackContext context)
        {
            FirePressed?.Invoke();
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            FireReleased?.Invoke();
        }
    }
}
