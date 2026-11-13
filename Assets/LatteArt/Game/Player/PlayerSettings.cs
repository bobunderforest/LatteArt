using UnityEngine;

namespace LatteArt.Player
{
    [CreateAssetMenu(fileName = "SO_PlayerSettings", menuName = "LatteArt/Player Settings")]
    public sealed class PlayerSettings : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 2f;
        [SerializeField, Range(0f, 0.5f)] private float moveSmoothTime = 0.1f;

        [Header("Interaction")]
        [SerializeField, Min(0f)] private float interactRange = 1.5f;
        [SerializeField] private LayerMask interactLayers = ~0;

        [Header("Look")]
        [SerializeField, Range(0.01f, 1f)] private float lookDegreesPerPixel = 0.1f;
        [SerializeField, Range(-89f, 0f)] private float minPitch = -80f;
        [SerializeField, Range(0f, 89f)] private float maxPitch = 80f;

        public float WalkSpeed => walkSpeed;
        public float MoveSmoothTime => moveSmoothTime;
        public float LookDegreesPerPixel => lookDegreesPerPixel;
        public float InteractRange => interactRange;
        public LayerMask InteractLayers => interactLayers;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
    }
}
