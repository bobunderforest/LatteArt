using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeTools.SteamWand
{
    public sealed class SteamWandTool : MonoBehaviour, ISurfaceTool, IToolActivity
    {
        [Header("Dependencies")]
        [SerializeField] private SteamWandSettings settings;

        [Header("Input")]
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference blowAction;
        [SerializeField] private InputActionReference resizeAction;

        private bool _hasValidSetup;
        private FluidSurface _surface;
        private Camera _viewCamera;
        private SurfacePointer _pointer;
        private StreamGrowth _blowGrowth;

        public bool IsActive => _blowGrowth != null && _blowGrowth.IsFlowing;
        public float Intensity => IsActive ? Mathf.Clamp01(_blowGrowth.Radius / settings.BlowStream.MaxRadius) : 0f;

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
                pointAction.action, blowAction.action, resizeAction.action, settings.BlowStream.CreateRadius());
            _blowGrowth = settings.BlowStream.CreateGrowth();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup || _surface == null) enabled = false;
        }

        private void OnDisable()
        {
            _blowGrowth?.Stop();
            if (_surface == null) return;

            _surface.HideBrush();
            if (_surface.Simulation != null) _surface.Simulation.StopBlow();
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
                _blowGrowth.Stop();
                _surface.HideBrush();
                simulation.StopBlow();
                return;
            }

            _surface.ShowBrush(pointerUv, _pointer.Radius);

            if (_pointer.IsPressed) _blowGrowth.Grow(_pointer.Radius, Time.deltaTime);
            else _blowGrowth.Taper(Time.deltaTime);

            if (!_blowGrowth.IsFlowing)
            {
                simulation.StopBlow();
                return;
            }

            simulation.SetBlow(pointerUv, _blowGrowth.Radius, settings.BlowRate);
        }

        private bool HasValidSetup()
        {
            if (settings != null &&
                pointAction != null && blowAction != null && resizeAction != null)
            {
                return true;
            }

            Debug.LogError("SteamWandTool has unassigned references.", this);
            return false;
        }
    }
}
