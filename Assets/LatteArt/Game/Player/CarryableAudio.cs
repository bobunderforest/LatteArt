using LatteArt.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace LatteArt.Player
{
    [RequireComponent(typeof(Carryable))]
    public sealed class CarryableAudio : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioEmitterSettings emitter;

        [Header("Sounds")]
        [SerializeField] private AudioResource pickUpSound;
        [SerializeField] private AudioResource placeSound;

        private Carryable _carryable;
        private bool _hasValidSetup;
        private AudioSource _source;

        private void Awake()
        {
            _carryable = GetComponent<Carryable>();
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

            _carryable.PickedUp += PlayPickUp;
            _carryable.Placed += PlayPlace;
        }

        private void OnDisable()
        {
            _carryable.PickedUp -= PlayPickUp;
            _carryable.Placed -= PlayPlace;
        }

        private void PlayPickUp()
        {
            Play(pickUpSound);
        }

        private void PlayPlace(PlacementSurface surface)
        {
            if (surface == null) Play(placeSound);
        }

        private void Play(AudioResource sound)
        {
            if (sound == null) return;

            _source.resource = sound;
            _source.Play();
        }

        private bool HasValidSetup()
        {
            if (emitter != null) return true;

            Debug.LogError("CarryableAudio has no emitter settings.", this);
            return false;
        }
    }
}
