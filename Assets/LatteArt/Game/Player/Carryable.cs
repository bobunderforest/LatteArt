using System;
using LatteArt.MaterialLibrary.Outline;
using UnityEngine;

namespace LatteArt.Player
{
    [RequireComponent(typeof(OutlineHighlight))]
    public sealed class Carryable : MonoBehaviour, IInteractable
    {
        private Collider[] _colliders;
        private bool _hasValidSetup;
        private OutlineHighlight _highlight;

        public event Action PickedUp;
        public event Action Placed;

        private void Awake()
        {
            _highlight = GetComponent<OutlineHighlight>();
            _colliders = GetComponentsInChildren<Collider>();
            _hasValidSetup = HasValidSetup();
        }

        public void SetHovered(bool isHovered)
        {
            if (!_hasValidSetup) return;

            _highlight.SetVisible(isHovered);
        }

        public void Interact(Interactor interactor)
        {
            if (!_hasValidSetup) return;

            var carrier = interactor.GetComponent<Carrier>();
            if (carrier == null)
            {
                Debug.LogError("Carryable needs a Carrier next to the Interactor.", interactor);
                return;
            }

            if (carrier.IsCarrying(this)) carrier.TryPlace();
            else carrier.PickUp(this);
        }

        public void AttachTo(Transform anchor)
        {
            _highlight.SetVisible(false);
            SetCollidersEnabled(false);
            transform.SetParent(anchor, false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            PickedUp?.Invoke();
        }

        public void PlaceAt(Vector3 position, Quaternion rotation)
        {
            transform.SetParent(null);
            transform.SetPositionAndRotation(position, rotation);
            SetCollidersEnabled(true);
            Placed?.Invoke();
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            foreach (var bodyCollider in _colliders)
            {
                bodyCollider.enabled = isEnabled;
            }
        }

        private bool HasValidSetup()
        {
            if (_colliders.Length > 0) return true;

            Debug.LogError("Carryable needs at least one collider.", this);
            return false;
        }
    }
}
