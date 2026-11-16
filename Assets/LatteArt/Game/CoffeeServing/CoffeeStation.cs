using System;
using LatteArt.CameraUtils;
using LatteArt.CoffeeTools;
using LatteArt.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeServing
{
    public sealed class CoffeeStation : MonoBehaviour, IInteractable
    {
        [Header("Cup")]
        [SerializeField] private Transform cupAnchor;
        [SerializeField] private GameObject emptyCupPrefab;
        [SerializeField] private DrawableLatte drawableLattePrefab;
        [SerializeField] private ServedLatte servedLattePrefab;

        [Header("Dependencies")]
        [SerializeField] private CameraRig cameraRig;

        [Header("Input")]
        [SerializeField] private InputActionReference resetAction;

        private GameObject _emptyCup;
        private bool _hasValidSetup;
        private Interactor _interactor;
        private bool _isInUse;
        private DrawableLatte _latte;
        private FirstPersonController _playerController;
        private ToolSelector _toolSelector;

        public event Action CupPlaced;
        public event Action SurfaceReset;
        public event Action<bool> InUseChanged;

        public bool IsInUse => _isInUse;
        public bool CanPlaceCup => _hasValidSetup && _emptyCup == null && _latte == null;
        public bool CanPourCoffee => _hasValidSetup && _emptyCup != null;

        private void Awake()
        {
            _toolSelector = GetComponentInChildren<ToolSelector>(true);
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _toolSelector.Deactivate();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        private void OnDisable()
        {
            if (_isInUse) Leave();
        }

        private void Update()
        {
            if (!_isInUse || !resetAction.action.WasPressedThisFrame()) return;

            _latte.Surface.ResetSimulation();
            SurfaceReset?.Invoke();
        }

        public void SetHovered(bool isHovered)
        {
            if (_latte != null) _latte.SetHighlighted(isHovered && !_isInUse);
        }

        public void Interact(Interactor interactor)
        {
            if (!_hasValidSetup) return;

            if (_isInUse) Serve();
            else if (_latte != null) Enter(interactor);
        }

        public bool TryPlaceCup()
        {
            if (!CanPlaceCup) return false;

            _emptyCup = Instantiate(emptyCupPrefab, cupAnchor);
            CupPlaced?.Invoke();
            return true;
        }

        public bool TryPourCoffee()
        {
            if (!CanPourCoffee) return false;

            Destroy(_emptyCup);
            _emptyCup = null;
            _latte = Instantiate(drawableLattePrefab, cupAnchor);
            return true;
        }

        private void Enter(Interactor interactor)
        {
            var playerController = interactor.GetComponent<FirstPersonController>();
            if (playerController == null)
            {
                Debug.LogError("CoffeeStation needs a FirstPersonController next to the Interactor.", interactor);
                return;
            }

            _isInUse = true;
            _interactor = interactor;
            _playerController = playerController;

            _playerController.enabled = false;
            _latte.SetHighlighted(false);
            _toolSelector.Activate(_latte.Surface, cameraRig.ViewCamera);
            _interactor.HoldTarget(this);
            cameraRig.BlendToStation();
            InUseChanged?.Invoke(true);
        }

        private void Leave()
        {
            _isInUse = false;
            if (_toolSelector != null) _toolSelector.Deactivate();
            if (_interactor != null) _interactor.ReleaseTarget();
            if (_playerController != null) _playerController.enabled = true;
            if (cameraRig != null) cameraRig.BlendToFirstPerson();

            _interactor = null;
            _playerController = null;
            InUseChanged?.Invoke(false);
        }

        private void Serve()
        {
            var interactor = _interactor;
            Leave();

            var snapshot = _latte.Surface.TakeSnapshot();
            Destroy(_latte.gameObject);
            _latte = null;
            if (snapshot == null) return;

            var servedLatte = Instantiate(servedLattePrefab, cupAnchor.position, cupAnchor.rotation);
            servedLatte.Initialize(snapshot);
            HandOver(servedLatte, interactor);
        }

        private static void HandOver(ServedLatte servedLatte, Interactor interactor)
        {
            var carrier = interactor.GetComponent<Carrier>();
            if (carrier == null)
            {
                Debug.LogError("CoffeeStation needs a Carrier next to the Interactor.", interactor);
                return;
            }

            carrier.PickUp(servedLatte.Carryable);
        }

        private bool HasValidSetup()
        {
            if (cupAnchor != null && emptyCupPrefab != null && drawableLattePrefab != null &&
                servedLattePrefab != null && cameraRig != null && _toolSelector != null && resetAction != null)
            {
                return true;
            }

            Debug.LogError("CoffeeStation has unassigned references.", this);
            return false;
        }
    }
}
