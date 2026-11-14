using UnityEngine;

namespace LatteArt.Fluid
{
    public interface IFluidAppearance
    {
        RenderTexture Dye { get; }
        RenderTexture FoamCoords { get; }
        float FoamPhase { get; }
        float CoffeeDilution { get; }
        float CupRadius { get; }
        float TexelSize { get; }
    }
}
