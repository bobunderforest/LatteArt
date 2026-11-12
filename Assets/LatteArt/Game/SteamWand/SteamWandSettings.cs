using UnityEngine;

namespace LatteArt.SteamWand
{
    [CreateAssetMenu(fileName = "SO_SteamWandSettings", menuName = "LatteArt/Steam Wand Settings")]
    public sealed class SteamWandSettings : ScriptableObject
    {
        [Header("Blow Size")]
        [SerializeField, Range(0.005f, 0.2f)] private float blowRadius = 0.04f;
        [SerializeField, Range(0.005f, 0.2f)] private float minBlowRadius = 0.01f;
        [SerializeField, Range(0.005f, 0.2f)] private float maxBlowRadius = 0.12f;
        [SerializeField, Range(1.01f, 2f)] private float resizeFactorPerScroll = 1.15f;

        [Header("Blow Growth")]
        [SerializeField, Range(0.001f, 0.05f)] private float startRadius = 0.01f;
        [SerializeField, Range(0.01f, 2f)] private float growSpeed = 0.25f;
        [SerializeField, Range(0.01f, 0.5f)] private float growEaseTime = 0.03f;
        [SerializeField, Range(0.05f, 1f)] private float taperTime = 0.15f;

        [Header("Blow")]
        [SerializeField, Min(0f)] private float blowRate = 8f;

        public float BlowRadius => blowRadius;
        public float MinBlowRadius => minBlowRadius;
        public float MaxBlowRadius => maxBlowRadius;
        public float ResizeFactorPerScroll => resizeFactorPerScroll;
        public float StartRadius => startRadius;
        public float GrowSpeed => growSpeed;
        public float GrowEaseTime => growEaseTime;
        public float TaperTime => taperTime;
        public float BlowRate => blowRate;

        private void OnValidate()
        {
            maxBlowRadius = Mathf.Max(maxBlowRadius, minBlowRadius);
            blowRadius = Mathf.Clamp(blowRadius, minBlowRadius, maxBlowRadius);
        }
    }
}
