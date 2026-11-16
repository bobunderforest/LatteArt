using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeTools
{
    public sealed class SurfacePointer
    {
        private readonly InputAction _pointAction;
        private readonly InputAction _pressAction;
        private readonly InputAction _resizeAction;
        private readonly ToolRadius _radius;
        private bool _isPressBlocked;

        public SurfacePointer(InputAction pointAction, InputAction pressAction, InputAction resizeAction,
            ToolRadius radius)
        {
            _pointAction = pointAction;
            _pressAction = pressAction;
            _resizeAction = resizeAction;
            _radius = radius;
        }

        public bool IsBlocked { get; set; }
        public bool IsPressed => _pressAction.IsPressed() && !_isPressBlocked;
        public float Radius => _radius.Radius;

        public float NormalizedRadius
        {
            get => _radius.NormalizedRadius;
            set => _radius.NormalizedRadius = value;
        }

        public void ReadInput()
        {
            if (_pressAction.WasPressedThisFrame()) _isPressBlocked = IsBlocked;
            if (!IsBlocked) _radius.Resize(_resizeAction.ReadValue<float>());
        }

        public bool TryGetSurfaceUv(FluidSurface surface, Camera viewCamera, out Vector2 uv)
        {
            uv = default;
            if (IsBlocked) return false;

            var pointerRay = viewCamera.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
            return surface.TryGetSurfaceUv(pointerRay, out uv);
        }
    }
}
