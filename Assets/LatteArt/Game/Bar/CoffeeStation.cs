using LatteArt.Player;
using LatteArt.Tools;
using LatteArt.View;
using UnityEngine;

namespace LatteArt.Bar
{
    public sealed class CoffeeStation : MonoBehaviour, IInteractable
    {
        [Header("Dependencies")]
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private ToolSelector toolSelector;
        [SerializeField] private FirstPersonController firstPersonController;
        [SerializeField] private Interactor interactor;
        [SerializeField] private OutlineHighlight highlight;

        private bool _hasValidSetup;
        private bool _isInUse;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            SetInUse(false);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        private void OnDisable()
        {
            if (!_isInUse) return;

            _isInUse = false;
            toolSelector.enabled = false;
            interactor.ReleaseTarget();
            firstPersonController.enabled = true;
            cameraRig.BlendToFirstPerson();
        }

        public void SetHovered(bool isHovered)
        {
            if (!_hasValidSetup) return;

            highlight.SetVisible(isHovered && !_isInUse);
        }

        public void Interact()
        {
            if (!_hasValidSetup) return;

            SetInUse(!_isInUse);
        }

        private void SetInUse(bool isInUse)
        {
            _isInUse = isInUse;
            toolSelector.enabled = isInUse;
            firstPersonController.enabled = !isInUse;

            if (isInUse)
            {
                highlight.SetVisible(false);
                interactor.HoldTarget(this);
                cameraRig.BlendToStation();
                return;
            }

            interactor.ReleaseTarget();
            cameraRig.BlendToFirstPerson();
        }

        private bool HasValidSetup()
        {
            if (cameraRig != null && toolSelector != null && firstPersonController != null && interactor != null &&
                highlight != null)
            {
                return true;
            }

            Debug.LogError("CoffeeStation has unassigned references.", this);
            return false;
        }
    }
}
