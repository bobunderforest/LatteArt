using UnityEngine;

namespace LatteArt.Fluid
{
    [CreateAssetMenu(fileName = "SO_FluidSettings", menuName = "LatteArt/Fluid Settings")]
    public sealed class FluidSettings : ScriptableObject
    {
        [Header("Grid")]
        [SerializeField, Min(32)] private int resolution = 256;
        [SerializeField, Range(0.1f, 0.49f)] private float cupRadius = 0.48f;

        [Header("Timing")]
        [SerializeField, Min(1)] private int stepsPerSecond = 60;
        [SerializeField, Min(1)] private int maxStepsPerFrame = 4;

        [Header("Solver")]
        [SerializeField, Range(1, 100)] private int pressureIterations = 30;

        [Header("Dynamics")]
        [SerializeField, Min(0f)] private float velocityDamping = 1f;
        [SerializeField, Range(0.02f, 1f)] private float stirSettleTime = 0.3f;

        public int Resolution => resolution;
        public float CupRadius => cupRadius;
        public int StepsPerSecond => stepsPerSecond;
        public int MaxStepsPerFrame => maxStepsPerFrame;
        public int PressureIterations => pressureIterations;
        public float VelocityDamping => velocityDamping;
        public float StirSettleTime => stirSettleTime;
    }
}
