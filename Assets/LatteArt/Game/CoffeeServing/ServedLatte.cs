using LatteArt.Fluid;
using LatteArt.Player;
using UnityEngine;

namespace LatteArt.CoffeeServing
{
    [RequireComponent(typeof(Carryable))]
    public sealed class ServedLatte : MonoBehaviour
    {
        private Carryable _carryable;
        private FrozenFluidSurface _surface;

        public Carryable Carryable => _carryable;

        private void Awake()
        {
            _carryable = GetComponent<Carryable>();
            _surface = GetComponentInChildren<FrozenFluidSurface>(true);
        }

        public void Initialize(FluidSnapshot snapshot)
        {
            if (_surface == null)
            {
                Debug.LogError("ServedLatte has no FrozenFluidSurface in its children.", this);
                snapshot.Dispose();
                return;
            }

            _surface.Show(snapshot);
        }
    }
}
