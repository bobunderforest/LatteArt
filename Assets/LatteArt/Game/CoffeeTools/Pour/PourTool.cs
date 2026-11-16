using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeTools.Pour
{
    public sealed class PourTool : MonoBehaviour, ISurfaceTool, IToolActivity
    {
        [Header("Dependencies")]
        [SerializeField] private PourSettings settings;

        [Header("Input")]
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference pourAction;
        [SerializeField] private InputActionReference resizeStreamAction;

        private readonly PointerMotion _pointerMotion = new();
        private bool _hasValidSetup;
        private FluidSurface _surface;
        private Camera _viewCamera;
        private SurfacePointer _pointer;
        private StreamGrowth _streamGrowth;
        private Vector2 _streamUv;

        public bool IsActive => _streamGrowth != null && _streamGrowth.IsFlowing;
        public float Intensity => IsActive ? Mathf.Clamp01(_streamGrowth.Radius / settings.Stream.MaxRadius) : 0f;

        public float NormalizedRadius
        {
            get => _pointer != null ? _pointer.NormalizedRadius : 0f;
            set
            {
                if (_pointer != null) _pointer.NormalizedRadius = value;
            }
        }

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _pointer = new SurfacePointer(
                pointAction.action, pourAction.action, resizeStreamAction.action, settings.Stream.CreateRadius());
            _streamGrowth = settings.Stream.CreateGrowth();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup || _surface == null) enabled = false;
        }

        private void OnDisable()
        {
            _pointerMotion.Reset();
            _streamGrowth?.Stop();
            if (_surface == null) return;

            _surface.HideBrush();
            if (_surface.Simulation != null) _surface.Simulation.StopInflow();
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

        public void SetPointerBlocked(bool isBlocked)
        {
            if (_pointer != null) _pointer.IsBlocked = isBlocked;
        }

        private void Update()
        {
            var simulation = _surface.Simulation;
            if (simulation == null) return;

            _pointer.ReadInput();
            if (!_pointer.TryGetSurfaceUv(_surface, _viewCamera, out var pointerUv))
            {
                _pointerMotion.Reset();
                _streamGrowth.Stop();
                _surface.HideBrush();
                simulation.StopInflow();
                return;
            }

            var deltaTime = Time.deltaTime;
            _pointerMotion.MoveTo(pointerUv, deltaTime);
            _surface.ShowBrush(pointerUv, _pointer.Radius);

            var wasFlowing = _streamGrowth.IsFlowing;
            if (_pointer.IsPressed) _streamGrowth.Grow(_pointer.Radius, deltaTime);
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
            if (settings != null &&
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
