using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeTools.Straw
{
    public sealed class StrawTool : MonoBehaviour, ISurfaceTool
    {
        [Header("Dependencies")]
        [SerializeField] private StrawSettings settings;

        [Header("Input")]
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference dragAction;
        [SerializeField] private InputActionReference resizeAction;

        private bool _hasValidSetup;
        private FluidSurface _surface;
        private Camera _viewCamera;
        private SurfacePointer _pointer;
        private bool _isStroking;
        private Vector2 _tipUv;
        private Vector2 _tipVelocity;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            var stirRadius = new ToolRadius(
                settings.StirRadius,
                settings.MinStirRadius,
                settings.MaxStirRadius,
                settings.ResizeFactorPerScroll);
            _pointer = new SurfacePointer(pointAction.action, resizeAction.action, stirRadius);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup || _surface == null) enabled = false;
        }

        private void OnDisable()
        {
            _isStroking = false;
            if (_surface != null) _surface.HideBrush();
        }

        public void Attach(FluidSurface surface, Camera viewCamera)
        {
            _surface = surface;
            _viewCamera = viewCamera;
        }

        public void Detach()
        {
            _surface = null;
            _viewCamera = null;
        }

        private void Update()
        {
            var simulation = _surface.Simulation;
            if (simulation == null) return;

            _pointer.ResizeFromScroll();
            if (!_pointer.TryGetSurfaceUv(_surface, _viewCamera, out var pointerUv))
            {
                _isStroking = false;
                _surface.HideBrush();
                return;
            }

            if (!dragAction.action.IsPressed())
            {
                _isStroking = false;
                _surface.ShowBrush(pointerUv, _pointer.Radius);
                return;
            }

            if (!_isStroking) BeginStroke(pointerUv);

            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            var previousTipUv = _tipUv;
            _tipUv = Vector2.SmoothDamp(
                _tipUv, pointerUv, ref _tipVelocity, settings.FollowTime, Mathf.Infinity, deltaTime);
            _surface.ShowBrush(_tipUv, _pointer.Radius);

            var fluidVelocity = _tipVelocity * settings.FluidFollow;
            var coupling = 1f - Mathf.Exp(-deltaTime / settings.GrabTime);
            simulation.Stir(previousTipUv, _tipUv, fluidVelocity, _pointer.Radius, coupling);
        }

        private void BeginStroke(Vector2 pointerUv)
        {
            _tipUv = pointerUv;
            _tipVelocity = Vector2.zero;
            _isStroking = true;
        }

        private bool HasValidSetup()
        {
            if (settings != null &&
                pointAction != null && dragAction != null && resizeAction != null)
            {
                return true;
            }

            Debug.LogError("StrawTool has unassigned references.", this);
            return false;
        }
    }
}
