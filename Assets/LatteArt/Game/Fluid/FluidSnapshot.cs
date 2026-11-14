using System;
using UnityEngine;

namespace LatteArt.Fluid
{
    public sealed class FluidSnapshot : IFluidAppearance, IDisposable
    {
        public FluidSnapshot(IFluidAppearance source)
        {
            Dye = FluidTexture.Copy(source.Dye, "FluidSnapshotDye");
            FoamCoords = FluidTexture.Copy(source.FoamCoords, "FluidSnapshotFoamCoords");
            FoamPhase = source.FoamPhase;
            CoffeeDilution = source.CoffeeDilution;
            CupRadius = source.CupRadius;
            TexelSize = source.TexelSize;
        }

        public RenderTexture Dye { get; }
        public RenderTexture FoamCoords { get; }
        public float FoamPhase { get; }
        public float CoffeeDilution { get; }
        public float CupRadius { get; }
        public float TexelSize { get; }

        public void Dispose()
        {
            FluidTexture.Release(Dye);
            FluidTexture.Release(FoamCoords);
        }
    }
}
