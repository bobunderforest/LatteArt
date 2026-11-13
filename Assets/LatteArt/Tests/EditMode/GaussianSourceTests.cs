using LatteArt.Fluid;
using NUnit.Framework;
using UnityEngine;

namespace LatteArt.Tests.EditMode
{
    public sealed class GaussianSourceTests
    {
        private const float SourceRadius = 0.02f;
        private const float CircleRadius = 0.48f;
        private const float RelativeTolerance = 0.01f;
        private const float EdgeCurvatureTolerance = 0.05f;

        private static readonly Vector2 CircleCenter = new(0.5f, 0.5f);
        private static readonly float FullMass = Mathf.PI * SourceRadius * SourceRadius;

        [Test]
        public void MassInsideCircle_SourceWellInside_ReturnsFullGaussianMass()
        {
            var mass = GaussianSource.MassInsideCircle(CircleCenter, SourceRadius, CircleCenter, CircleRadius);

            Assert.That(mass, Is.EqualTo(FullMass).Within(FullMass * RelativeTolerance));
        }

        [Test]
        public void MassInsideCircle_SourceOnEdge_ReturnsAboutHalfMass()
        {
            var center = CircleCenter + new Vector2(CircleRadius, 0f);

            var mass = GaussianSource.MassInsideCircle(center, SourceRadius, CircleCenter, CircleRadius);

            Assert.That(mass, Is.EqualTo(FullMass * 0.5f).Within(FullMass * 0.5f * EdgeCurvatureTolerance));
        }

        [Test]
        public void MassInsideCircle_SourceFarOutside_ReturnsZero()
        {
            var center = CircleCenter + new Vector2(CircleRadius + 5f * SourceRadius, 0f);

            var mass = GaussianSource.MassInsideCircle(center, SourceRadius, CircleCenter, CircleRadius);

            Assert.That(mass, Is.EqualTo(0f));
        }
    }
}
