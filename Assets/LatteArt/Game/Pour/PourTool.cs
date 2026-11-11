using LatteArt.Fluid;
using LatteArt.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Pour
{
    public sealed class PourTool : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private FluidSurface surface;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private PourSettings settings;

        [Header("Input")]
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference pourAction;
        [SerializeField] private InputActionReference resizeStreamAction;

        private readonly PointerMotion _pointerMotion = new();
        private bool _hasValidSetup;
        private ToolRadius _streamRadius;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _streamRadius = new ToolRadius(
                settings.StreamRadius,
                settings.MinStreamRadius,
                settings.MaxStreamRadius,
                settings.ResizeFactorPerScroll);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        private void OnDisable()
        {
            _pointerMotion.Reset();
            if (surface == null) return;

            surface.HideBrush();
            if (surface.Simulation != null) surface.Simulation.StopInflow();
        }

        private void Update()
        {
            var simulation = surface.Simulation;
            if (simulation == null) return;

            _streamRadius.Resize(resizeStreamAction.action.ReadValue<float>());

            var pointerRay = viewCamera.ScreenPointToRay(pointAction.action.ReadValue<Vector2>());
            if (!surface.TryGetSurfaceUv(pointerRay, out var pointerUv))
            {
                _pointerMotion.Reset();
                surface.HideBrush();
                simulation.StopInflow();
                return;
            }

            var deltaTime = Time.deltaTime;
            _pointerMotion.MoveTo(pointerUv, deltaTime);
            surface.ShowBrush(pointerUv, _streamRadius.Radius);

            if (!pourAction.action.IsPressed())
            {
                simulation.StopInflow();
                return;
            }

            Pour(simulation, pointerUv, _pointerMotion.Velocity, deltaTime);
        }

        private bool HasValidSetup()
        {
            if (surface != null && viewCamera != null && settings != null &&
                pointAction != null && pourAction != null && resizeStreamAction != null)
            {
                return true;
            }

            Debug.LogError("PourTool has unassigned references.", this);
            return false;
        }

        private void Pour(FluidSimulation simulation, Vector2 uv, Vector2 pitcherVelocity, float deltaTime)
        {
            var radius = _streamRadius.Radius;
            var dragImpulse = pitcherVelocity * (settings.DragStrength * deltaTime);

            simulation.SetInflow(uv, radius, settings.InflowRate);
            simulation.InjectMilk(uv, radius);
            simulation.AddForce(uv, dragImpulse, radius);
        }
    }
}
