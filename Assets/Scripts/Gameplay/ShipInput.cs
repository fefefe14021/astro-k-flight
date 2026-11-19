
using System;
using KANYE.UI;
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

        // Новые события для обучения
        public event Action OnFirstShot;
        public event Action OnFirstMove;

        private bool _firstShotFired = false;
        private bool _firstMoveDetected = false;

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
            if (MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
            {
                Move = Vector2.zero;
                FireHeld = false;
                return;
            }

            var move = moveAction.ReadValue<Vector2>();
            if (move.sqrMagnitude < 0.01f && TouchFlightControls.TouchMove.sqrMagnitude > 0.01f)
                move = TouchFlightControls.TouchMove;
            Move = move;

            FireHeld = fireAction.IsPressed() || TouchFlightControls.TouchFireHeld;

            // Проверяем первый выстрел (только один раз)
            if (!_firstShotFired && FireHeld)
            {
                _firstShotFired = true;
                OnFirstShot?.Invoke();
                OnFirstShot = null; // Отписываемся, чтобы не вызывать повторно
            }

            // Проверяем первое движение (только один раз)
            if (!_firstMoveDetected && Move != Vector2.zero)
            {
                _firstMoveDetected = true;
                OnFirstMove?.Invoke();
                OnFirstMove = null; // Отписываемся, чтобы не вызывать повторно
            }
        }

        private void OnFireStarted(InputAction.CallbackContext context)
        {
            if (MissionRadioComms.IsCommsActive || Time.timeScale <= 0f)
                return;

            FirePressed?.Invoke();
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            FireReleased?.Invoke();
        }
    }
}
