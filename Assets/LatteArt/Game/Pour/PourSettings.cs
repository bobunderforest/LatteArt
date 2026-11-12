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

        [Header("Stream Growth")]
        [SerializeField, Range(0.001f, 0.05f)] private float startRadius = 0.01f;
        [SerializeField, Range(0.01f, 2f)] private float growSpeed = 0.25f;
        [SerializeField, Range(0.01f, 0.5f)] private float growEaseTime = 0.03f;
        [SerializeField, Range(0.05f, 1f)] private float taperTime = 0.15f;

        [Header("Flow")]
        [SerializeField, Min(0f)] private float inflowRate = 8f;

        [Header("Pitcher Motion")]
        [SerializeField, Min(0f)] private float dragStrength = 8f;
        [SerializeField, Range(0.05f, 5f)] private float fullDragSpeed = 1f;

        public float StreamRadius => streamRadius;
        public float MinStreamRadius => minStreamRadius;
        public float MaxStreamRadius => maxStreamRadius;
        public float ResizeFactorPerScroll => resizeFactorPerScroll;
        public float StartRadius => startRadius;
        public float GrowSpeed => growSpeed;
        public float GrowEaseTime => growEaseTime;
        public float TaperTime => taperTime;
        public float InflowRate => inflowRate;
        public float DragStrength => dragStrength;
        public float FullDragSpeed => fullDragSpeed;

        private void OnValidate()
        {
            maxStreamRadius = Mathf.Max(maxStreamRadius, minStreamRadius);
            streamRadius = Mathf.Clamp(streamRadius, minStreamRadius, maxStreamRadius);
        }
    }
}
