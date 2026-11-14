using LatteArt.Fluid;
using LatteArt.MaterialLibrary.Outline;
using UnityEngine;

namespace LatteArt.CoffeeServing
{
    [RequireComponent(typeof(OutlineHighlight))]
    public sealed class DrawableLatte : MonoBehaviour
    {
        private OutlineHighlight _highlight;
        private FluidSurface _surface;

        public FluidSurface Surface => _surface;

        private void Awake()
        {
            _highlight = GetComponent<OutlineHighlight>();
            _surface = GetComponentInChildren<FluidSurface>(true);
            if (_surface == null) Debug.LogError("DrawableLatte has no FluidSurface in its children.", this);
        }

        public void SetHighlighted(bool isHighlighted)
        {
            _highlight.SetVisible(isHighlighted);
        }
    }
}
