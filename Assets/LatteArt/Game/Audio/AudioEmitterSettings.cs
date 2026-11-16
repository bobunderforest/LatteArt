using UnityEngine;
using UnityEngine.Audio;

namespace LatteArt.Audio
{
    [CreateAssetMenu(fileName = "SO_AudioEmitterSettings", menuName = "LatteArt/Audio Emitter Settings")]
    public sealed class AudioEmitterSettings : ScriptableObject
    {
        [Header("Routing")]
        [SerializeField] private AudioMixerGroup output;
        [SerializeField, Range(0, 256)] private int priority = 128;

        [Header("Spatial")]
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
        [SerializeField, Range(0f, 5f)] private float dopplerLevel;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField, Min(0.01f)] private float minDistance = 0.75f;
        [SerializeField, Min(0.01f)] private float maxDistance = 15f;

        public AudioSource CreateSource(GameObject owner)
        {
            var source = owner.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = output;
            source.priority = priority;
            source.spatialBlend = spatialBlend;
            source.dopplerLevel = dopplerLevel;
            source.rolloffMode = rolloffMode;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            return source;
        }

        private void OnValidate()
        {
            maxDistance = Mathf.Max(maxDistance, minDistance);
        }
    }
}
