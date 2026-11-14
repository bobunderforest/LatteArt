using UnityEngine;

namespace LatteArt.Fluid
{
    [RequireComponent(typeof(Renderer))]
    public sealed class FrozenFluidSurface : MonoBehaviour
    {
        private FluidSnapshot _snapshot;
        private Renderer _surfaceRenderer;

        private void Awake()
        {
            _surfaceRenderer = GetComponent<Renderer>();
        }

        public void Show(FluidSnapshot snapshot)
        {
            _snapshot?.Dispose();
            _snapshot = snapshot;
            FluidSurfaceMaterial.Apply(_surfaceRenderer, new MaterialPropertyBlock(), snapshot, Vector2.zero, 0f);
        }

        private void OnDestroy()
        {
            _snapshot?.Dispose();
        }
    }
}
