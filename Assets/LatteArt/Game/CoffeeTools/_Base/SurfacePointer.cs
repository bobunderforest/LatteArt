using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeTools
{
    public sealed class SurfacePointer
    {
        private readonly InputAction _pointAction;
        private readonly InputAction _resizeAction;
        private readonly ToolRadius _radius;

        public SurfacePointer(InputAction pointAction, InputAction resizeAction, ToolRadius radius)
        {
            _pointAction = pointAction;
            _resizeAction = resizeAction;
            _radius = radius;
        }

        public float Radius => _radius.Radius;

        public void ResizeFromScroll()
        {
            _radius.Resize(_resizeAction.ReadValue<float>());
        }

        public bool TryGetSurfaceUv(FluidSurface surface, Camera viewCamera, out Vector2 uv)
        {
            var pointerRay = viewCamera.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
            return surface.TryGetSurfaceUv(pointerRay, out uv);
        }
    }
}
