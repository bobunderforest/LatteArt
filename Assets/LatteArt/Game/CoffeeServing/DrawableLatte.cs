using LatteArt.Fluid;
using LatteArt.MaterialLibrary.Outline;
using UnityEngine;

namespace LatteArt.CoffeeServing
{
    [RequireComponent(typeof(OutlineHighlight))]
    public sealed class DrawableLatte : MonoBehaviour
    {
        private const float NeverCulledBoundsSize = 100000f;

        private OutlineHighlight _highlight;
        private Renderer[] _renderers;
        private FluidSurface _surface;

        public FluidSurface Surface => _surface;

        private void Awake()
        {
            _highlight = GetComponent<OutlineHighlight>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _surface = GetComponentInChildren<FluidSurface>(true);
            if (_surface == null) Debug.LogError("DrawableLatte has no FluidSurface in its children.", this);
        }

        public void SetHighlighted(bool isHighlighted)
        {
            _highlight.SetVisible(isHighlighted);
        }

        public void BeginWarmUp()
        {
            foreach (var latteRenderer in _renderers)
            {
                latteRenderer.bounds = new Bounds(latteRenderer.bounds.center, Vector3.one * NeverCulledBoundsSize);
            }

            _highlight.SetVisible(true);
            if (_surface != null) _surface.WarmUp();
        }

        public void EndWarmUp()
        {
            _highlight.SetVisible(false);
            foreach (var latteRenderer in _renderers)
            {
                latteRenderer.ResetBounds();
            }
        }
    }
}
