using LatteArt.Audio;
using LatteArt.CoffeeTools;
using UnityEngine;
using UnityEngine.Audio;

namespace LatteArt.CoffeeServing
{
    [RequireComponent(typeof(CoffeeStation))]
    public sealed class CoffeeStationAudio : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioEmitterSettings emitter;

        [Header("Cup")]
        [SerializeField] private AudioResource cupPlacedSound;
        [SerializeField] private AudioResource surfaceResetSound;

        [Header("Station")]
        [SerializeField] private AudioResource toolSwitchSound;

        private bool _hasValidSetup;
        private AudioSource _source;
        private CoffeeStation _station;
        private ToolSelector _toolSelector;

        private void Awake()
        {
            _station = GetComponent<CoffeeStation>();
            _toolSelector = GetComponentInChildren<ToolSelector>(true);
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _source = emitter.CreateSource(gameObject);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            _station.CupPlaced += PlayCupPlaced;
            _station.SurfaceReset += PlaySurfaceReset;
            _toolSelector.ToolSwitched += PlayToolSwitch;
        }

        private void OnDisable()
        {
            if (!_hasValidSetup) return;

            _station.CupPlaced -= PlayCupPlaced;
            _station.SurfaceReset -= PlaySurfaceReset;
            _toolSelector.ToolSwitched -= PlayToolSwitch;
        }

        private void PlayCupPlaced()
        {
            Play(cupPlacedSound);
        }

        private void PlaySurfaceReset()
        {
            Play(surfaceResetSound);
        }

        private void PlayToolSwitch()
        {
            Play(toolSwitchSound);
        }

        private void Play(AudioResource sound)
        {
            if (sound == null) return;

            _source.resource = sound;
            _source.Play();
        }

        private bool HasValidSetup()
        {
            if (emitter != null && _toolSelector != null) return true;

            Debug.LogError("CoffeeStationAudio needs emitter settings and a ToolSelector in children.", this);
            return false;
        }
    }
}
