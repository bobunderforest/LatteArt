using LatteArt.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace LatteArt.CoffeeTools
{
    public sealed class ToolActivityAudio : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioEmitterSettings emitter;

        [Header("Sound")]
        [SerializeField] private AudioResource sound;

        [Header("Playback")]
        [SerializeField] private bool isLooping = true;
        [SerializeField, Range(0.01f, 1f)] private float fadeTime = 0.15f;

        [Header("Intensity")]
        [SerializeField, Range(0f, 1f)] private float volumeByIntensity;

        private IToolActivity _activity;
        private float _gain;
        private bool _hasValidSetup;
        private float _intensity;
        private AudioSource _source;
        private bool _wasActive;

        private void Awake()
        {
            _activity = GetComponent<IToolActivity>();
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _source = emitter.CreateSource(gameObject);
            _source.resource = sound;
        }

        private void OnEnable()
        {
            if (!_hasValidSetup) enabled = false;
        }

        private void OnDisable()
        {
            _gain = 0f;
            _wasActive = false;
            if (_source != null) _source.Stop();
        }

        private void Update()
        {
            var fadeStep = Time.deltaTime / fadeTime;
            var isActive = _activity.IsActive;
            if (isActive && !_wasActive) StartPlayback();
            _wasActive = isActive;

            _gain = Mathf.MoveTowards(_gain, isActive ? 1f : 0f, fadeStep);
            if (isActive) _intensity = Mathf.MoveTowards(_intensity, _activity.Intensity, fadeStep);

            if (_gain <= 0f)
            {
                if (_source.isPlaying) _source.Stop();
                return;
            }

            _source.volume = _gain * Mathf.Lerp(1f, _intensity, volumeByIntensity);
        }

        private void StartPlayback()
        {
            if (isLooping && _source.isPlaying) return;

            _source.loop = isLooping;
            _source.Play();
        }

        private bool HasValidSetup()
        {
            if (emitter != null && sound != null && _activity != null) return true;

            Debug.LogError("ToolActivityAudio needs emitter settings, a sound and an IToolActivity tool.", this);
            return false;
        }
    }
}
