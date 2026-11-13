using LatteArt.Tools;
using NUnit.Framework;
using UnityEngine;

namespace LatteArt.Tests.EditMode
{
    public sealed class StreamGrowthTests
    {
        private const float Tolerance = 1e-4f;
        private const float StartRadius = 0.005f;
        private const float GrowSpeed = 0.2f;
        private const float EaseTime = 0.05f;
        private const float TaperTime = 1f;
        private const float ShortTaperTime = 0.2f;
        private const float TargetRadius = 0.1f;
        private const float FrameTime = 0.01f;

        [Test]
        public void Grow_FirstFrame_StartsAtStartRadius()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);

            growth.Grow(TargetRadius, FrameTime);

            Assert.That(growth.Radius, Is.EqualTo(StartRadius).Within(Tolerance));
        }

        [Test]
        public void Grow_TargetBelowStartRadius_StartsAtTarget()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);

            growth.Grow(0.002f, FrameTime);

            Assert.That(growth.Radius, Is.EqualTo(0.002f).Within(Tolerance));
        }

        [Test]
        public void Grow_LaterFrames_NeverExceedGrowSpeed()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);
            growth.Grow(TargetRadius, FrameTime);

            for (var i = 0; i < 30; i++)
            {
                var previousRadius = growth.Radius;
                growth.Grow(TargetRadius, FrameTime);

                Assert.That(growth.Radius - previousRadius, Is.LessThanOrEqualTo(GrowSpeed * FrameTime + Tolerance));
            }
        }

        [Test]
        public void Grow_TravelPlusEaseTime_ReachesTargetRadiusExactly()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);
            growth.Grow(TargetRadius, FrameTime);
            var travelTime = (TargetRadius - StartRadius) / GrowSpeed + EaseTime;
            var frames = Mathf.CeilToInt(travelTime / FrameTime) + 2;

            for (var i = 0; i < frames; i++) growth.Grow(TargetRadius, FrameTime);

            Assert.That(growth.Radius, Is.EqualTo(TargetRadius));
        }

        [Test]
        public void Grow_TargetDropsBelowRadius_ShrinksWithoutJump()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);
            for (var i = 0; i < 200; i++) growth.Grow(TargetRadius, FrameTime);

            growth.Grow(0.05f, FrameTime);

            Assert.That(growth.Radius, Is.LessThan(TargetRadius));
            Assert.That(TargetRadius - growth.Radius, Is.LessThanOrEqualTo(GrowSpeed * FrameTime + Tolerance));
        }

        [Test]
        public void Taper_AfterGrowing_ShrinksAtMostGrowSpeedThenSettlesAtStartRadius()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);
            for (var i = 0; i < 200; i++) growth.Grow(TargetRadius, FrameTime);

            var frames = 0;
            while (growth.IsFlowing && frames < 200)
            {
                var previousRadius = growth.Radius;
                growth.Taper(FrameTime);
                frames++;

                Assert.That(previousRadius - growth.Radius, Is.LessThanOrEqualTo(GrowSpeed * FrameTime + Tolerance));
            }

            Assert.That(growth.IsFlowing, Is.False);
            Assert.That(growth.Radius, Is.EqualTo(StartRadius));
        }

        [Test]
        public void Taper_LargeRadius_FinishesWithinTaperTime()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, ShortTaperTime);
            for (var i = 0; i < 200; i++) growth.Grow(TargetRadius, FrameTime);

            var frames = 0;
            while (growth.IsFlowing && frames < 200)
            {
                growth.Taper(FrameTime);
                frames++;
            }

            Assert.That(growth.IsFlowing, Is.False);
            Assert.That(frames * FrameTime, Is.LessThanOrEqualTo(ShortTaperTime + 2f * FrameTime));
        }

        [Test]
        public void Grow_DuringTaper_ContinuesFromCurrentRadius()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);
            for (var i = 0; i < 200; i++) growth.Grow(TargetRadius, FrameTime);
            for (var i = 0; i < 10; i++) growth.Taper(FrameTime);
            var taperedRadius = growth.Radius;

            growth.Grow(TargetRadius, FrameTime);

            Assert.That(growth.IsFlowing, Is.True);
            Assert.That(growth.Radius, Is.GreaterThan(StartRadius));
            Assert.That(growth.Radius, Is.EqualTo(taperedRadius).Within(GrowSpeed * FrameTime + Tolerance));
        }

        [Test]
        public void Taper_NotFlowing_DoesNothing()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);

            growth.Taper(FrameTime);

            Assert.That(growth.IsFlowing, Is.False);
        }

        [Test]
        public void Stop_ThenGrow_RestartsFromStartRadius()
        {
            var growth = new StreamGrowth(StartRadius, GrowSpeed, EaseTime, TaperTime);
            for (var i = 0; i < 200; i++) growth.Grow(TargetRadius, FrameTime);

            growth.Stop();
            growth.Grow(TargetRadius, FrameTime);

            Assert.That(growth.Radius, Is.EqualTo(StartRadius).Within(Tolerance));
        }
    }
}
