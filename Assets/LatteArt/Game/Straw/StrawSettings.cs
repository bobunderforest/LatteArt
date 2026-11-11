using UnityEngine;

namespace LatteArt.Straw
{
    [CreateAssetMenu(fileName = "SO_StrawSettings", menuName = "LatteArt/Straw Settings")]
    public sealed class StrawSettings : ScriptableObject
    {
        [Header("Stir Size")]
        [SerializeField, Range(0.003f, 0.06f)] private float stirRadius = 0.008f;
        [SerializeField, Range(0.003f, 0.06f)] private float minStirRadius = 0.004f;
        [SerializeField, Range(0.003f, 0.06f)] private float maxStirRadius = 0.04f;
        [SerializeField, Range(1.01f, 2f)] private float resizeFactorPerScroll = 1.15f;

        [Header("Stir")]
        [SerializeField, Range(0f, 1f)] private float fluidFollow = 0.88f;
        [SerializeField, Range(0.005f, 0.2f)] private float grabTime = 0.02f;

        [Header("Motion")]
        [SerializeField, Range(0f, 0.2f)] private float followTime = 0.015f;

        public float StirRadius => stirRadius;
        public float MinStirRadius => minStirRadius;
        public float MaxStirRadius => maxStirRadius;
        public float ResizeFactorPerScroll => resizeFactorPerScroll;
        public float FluidFollow => fluidFollow;
        public float GrabTime => grabTime;
        public float FollowTime => followTime;

        private void OnValidate()
        {
            maxStirRadius = Mathf.Max(maxStirRadius, minStirRadius);
            stirRadius = Mathf.Clamp(stirRadius, minStirRadius, maxStirRadius);
        }
    }
}
