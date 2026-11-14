using UnityEngine;

namespace LatteArt.CoffeeTools.SteamWand
{
    [CreateAssetMenu(fileName = "SO_SteamWandSettings", menuName = "LatteArt/Steam Wand Settings")]
    public sealed class SteamWandSettings : ScriptableObject
    {
        [Header("Blow Stream")]
        [SerializeField] private StreamProfile blowStream = new();

        [Header("Blow")]
        [SerializeField, Min(0f)] private float blowRate = 8f;

        public StreamProfile BlowStream => blowStream;
        public float BlowRate => blowRate;

        private void OnValidate()
        {
            blowStream.Validate();
        }
    }
}
