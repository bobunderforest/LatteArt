using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Bar
{
    public sealed class CupReset : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CoffeeStation station;
        [SerializeField] private FluidSurface surface;

        [Header("Input")]
        [SerializeField] private InputActionReference resetAction;

        private void Awake()
        {
            if (!HasValidSetup()) enabled = false;
        }

        private void Update()
        {
            if (!station.IsInUse || !resetAction.action.WasPressedThisFrame()) return;

            surface.ResetSimulation();
        }

        private bool HasValidSetup()
        {
            if (station != null && surface != null && resetAction != null) return true;

            Debug.LogError("CupReset has unassigned references.", this);
            return false;
        }
    }
}
