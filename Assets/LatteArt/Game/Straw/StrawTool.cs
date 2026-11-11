using LatteArt.Fluid;
using LatteArt.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Straw
{
    public sealed class StrawTool : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private FluidSurface surface;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private StrawSettings settings;

        [Header("Input")]
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference dragAction;
        [SerializeField] private InputActionReference resizeAction;

        private ToolRadius _stirRadius;
        private bool _hasValidSetup;
        private bool _isStroking;
        private Vector2 _tipUv;
        private Vector2 _tipVelocity;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _stirRadius = new ToolRadius(
                settings.StirRadius,
                settings.MinStirRadius,
                settings.MaxStirRadius,
                settings.ResizeFactorPerScroll);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        private void OnDisable()
        {
            _isStroking = false;
            if (surface != null) surface.HideBrush();
        }

        private void Update()
        {
            var simulation = surface.Simulation;
            if (simulation == null) return;

            _stirRadius.Resize(resizeAction.action.ReadValue<float>());

            var pointerRay = viewCamera.ScreenPointToRay(pointAction.action.ReadValue<Vector2>());
            if (!surface.TryGetSurfaceUv(pointerRay, out var pointerUv))
            {
                _isStroking = false;
                surface.HideBrush();
                return;
            }

            if (!dragAction.action.IsPressed())
            {
                _isStroking = false;
                surface.ShowBrush(pointerUv, _stirRadius.Radius);
                return;
            }

            if (!_isStroking) BeginStroke(pointerUv);

            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            var previousTipUv = _tipUv;
            _tipUv = Vector2.SmoothDamp(
                _tipUv, pointerUv, ref _tipVelocity, settings.FollowTime, Mathf.Infinity, deltaTime);
            surface.ShowBrush(_tipUv, _stirRadius.Radius);

            var fluidVelocity = _tipVelocity * settings.FluidFollow;
            var coupling = 1f - Mathf.Exp(-deltaTime / settings.GrabTime);
            simulation.Stir(previousTipUv, _tipUv, fluidVelocity, _stirRadius.Radius, coupling);
        }

        private void BeginStroke(Vector2 pointerUv)
        {
            _tipUv = pointerUv;
            _tipVelocity = Vector2.zero;
            _isStroking = true;
        }

        private bool HasValidSetup()
        {
            if (surface != null && viewCamera != null && settings != null &&
                pointAction != null && dragAction != null && resizeAction != null)
            {
                return true;
            }

            Debug.LogError("StrawTool has unassigned references.", this);
            return false;
        }
    }
}
