using LatteArt.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace LatteArt.CoffeeServing
{
    [RequireComponent(typeof(CoffeeMachine))]
    public sealed class CoffeeMachineAudio : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioEmitterSettings emitter;

        [Header("Sounds")]
        [SerializeField] private AudioResource brewSound;

        private bool _hasValidSetup;
        private CoffeeMachine _machine;
        private AudioSource _source;

        private void Awake()
        {
            _machine = GetComponent<CoffeeMachine>();
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

            _machine.Brewed += PlayBrew;
        }

        private void OnDisable()
        {
            _machine.Brewed -= PlayBrew;
        }

        private void PlayBrew()
        {
            if (brewSound == null) return;

            _source.resource = brewSound;
            _source.Play();
        }

        private bool HasValidSetup()
        {
            if (emitter != null) return true;

            Debug.LogError("CoffeeMachineAudio has no emitter settings.", this);
            return false;
        }
    }
}
