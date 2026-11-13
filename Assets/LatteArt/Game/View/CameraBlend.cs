using UnityEngine;

namespace LatteArt.View
{
    public sealed class CameraBlend
    {
        private readonly float _duration;

        public CameraBlend(float duration)
        {
            _duration = duration;
        }

        public float Progress { get; private set; }
        public float EasedProgress => Mathf.SmoothStep(0f, 1f, Progress);

        public void SnapTo(float target)
        {
            Progress = Mathf.Clamp01(target);
        }

        public void Advance(float deltaTime, float target)
        {
            target = Mathf.Clamp01(target);
            if (_duration <= 0f)
            {
                Progress = target;
                return;
            }

            Progress = Mathf.MoveTowards(Progress, target, deltaTime / _duration);
        }
    }
}
