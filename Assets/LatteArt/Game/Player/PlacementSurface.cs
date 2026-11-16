using System;
using UnityEngine;

namespace LatteArt.Player
{
    [DisallowMultipleComponent]
    public sealed class PlacementSurface : MonoBehaviour
    {
        public event Action<Carryable> CarryablePlaced;

        public void NotifyPlaced(Carryable carryable)
        {
            CarryablePlaced?.Invoke(carryable);
        }
    }
}
