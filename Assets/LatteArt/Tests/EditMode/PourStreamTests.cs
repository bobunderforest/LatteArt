using LatteArt.Pour;
using NUnit.Framework;

namespace LatteArt.Tests.EditMode
{
    public sealed class PourStreamTests
    {
        private const float Tolerance = 1e-5f;
        private const float MinRadius = 0.01f;
        private const float MaxRadius = 0.1f;
        private const float ResizeFactor = 2f;

        [Test]
        public void Constructor_InitialRadiusOutsideLimits_ClampsRadius()
        {
            var stream = new PourStream(1f, MinRadius, MaxRadius, ResizeFactor);

            Assert.That(stream.Radius, Is.EqualTo(MaxRadius).Within(Tolerance));
        }

        [Test]
        public void Resize_ScrollUp_MultipliesRadiusByFactor()
        {
            var stream = new PourStream(0.02f, MinRadius, MaxRadius, ResizeFactor);

            stream.Resize(1f);

            Assert.That(stream.Radius, Is.EqualTo(0.04f).Within(Tolerance));
        }

        [Test]
        public void Resize_ScrollDown_DividesRadiusByFactor()
        {
            var stream = new PourStream(0.04f, MinRadius, MaxRadius, ResizeFactor);

            stream.Resize(-1f);

            Assert.That(stream.Radius, Is.EqualTo(0.02f).Within(Tolerance));
        }

        [Test]
        public void Resize_ScrollAmount_UsesOnlyDirection()
        {
            var stream = new PourStream(0.02f, MinRadius, MaxRadius, ResizeFactor);

            stream.Resize(120f);

            Assert.That(stream.Radius, Is.EqualTo(0.04f).Within(Tolerance));
        }

        [Test]
        public void Resize_BeyondLimits_ClampsRadius()
        {
            var stream = new PourStream(0.08f, MinRadius, MaxRadius, ResizeFactor);

            stream.Resize(1f);
            Assert.That(stream.Radius, Is.EqualTo(MaxRadius).Within(Tolerance));

            for (var i = 0; i < 10; i++) stream.Resize(-1f);
            Assert.That(stream.Radius, Is.EqualTo(MinRadius).Within(Tolerance));
        }

        [Test]
        public void Resize_NoScroll_KeepsRadius()
        {
            var stream = new PourStream(0.05f, MinRadius, MaxRadius, ResizeFactor);

            stream.Resize(0f);

            Assert.That(stream.Radius, Is.EqualTo(0.05f).Within(Tolerance));
        }
    }
}
