using UnityEngine;

namespace LatteArt.Pour
{
    public sealed class StreamGrowth
    {
        private readonly float _startRadius;
        private readonly float _growSpeed;
        private readonly float _easeTime;
        private readonly float _taperTravelTime;
        private float _growVelocity;
        private float _taperSpeed;
        private bool _isTapering;

        public StreamGrowth(float startRadius, float growSpeed, float easeTime, float taperTime)
        {
            _startRadius = startRadius;
            _growSpeed = growSpeed;
            _easeTime = Mathf.Max(easeTime, Mathf.Epsilon);
            _taperTravelTime = Mathf.Max(taperTime - _easeTime, Mathf.Epsilon);
        }

        public float Radius { get; private set; }
        public bool IsFlowing { get; private set; }

        public void Grow(float targetRadius, float deltaTime)
        {
            if (!IsFlowing)
            {
                Radius = Mathf.Min(_startRadius, targetRadius);
                _growVelocity = 0f;
                IsFlowing = true;
                return;
            }

            _isTapering = false;
            MoveRadiusTowards(targetRadius, _growSpeed, deltaTime);
        }

        public void Taper(float deltaTime)
        {
            if (!IsFlowing) return;

            if (!_isTapering)
            {
                _taperSpeed = Mathf.Max(_growSpeed, (Radius - _startRadius) / _taperTravelTime);
                _isTapering = true;
            }

            if (Radius > _startRadius) MoveRadiusTowards(_startRadius, _taperSpeed, deltaTime);
            if (Radius <= _startRadius) Stop();
        }

        public void Stop()
        {
            IsFlowing = false;
            _isTapering = false;
        }

        private void MoveRadiusTowards(float targetRadius, float maxSpeed, float deltaTime)
        {
            if (deltaTime <= 0f) return;

            var acceleration = Mathf.Max(maxSpeed, Mathf.Abs(_growVelocity)) / _easeTime;
            var offset = targetRadius - Radius;
            var brakingSpeed = Mathf.Sqrt(2f * acceleration * Mathf.Abs(offset));
            var desiredVelocity = Mathf.Sign(offset) * Mathf.Min(maxSpeed, brakingSpeed);
            _growVelocity = Mathf.MoveTowards(_growVelocity, desiredVelocity, acceleration * deltaTime);

            var step = _growVelocity * deltaTime;
            var reachesTarget = step * offset >= 0f && Mathf.Abs(step) >= Mathf.Abs(offset);
            if (reachesTarget)
            {
                Radius = targetRadius;
                _growVelocity = 0f;
                return;
            }

            Radius += step;
        }
    }
}
