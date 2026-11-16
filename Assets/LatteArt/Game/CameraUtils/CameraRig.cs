using System;
using UnityEngine;

namespace LatteArt.CameraUtils
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        [Header("Anchors")]
        [SerializeField] private Transform firstPersonAnchor;
        [SerializeField] private Transform stationAnchor;

        [Header("Blend")]
        [SerializeField, Min(0f)] private float blendDuration = 0.8f;

        private CameraBlend _blend;
        private bool _hasValidSetup;
        private bool _isStationArrivalPending;
        private float _target;
        private Camera _viewCamera;

        public event Action StationReached;

        public Camera ViewCamera => _viewCamera;

        private void Awake()
        {
            _viewCamera = GetComponent<Camera>();
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _blend = new CameraBlend(blendDuration);
            _blend.SnapTo(_target);
            ApplyBlend();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        public void BlendToFirstPerson()
        {
            _target = 0f;
            _isStationArrivalPending = false;
        }

        public void BlendToStation()
        {
            _target = 1f;
            _isStationArrivalPending = true;
        }

        private void LateUpdate()
        {
            _blend.Advance(Time.deltaTime, _target);
            ApplyBlend();
            if (_isStationArrivalPending && _blend.Progress >= 1f) ReachStation();
        }

        private void ReachStation()
        {
            _isStationArrivalPending = false;
            StationReached?.Invoke();
        }

        private void ApplyBlend()
        {
            var blendProgress = _blend.EasedProgress;
            transform.SetPositionAndRotation(
                Vector3.Lerp(firstPersonAnchor.position, stationAnchor.position, blendProgress),
                Quaternion.Slerp(firstPersonAnchor.rotation, stationAnchor.rotation, blendProgress));
        }

        private bool HasValidSetup()
        {
            if (firstPersonAnchor != null && stationAnchor != null) return true;

            Debug.LogError("CameraRig has unassigned anchors.", this);
            return false;
        }
    }
}
