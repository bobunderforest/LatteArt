using LatteArt.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace LatteArt.Player
{
    [RequireComponent(typeof(PlacementSurface))]
    public sealed class PlacementSurfaceAudio : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioEmitterSettings emitter;

        [Header("Sounds")]
        [SerializeField] private AudioResource placedSound;

        private bool _hasValidSetup;
        private AudioSource _source;
        private PlacementSurface _surface;

        private void Awake()
        {
            _surface = GetComponent<PlacementSurface>();
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _source = emitter.CreateSource(gameObject);
            _source.resource = placedSound;
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            _surface.CarryablePlaced += PlayPlaced;
        }

        private void OnDisable()
        {
            _surface.CarryablePlaced -= PlayPlaced;
        }

        private void PlayPlaced(Carryable carryable)
        {
            _source.Play();
        }

        private bool HasValidSetup()
        {
            if (emitter != null && placedSound != null) return true;

            Debug.LogError("PlacementSurfaceAudio needs emitter settings and a placed sound.", this);
            return false;
        }
    }
}
