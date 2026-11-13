using LatteArt.Fluid;
using LatteArt.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.SteamWand
{
    public sealed class SteamWandTool : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private FluidSurface surface;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private SteamWandSettings settings;

        [Header("Input")]
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private InputActionReference blowAction;
        [SerializeField] private InputActionReference resizeAction;

        private bool _hasValidSetup;
        private SurfacePointer _pointer;
        private StreamGrowth _blowGrowth;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _pointer = new SurfacePointer(
                surface, viewCamera, pointAction.action, resizeAction.action, settings.BlowStream.CreateRadius());
            _blowGrowth = settings.BlowStream.CreateGrowth();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        private void OnDisable()
        {
            _blowGrowth?.Stop();
            if (surface == null) return;

            surface.HideBrush();
            if (surface.Simulation != null) surface.Simulation.StopBlow();
        }

        private void Update()
        {
            var simulation = surface.Simulation;
            if (simulation == null) return;

            _pointer.ResizeFromScroll();
            if (!_pointer.TryGetSurfaceUv(out var pointerUv))
            {
                _blowGrowth.Stop();
                _pointer.HideBrush();
                simulation.StopBlow();
                return;
            }

            _pointer.ShowBrush(pointerUv);

            if (blowAction.action.IsPressed()) _blowGrowth.Grow(_pointer.Radius, Time.deltaTime);
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
            if (surface != null && viewCamera != null && settings != null &&
                pointAction != null && blowAction != null && resizeAction != null)
            {
                return true;
            }

            Debug.LogError("SteamWandTool has unassigned references.", this);
            return false;
        }
    }
}
