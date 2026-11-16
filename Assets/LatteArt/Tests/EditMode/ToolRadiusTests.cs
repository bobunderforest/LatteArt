using LatteArt.CoffeeTools;
using NUnit.Framework;

namespace LatteArt.Tests.EditMode
{
    public sealed class ToolRadiusTests
    {
        private const float Tolerance = 1e-5f;
        private const float MinRadius = 0.01f;
        private const float MaxRadius = 0.1f;
        private const float ResizeFactor = 2f;

        [Test]
        public void Constructor_InitialRadiusOutsideLimits_ClampsRadius()
        {
            var radius = new ToolRadius(1f, MinRadius, MaxRadius, ResizeFactor);

            Assert.That(radius.Radius, Is.EqualTo(MaxRadius).Within(Tolerance));
        }

        [Test]
        public void Resize_ScrollUp_MultipliesRadiusByFactor()
        {
            var radius = new ToolRadius(0.02f, MinRadius, MaxRadius, ResizeFactor);

            radius.Resize(1f);

            Assert.That(radius.Radius, Is.EqualTo(0.04f).Within(Tolerance));
        }

        [Test]
        public void Resize_ScrollDown_DividesRadiusByFactor()
        {
            var radius = new ToolRadius(0.04f, MinRadius, MaxRadius, ResizeFactor);

            radius.Resize(-1f);

            Assert.That(radius.Radius, Is.EqualTo(0.02f).Within(Tolerance));
        }

        [Test]
        public void Resize_PartialNotches_AddUpToWholeNotch()
        {
            var radius = new ToolRadius(0.02f, MinRadius, MaxRadius, ResizeFactor);

            for (var i = 0; i < 4; i++) radius.Resize(0.25f);

            Assert.That(radius.Radius, Is.EqualTo(0.04f).Within(Tolerance));
        }

        [Test]
        public void Resize_BeyondLimits_ClampsRadius()
        {
            var radius = new ToolRadius(0.08f, MinRadius, MaxRadius, ResizeFactor);

            radius.Resize(1f);
            Assert.That(radius.Radius, Is.EqualTo(MaxRadius).Within(Tolerance));

            for (var i = 0; i < 10; i++) radius.Resize(-1f);
            Assert.That(radius.Radius, Is.EqualTo(MinRadius).Within(Tolerance));
        }

        [Test]
        public void NormalizedRadius_AtLimits_IsZeroAndOne()
        {
            var radius = new ToolRadius(MinRadius, MinRadius, MaxRadius, ResizeFactor);
            Assert.That(radius.NormalizedRadius, Is.EqualTo(0f).Within(Tolerance));

            radius.Resize(10f);
            Assert.That(radius.NormalizedRadius, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void NormalizedRadius_Half_IsGeometricMeanOfLimits()
        {
            var radius = new ToolRadius(0.05f, MinRadius, MaxRadius, ResizeFactor);

            radius.NormalizedRadius = 0.5f;

            Assert.That(radius.Radius, Is.EqualTo(UnityEngine.Mathf.Sqrt(MinRadius * MaxRadius)).Within(Tolerance));
        }

        [Test]
        public void NormalizedRadius_OutOfRange_ClampsToLimits()
        {
            var radius = new ToolRadius(0.05f, MinRadius, MaxRadius, ResizeFactor);

            radius.NormalizedRadius = 2f;

            Assert.That(radius.Radius, Is.EqualTo(MaxRadius).Within(Tolerance));
        }

        [Test]
        public void Resize_NoScroll_KeepsRadius()
        {
            var radius = new ToolRadius(0.05f, MinRadius, MaxRadius, ResizeFactor);

            radius.Resize(0f);

            Assert.That(radius.Radius, Is.EqualTo(0.05f).Within(Tolerance));
        }
    }
}
