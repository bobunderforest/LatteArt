using LatteArt.Fluid;
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

        private PourStream _stream;
        private Vector2 _previousPointerUv;
        private bool _hasPreviousPointerUv;

        private void Awake()
        {
            if (!HasValidSetup())
            {
                enabled = false;
                return;
            }

            _stream = new PourStream(
                settings.StreamRadius,
                settings.MinStreamRadius,
                settings.MaxStreamRadius,
                settings.ResizeFactorPerScroll);
        }

        private void OnEnable()
        {
            pointAction.action.Enable();
            pourAction.action.Enable();
            resizeStreamAction.action.Enable();
        }

        private void OnDisable()
        {
            pointAction.action.Disable();
            pourAction.action.Disable();
            resizeStreamAction.action.Disable();
            _hasPreviousPointerUv = false;
            surface.HideBrush();
            if (surface.Simulation != null) surface.Simulation.StopInflow();
        }

        private void Update()
        {
            var simulation = surface.Simulation;
            if (simulation == null) return;

            _stream.Resize(resizeStreamAction.action.ReadValue<float>());

            var pointerRay = viewCamera.ScreenPointToRay(pointAction.action.ReadValue<Vector2>());
            if (!surface.TryGetSurfaceUv(pointerRay, out var pointerUv))
            {
                _hasPreviousPointerUv = false;
                surface.HideBrush();
                simulation.StopInflow();
                return;
            }

            var deltaTime = Time.deltaTime;
            var pointerVelocity = _hasPreviousPointerUv && deltaTime > 0f
                ? (pointerUv - _previousPointerUv) / deltaTime
                : Vector2.zero;
            _previousPointerUv = pointerUv;
            _hasPreviousPointerUv = true;

            surface.ShowBrush(pointerUv, _stream.Radius);

            if (!pourAction.action.IsPressed())
            {
                simulation.StopInflow();
                return;
            }

            Pour(simulation, pointerUv, pointerVelocity, deltaTime);
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
            var radius = _stream.Radius;
            var dragImpulse = pitcherVelocity * (settings.DragStrength * deltaTime);

            simulation.SetInflow(uv, radius, settings.InflowRate);
            simulation.InjectMilk(uv, radius);
            simulation.AddForce(uv, dragImpulse, radius);
        }
    }
}
