using UnityEngine;

namespace LatteArt.Pour
{
    [CreateAssetMenu(fileName = "SO_PourSettings", menuName = "LatteArt/Pour Settings")]
    public sealed class PourSettings : ScriptableObject
    {
        [Header("Stream Size")]
        [SerializeField, Range(0.005f, 0.2f)] private float streamRadius = 0.035f;
        [SerializeField, Range(0.005f, 0.2f)] private float minStreamRadius = 0.01f;
        [SerializeField, Range(0.005f, 0.2f)] private float maxStreamRadius = 0.12f;
        [SerializeField, Range(1.01f, 2f)] private float resizeFactorPerScroll = 1.15f;

        [Header("Flow")]
        [SerializeField, Min(0f)] private float inflowRate = 6f;

        [Header("Pitcher Motion")]
        [SerializeField, Min(0f)] private float dragStrength = 3f;

        public float StreamRadius => streamRadius;
        public float MinStreamRadius => minStreamRadius;
        public float MaxStreamRadius => maxStreamRadius;
        public float ResizeFactorPerScroll => resizeFactorPerScroll;
        public float InflowRate => inflowRate;
        public float DragStrength => dragStrength;

        private void OnValidate()
        {
            maxStreamRadius = Mathf.Max(maxStreamRadius, minStreamRadius);
            streamRadius = Mathf.Clamp(streamRadius, minStreamRadius, maxStreamRadius);
        }
    }
}
