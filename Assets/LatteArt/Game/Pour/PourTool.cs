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
        private StreamGrowth _streamGrowth;
        private Vector2 _streamUv;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _streamRadius = new ToolRadius(
                settings.StreamRadius,
                settings.MinStreamRadius,
                settings.MaxStreamRadius,
                settings.ResizeFactorPerScroll);
            _streamGrowth = new StreamGrowth(
                settings.StartRadius,
                settings.GrowSpeed,
                settings.GrowEaseTime,
                settings.TaperTime);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        private void OnDisable()
        {
            _pointerMotion.Reset();
            _streamGrowth?.Stop();
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
                _streamGrowth.Stop();
                surface.HideBrush();
                simulation.StopInflow();
                return;
            }

            var deltaTime = Time.deltaTime;
            _pointerMotion.MoveTo(pointerUv, deltaTime);
            surface.ShowBrush(pointerUv, _streamRadius.Radius);

            var wasFlowing = _streamGrowth.IsFlowing;
            if (pourAction.action.IsPressed()) _streamGrowth.Grow(_streamRadius.Radius, deltaTime);
            else _streamGrowth.Taper(deltaTime);

            if (!_streamGrowth.IsFlowing)
            {
                simulation.StopInflow();
                return;
            }

            var previousStreamUv = wasFlowing ? _streamUv : pointerUv;
            _streamUv = pointerUv;
            Pour(simulation, previousStreamUv, _streamUv, _pointerMotion.Velocity, deltaTime);
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

        private void Pour(
            FluidSimulation simulation, Vector2 fromUv, Vector2 toUv, Vector2 pitcherVelocity, float deltaTime)
        {
            var radius = _streamGrowth.Radius;
            var speedFactor = Mathf.Clamp01(pitcherVelocity.magnitude / settings.FullDragSpeed);
            var dragImpulse = pitcherVelocity * (settings.DragStrength * speedFactor * deltaTime);

            simulation.SetInflow(toUv, radius, settings.InflowRate);
            simulation.InjectMilk(fromUv, toUv, radius);
            simulation.AddForce(fromUv, toUv, dragImpulse, radius);
        }
    }
}
