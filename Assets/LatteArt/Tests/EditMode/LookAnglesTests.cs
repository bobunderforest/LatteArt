using LatteArt.Player;
using NUnit.Framework;
using UnityEngine;

namespace LatteArt.Tests.EditMode
{
    public sealed class LookAnglesTests
    {
        private const float Tolerance = 1e-4f;
        private const float MinPitch = -80f;
        private const float MaxPitch = 80f;

        [Test]
        public void Rotate_HorizontalLook_AddsToYaw()
        {
            var look = new LookAngles(10f, 0f, MinPitch, MaxPitch);

            look.Rotate(new Vector2(30f, 0f));

            Assert.That(look.Yaw, Is.EqualTo(40f).Within(Tolerance));
        }

        [Test]
        public void Rotate_PastFullTurn_WrapsYaw()
        {
            var look = new LookAngles(350f, 0f, MinPitch, MaxPitch);

            look.Rotate(new Vector2(20f, 0f));

            Assert.That(look.Yaw, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void Rotate_LookUp_DecreasesPitch()
        {
            var look = new LookAngles(0f, 0f, MinPitch, MaxPitch);

            look.Rotate(new Vector2(0f, 15f));

            Assert.That(look.Pitch, Is.EqualTo(-15f).Within(Tolerance));
        }

        [Test]
        public void Rotate_BeyondPitchLimits_ClampsPitch()
        {
            var look = new LookAngles(0f, 0f, MinPitch, MaxPitch);

            look.Rotate(new Vector2(0f, 200f));
            Assert.That(look.Pitch, Is.EqualTo(MinPitch).Within(Tolerance));

            look.Rotate(new Vector2(0f, -400f));
            Assert.That(look.Pitch, Is.EqualTo(MaxPitch).Within(Tolerance));
        }

        [Test]
        public void Constructor_PitchOutsideLimits_ClampsPitch()
        {
            var look = new LookAngles(0f, 120f, MinPitch, MaxPitch);

            Assert.That(look.Pitch, Is.EqualTo(MaxPitch).Within(Tolerance));
        }
    }
}
