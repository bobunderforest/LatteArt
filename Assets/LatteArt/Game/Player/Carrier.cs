using UnityEngine;

namespace LatteArt.Player
{
    [RequireComponent(typeof(Interactor))]
    public sealed class Carrier : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private Transform holdAnchor;

        private Carryable _carried;
        private bool _hasValidSetup;
        private Interactor _interactor;
        private PlacementPreview _preview;

        private void Awake()
        {
            _interactor = GetComponent<Interactor>();
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _preview = new PlacementPreview(settings.PlacePreviewMaterial);
        }

        private void Update()
        {
            if (_carried == null || !TryFindPlacement(out var position, out var rotation, out _)) return;

            _preview.Draw(position, rotation);
        }

        public bool IsCarrying(Carryable carryable)
        {
            return _carried != null && _carried == carryable;
        }

        public void PickUp(Carryable carryable)
        {
            if (!_hasValidSetup || _carried != null) return;

            _carried = carryable;
            _preview.Capture(carryable.transform);
            carryable.AttachTo(holdAnchor);
            _interactor.HoldTarget(carryable);
        }

        public void TryPlace()
        {
            if (_carried == null) return;
            if (!TryFindPlacement(out var position, out var rotation, out var surfaceCollider)) return;

            var placed = _carried;
            placed.PlaceAt(position, rotation);
            _carried = null;
            _preview.Clear();
            _interactor.ReleaseTarget();

            var surface = surfaceCollider.GetComponentInParent<PlacementSurface>();
            if (surface != null) surface.NotifyPlaced(placed);
        }

        private bool TryFindPlacement(out Vector3 position, out Quaternion rotation, out Collider surfaceCollider)
        {
            position = default;
            rotation = default;
            surfaceCollider = null;

            var ray = new Ray(rayOrigin.position, rayOrigin.forward);
            var hasHit = Physics.Raycast(
                ray, out var hit, settings.InteractRange, settings.InteractLayers, QueryTriggerInteraction.Ignore);
            if (!hasHit || Vector3.Angle(hit.normal, Vector3.up) > settings.MaxPlaceSlope) return false;

            var offsetFromCarrier = Vector3.ProjectOnPlane(hit.point - transform.position, Vector3.up);
            if (offsetFromCarrier.magnitude < settings.MinPlaceDistance) return false;

            var facing = Vector3.ProjectOnPlane(rayOrigin.forward, Vector3.up);
            position = hit.point;
            surfaceCollider = hit.collider;
            rotation = facing.sqrMagnitude > 0f ? Quaternion.LookRotation(facing) : Quaternion.identity;
            return true;
        }

        private bool HasValidSetup()
        {
            if (settings != null && settings.PlacePreviewMaterial != null && rayOrigin != null && holdAnchor != null)
            {
                return true;
            }

            Debug.LogError("Carrier has unassigned references or no place preview material.", this);
            return false;
        }
    }
}
