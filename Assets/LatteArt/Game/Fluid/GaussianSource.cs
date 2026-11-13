using UnityEngine;

namespace LatteArt.Fluid
{
    public static class GaussianSource
    {
        private const int SamplesPerAxis = 16;
        private const float ExtentInRadii = 3f;

        public static float MassInsideCircle(Vector2 center, float radius, Vector2 circleCenter, float circleRadius)
        {
            var extent = radius * ExtentInRadii;
            var sampleSpacing = 2f * extent / SamplesPerAxis;
            var circleRadiusSquared = circleRadius * circleRadius;
            var radiusSquared = radius * radius;
            var mass = 0f;

            for (var y = 0; y < SamplesPerAxis; y++)
            {
                for (var x = 0; x < SamplesPerAxis; x++)
                {
                    var offset = new Vector2(
                        (x + 0.5f) * sampleSpacing - extent,
                        (y + 0.5f) * sampleSpacing - extent);
                    var isInsideCircle = (center + offset - circleCenter).sqrMagnitude < circleRadiusSquared;
                    if (!isInsideCircle) continue;

                    mass += Mathf.Exp(-offset.sqrMagnitude / radiusSquared);
                }
            }

            return mass * sampleSpacing * sampleSpacing;
        }
    }
}
