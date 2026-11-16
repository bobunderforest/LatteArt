using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        private const float GroundStickSpeed = -1f;

        [Header("Dependencies")]
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Transform cameraPivot;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;

        private CharacterController _characterController;
        private bool _hasValidSetup;
        private LookAngles _look;
        private Vector3 _horizontalVelocity;
        private Vector3 _horizontalAcceleration;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private float _verticalSpeed;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            transform.GetPositionAndRotation(out _spawnPosition, out _spawnRotation);
            var initialPitch = Mathf.DeltaAngle(0f, cameraPivot.localEulerAngles.x);
            _look = new LookAngles(transform.eulerAngles.y, initialPitch, settings.MinPitch, settings.MaxPitch);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ResetMotion();
        }

        private void Update()
        {
            Look();
            Move(Time.deltaTime);
            if (HasFallenOff()) Respawn();
        }

        private void Look()
        {
            var lookDegrees = lookAction.action.ReadValue<Vector2>() * settings.LookDegreesPerPixel;
            _look.Rotate(lookDegrees);

            transform.rotation = Quaternion.Euler(0f, _look.Yaw, 0f);
            cameraPivot.localRotation = Quaternion.Euler(_look.Pitch, 0f, 0f);
        }

        private void Move(float deltaTime)
        {
            var moveInput = moveAction.action.ReadValue<Vector2>();
            var targetVelocity = (transform.right * moveInput.x + transform.forward * moveInput.y) * settings.WalkSpeed;
            _horizontalVelocity = Vector3.SmoothDamp(
                _horizontalVelocity, targetVelocity, ref _horizontalAcceleration, settings.MoveSmoothTime);

            _verticalSpeed = _characterController.isGrounded
                ? GroundStickSpeed
                : _verticalSpeed + Physics.gravity.y * deltaTime;

            _characterController.Move((_horizontalVelocity + Vector3.up * _verticalSpeed) * deltaTime);
        }

        private bool HasFallenOff()
        {
            return transform.position.y < _spawnPosition.y - settings.MaxFallDistance;
        }

        private void Respawn()
        {
            _characterController.enabled = false;
            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            _characterController.enabled = true;

            ResetMotion();
            _look = new LookAngles(_spawnRotation.eulerAngles.y, _look.Pitch, settings.MinPitch, settings.MaxPitch);
        }

        private void ResetMotion()
        {
            _horizontalVelocity = Vector3.zero;
            _horizontalAcceleration = Vector3.zero;
            _verticalSpeed = 0f;
        }

        private bool HasValidSetup()
        {
            if (settings != null && cameraPivot != null && moveAction != null && lookAction != null) return true;

            Debug.LogError("FirstPersonController has unassigned references.", this);
            return false;
        }
    }
}
