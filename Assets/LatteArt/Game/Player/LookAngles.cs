using UnityEngine;

namespace LatteArt.Player
{
    public sealed class LookAngles
    {
        private const float FullTurn = 360f;

        private readonly float _minPitch;
        private readonly float _maxPitch;

        public LookAngles(float yaw, float pitch, float minPitch, float maxPitch)
        {
            _minPitch = minPitch;
            _maxPitch = maxPitch;
            Yaw = Mathf.Repeat(yaw, FullTurn);
            Pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public void Rotate(Vector2 lookDegrees)
        {
            Yaw = Mathf.Repeat(Yaw + lookDegrees.x, FullTurn);
            Pitch = Mathf.Clamp(Pitch - lookDegrees.y, _minPitch, _maxPitch);
        }
    }
}
