using UnityEngine;

namespace LatteArt.Fluid
{
    internal static class FluidSurfaceMaterial
    {
        private static readonly int DyeTextureId = Shader.PropertyToID("_DyeTex");
        private static readonly int FoamCoordsTextureId = Shader.PropertyToID("_FoamCoordsTex");
        private static readonly int FoamPhaseId = Shader.PropertyToID("_FoamPhase");
        private static readonly int CoffeeDilutionId = Shader.PropertyToID("_CoffeeDilution");
        private static readonly int CupRadiusId = Shader.PropertyToID("_CupRadius");
        private static readonly int SimulationTexelSizeId = Shader.PropertyToID("_SimulationTexelSize");
        private static readonly int BrushCenterId = Shader.PropertyToID("_BrushCenter");
        private static readonly int BrushRadiusId = Shader.PropertyToID("_BrushRadius");

        public static void Apply(
            Renderer surfaceRenderer, MaterialPropertyBlock properties, IFluidAppearance fluid,
            Vector2 brushCenter, float brushRadius)
        {
            surfaceRenderer.GetPropertyBlock(properties);
            properties.SetTexture(DyeTextureId, fluid.Dye);
            properties.SetTexture(FoamCoordsTextureId, fluid.FoamCoords);
            properties.SetFloat(FoamPhaseId, fluid.FoamPhase);
            properties.SetFloat(CoffeeDilutionId, fluid.CoffeeDilution);
            properties.SetFloat(CupRadiusId, fluid.CupRadius);
            properties.SetFloat(SimulationTexelSizeId, fluid.TexelSize);
            properties.SetVector(BrushCenterId, brushCenter);
            properties.SetFloat(BrushRadiusId, brushRadius);
            surfaceRenderer.SetPropertyBlock(properties);
        }
    }
}
