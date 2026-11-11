using UnityEngine;

namespace LatteArt.Tools
{
    public sealed class ToolRadius
    {
        private readonly float _minRadius;
        private readonly float _maxRadius;
        private readonly float _resizeFactor;

        public ToolRadius(float initialRadius, float minRadius, float maxRadius, float resizeFactor)
        {
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            _resizeFactor = resizeFactor;
            Radius = Mathf.Clamp(initialRadius, minRadius, maxRadius);
        }

        public float Radius { get; private set; }

        public void Resize(float scrollDelta)
        {
            if (Mathf.Approximately(scrollDelta, 0f)) return;

            var factor = scrollDelta > 0f ? _resizeFactor : 1f / _resizeFactor;
            Radius = Mathf.Clamp(Radius * factor, _minRadius, _maxRadius);
        }
    }
}
