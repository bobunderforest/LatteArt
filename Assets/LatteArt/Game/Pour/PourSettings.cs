using LatteArt.Tools;
using UnityEngine;

namespace LatteArt.Pour
{
    [CreateAssetMenu(fileName = "SO_PourSettings", menuName = "LatteArt/Pour Settings")]
    public sealed class PourSettings : ScriptableObject
    {
        [Header("Stream")]
        [SerializeField] private StreamProfile stream = new();

        [Header("Flow")]
        [SerializeField, Min(0f)] private float inflowRate = 8f;

        [Header("Pitcher Motion")]
        [SerializeField, Min(0f)] private float dragStrength = 8f;
        [SerializeField, Range(0.05f, 5f)] private float fullDragSpeed = 1f;

        public StreamProfile Stream => stream;
        public float InflowRate => inflowRate;
        public float DragStrength => dragStrength;
        public float FullDragSpeed => fullDragSpeed;

        private void OnValidate()
        {
            stream.Validate();
        }
    }
}
