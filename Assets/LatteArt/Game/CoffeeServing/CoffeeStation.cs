using System;
using System.Collections;
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
        [SerializeField] private InputActionReference serveAction;
        [SerializeField] private InputActionReference resetAction;
        [SerializeField] private InputActionReference leaveAction;

        private GameObject _emptyCup;
        private bool _hasValidSetup;
        private Interactor _interactor;
        private bool _isInUse;
        private bool _isLattePoured;
        private bool _isWarmedUp;
        private DrawableLatte _latte;
        private FirstPersonController _playerController;
        private ToolSelector _toolSelector;

        public event Action CupPlaced;
        public event Action SurfaceReset;
        public event Action<bool> InUseChanged;
        public event Action WarmedUp;

        public bool IsInUse => _isInUse;
        public bool IsWarmedUp => _isWarmedUp;
        public ToolSelector ToolSelector => _toolSelector;
        public InputAction ServeAction => serveAction != null ? serveAction.action : null;
        public InputAction ResetAction => resetAction != null ? resetAction.action : null;
        public InputAction LeaveAction => leaveAction != null ? leaveAction.action : null;
        public bool CanPlaceCup => _hasValidSetup && _emptyCup == null && !_isLattePoured;
        public bool CanPourCoffee => _hasValidSetup && _emptyCup != null && _latte != null;

        private void Awake()
        {
            _toolSelector = GetComponentInChildren<ToolSelector>(true);
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _toolSelector.Deactivate();
        }

        private IEnumerator Start()
        {
            yield return WarmUpLatte();
            _isWarmedUp = true;
            WarmedUp?.Invoke();
        }

        private IEnumerator WarmUpLatte()
        {
            _latte = Instantiate(drawableLattePrefab, cupAnchor);
            _latte.BeginWarmUp();
            yield return null;
            yield return new WaitForEndOfFrame();

            _latte.EndWarmUp();
            if (!_isLattePoured) _latte.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            cameraRig.StationReached += ActivateTools;
        }

        private void OnDisable()
        {
            if (_isInUse) Leave();
            if (cameraRig != null) cameraRig.StationReached -= ActivateTools;
        }

        private void Update()
        {
            if (serveAction.action.WasPressedThisFrame()) TryServe();
            else if (resetAction.action.WasPressedThisFrame()) TryResetSurface();
            else if (leaveAction.action.WasPressedThisFrame()) TryLeave();
        }

        public void SetHovered(bool isHovered)
        {
            if (_isLattePoured) _latte.SetHighlighted(isHovered && !_isInUse);
        }

        public void Interact(Interactor interactor)
        {
            if (!_hasValidSetup || _isInUse || !_isLattePoured) return;

            Enter(interactor);
        }

        public bool TryServe()
        {
            if (!_isInUse) return false;

            Serve();
            return true;
        }

        public bool TryResetSurface()
        {
            if (!_isInUse) return false;

            _latte.Surface.ResetSimulation();
            SurfaceReset?.Invoke();
            return true;
        }

        public bool TryLeave()
        {
            if (!_isInUse) return false;

            Leave();
            return true;
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
            _isLattePoured = true;
            _latte.gameObject.SetActive(true);
            _latte.Surface.ResetSimulation();
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
            _interactor.HoldTarget(this);
            cameraRig.BlendToStation();
            InUseChanged?.Invoke(true);
        }

        private void ActivateTools()
        {
            if (_isInUse) _toolSelector.Activate(_latte.Surface, cameraRig.ViewCamera);
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
            _isLattePoured = false;
            _latte.gameObject.SetActive(false);
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
                servedLattePrefab != null && cameraRig != null && _toolSelector != null &&
                serveAction != null && resetAction != null && leaveAction != null)
            {
                return true;
            }

            Debug.LogError("CoffeeStation has unassigned references.", this);
            return false;
        }
    }
}
