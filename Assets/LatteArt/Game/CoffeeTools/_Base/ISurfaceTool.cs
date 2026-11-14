using LatteArt.Fluid;
using UnityEngine;

namespace LatteArt.CoffeeTools
{
    public interface ISurfaceTool
    {
        void Attach(FluidSurface surface, Camera viewCamera);
        void Detach();
    }
}
