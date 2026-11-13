using LatteArt.View;
using NUnit.Framework;

namespace LatteArt.Tests.EditMode
{
    public sealed class CameraBlendTests
    {
        private const float Tolerance = 1e-4f;
        private const float Duration = 0.5f;

        [Test]
        public void Advance_ForFullDuration_ReachesTarget()
        {
            var blend = new CameraBlend(Duration);

            blend.Advance(Duration, 1f);

            Assert.That(blend.Progress, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Advance_PastTarget_StopsAtTarget()
        {
            var blend = new CameraBlend(Duration);

            blend.Advance(Duration * 4f, 1f);

            Assert.That(blend.Progress, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Advance_TowardFirstPerson_ReturnsToStart()
        {
            var blend = new CameraBlend(Duration);
            blend.SnapTo(1f);

            blend.Advance(Duration, 0f);

            Assert.That(blend.Progress, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void EasedProgress_AtHalfway_MatchesProgress()
        {
            var blend = new CameraBlend(Duration);

            blend.Advance(Duration * 0.5f, 1f);

            Assert.That(blend.EasedProgress, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void EasedProgress_NearStart_LagsBehindProgress()
        {
            var blend = new CameraBlend(Duration);

            blend.Advance(Duration * 0.25f, 1f);

            Assert.That(blend.EasedProgress, Is.LessThan(blend.Progress));
        }

        [Test]
        public void Advance_WithZeroDuration_SnapsToTarget()
        {
            var blend = new CameraBlend(0f);

            blend.Advance(0f, 1f);

            Assert.That(blend.Progress, Is.EqualTo(1f).Within(Tolerance));
        }
    }
}
