using LatteArt.Pour;
using NUnit.Framework;
using UnityEngine;

namespace LatteArt.Tests.EditMode
{
    public sealed class PointerMotionTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void MoveTo_FirstSample_StartsAtRest()
        {
            var motion = new PointerMotion();

            motion.MoveTo(new Vector2(0.3f, 0.4f), 0.1f);

            Assert.That(motion.Velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void MoveTo_SecondSample_ComputesVelocityFromLastUv()
        {
            var motion = new PointerMotion();
            motion.MoveTo(new Vector2(0.2f, 0.5f), 0.1f);

            motion.MoveTo(new Vector2(0.3f, 0.5f), 0.1f);

            Assert.That(motion.Velocity.x, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(motion.Velocity.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void MoveTo_ZeroDeltaTime_KeepsVelocityAtZero()
        {
            var motion = new PointerMotion();
            motion.MoveTo(new Vector2(0.2f, 0.5f), 0.1f);

            motion.MoveTo(new Vector2(0.3f, 0.5f), 0f);

            Assert.That(motion.Velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Reset_NextSample_StartsAtRestFromNewUv()
        {
            var motion = new PointerMotion();
            motion.MoveTo(new Vector2(0.2f, 0.5f), 0.1f);
            motion.MoveTo(new Vector2(0.3f, 0.5f), 0.1f);

            motion.Reset();
            motion.MoveTo(new Vector2(0.8f, 0.8f), 0.1f);

            Assert.That(motion.Velocity, Is.EqualTo(Vector2.zero));
        }
    }
}
