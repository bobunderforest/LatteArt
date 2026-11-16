using LatteArt.Fluid;
using UnityEngine;

namespace LatteArt.CoffeeTools
{
    public interface ISurfaceTool
    {
        float NormalizedRadius { get; set; }

        void Attach(FluidSurface surface, Camera viewCamera);
        void Detach();
    }
}
