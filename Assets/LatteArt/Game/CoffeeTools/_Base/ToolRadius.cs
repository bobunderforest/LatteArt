using UnityEngine;

namespace LatteArt.CoffeeTools
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

        public float NormalizedRadius
        {
            get => Mathf.InverseLerp(Mathf.Log(_minRadius), Mathf.Log(_maxRadius), Mathf.Log(Radius));
            set => Radius = Mathf.Exp(Mathf.Lerp(Mathf.Log(_minRadius), Mathf.Log(_maxRadius), Mathf.Clamp01(value)));
        }

        public void Resize(float scrollNotches)
        {
            Radius = Mathf.Clamp(Radius * Mathf.Pow(_resizeFactor, scrollNotches), _minRadius, _maxRadius);
        }
    }
}
