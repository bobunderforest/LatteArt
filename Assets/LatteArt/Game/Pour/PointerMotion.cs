using UnityEngine;

namespace LatteArt.Pour
{
    public sealed class PointerMotion
    {
        private bool _isTracking;
        private Vector2 _lastUv;

        public Vector2 Velocity { get; private set; }

        public void MoveTo(Vector2 uv, float deltaTime)
        {
            Velocity = _isTracking && deltaTime > 0f ? (uv - _lastUv) / deltaTime : Vector2.zero;
            _lastUv = uv;
            _isTracking = true;
        }

        public void Reset()
        {
            _isTracking = false;
            Velocity = Vector2.zero;
        }
    }
}
