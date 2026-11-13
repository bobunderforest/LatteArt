using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Tools
{
    public sealed class SurfacePointer
    {
        private readonly FluidSurface _surface;
        private readonly Camera _viewCamera;
        private readonly InputAction _pointAction;
        private readonly InputAction _resizeAction;
        private readonly ToolRadius _radius;

        public SurfacePointer(
            FluidSurface surface, Camera viewCamera, InputAction pointAction, InputAction resizeAction, ToolRadius radius)
        {
            _surface = surface;
            _viewCamera = viewCamera;
            _pointAction = pointAction;
            _resizeAction = resizeAction;
            _radius = radius;
        }

        public float Radius => _radius.Radius;

        public void ResizeFromScroll()
        {
            _radius.Resize(_resizeAction.ReadValue<float>());
        }

        public bool TryGetSurfaceUv(out Vector2 uv)
        {
            var pointerRay = _viewCamera.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
            return _surface.TryGetSurfaceUv(pointerRay, out uv);
        }

        public void ShowBrush(Vector2 uv)
        {
            _surface.ShowBrush(uv, Radius);
        }

        public void HideBrush()
        {
            _surface.HideBrush();
        }
    }
}
