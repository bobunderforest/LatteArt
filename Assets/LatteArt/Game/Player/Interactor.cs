using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Player
{
    public sealed class Interactor : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Transform rayOrigin;

        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;

        private bool _hasValidSetup;
        private IInteractable _held;
        private IInteractable _hovered;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            interactAction.action.performed += OnInteractPerformed;
        }

        private void OnDisable()
        {
            _held = null;
            Hover(null);
            if (!_hasValidSetup) return;

            interactAction.action.performed -= OnInteractPerformed;
        }

        public void HoldTarget(IInteractable target)
        {
            Hover(null);
            _held = target;
        }

        public void ReleaseTarget()
        {
            _held = null;
        }

        private void Update()
        {
            if (IsAlive(_held)) return;

            _held = null;
            Hover(FindInteractable());
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            var target = IsAlive(_held) ? _held : _hovered;
            if (IsAlive(target)) target.Interact(this);
        }

        private void Hover(IInteractable interactable)
        {
            if (interactable == _hovered) return;

            if (IsAlive(_hovered)) _hovered.SetHovered(false);
            _hovered = interactable;
            if (IsAlive(_hovered)) _hovered.SetHovered(true);
        }

        private static bool IsAlive(IInteractable interactable)
        {
            return interactable is Object unityObject ? unityObject != null : interactable != null;
        }

        private IInteractable FindInteractable()
        {
            var ray = new Ray(rayOrigin.position, rayOrigin.forward);
            var hasHit = Physics.Raycast(
                ray, out var hit, settings.InteractRange, settings.InteractLayers, QueryTriggerInteraction.Collide);

            return hasHit ? hit.collider.GetComponentInParent<IInteractable>() : null;
        }

        private bool HasValidSetup()
        {
            if (settings != null && rayOrigin != null && interactAction != null) return true;

            Debug.LogError("Interactor has unassigned references.", this);
            return false;
        }
    }
}
