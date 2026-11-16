using UnityEngine;

namespace LatteArt.Player
{
    [CreateAssetMenu(fileName = "SO_PlayerSettings", menuName = "LatteArt/Player Settings")]
    public sealed class PlayerSettings : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 2f;
        [SerializeField, Range(0f, 0.5f)] private float moveSmoothTime = 0.1f;
        [SerializeField, Min(0.1f)] private float maxFallDistance = 10f;

        [Header("Interaction")]
        [SerializeField, Min(0f)] private float interactRange = 1.5f;
        [SerializeField] private LayerMask interactLayers = ~0;
        [SerializeField, Range(0f, 90f)] private float maxPlaceSlope = 20f;
        [SerializeField, Min(0f)] private float minPlaceDistance = 0.5f;
        [SerializeField] private Material placePreviewMaterial;

        [Header("Look")]
        [SerializeField, Range(0.01f, 1f)] private float lookDegreesPerPixel = 0.1f;
        [SerializeField, Range(-89f, 0f)] private float minPitch = -80f;
        [SerializeField, Range(0f, 89f)] private float maxPitch = 80f;

        public float WalkSpeed => walkSpeed;
        public float MoveSmoothTime => moveSmoothTime;
        public float MaxFallDistance => maxFallDistance;
        public float LookDegreesPerPixel => lookDegreesPerPixel;
        public float InteractRange => interactRange;
        public LayerMask InteractLayers => interactLayers;
        public float MaxPlaceSlope => maxPlaceSlope;
        public float MinPlaceDistance => minPlaceDistance;
        public Material PlacePreviewMaterial => placePreviewMaterial;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
    }
}
