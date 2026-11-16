using LatteArt.CoffeeServing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace LatteArt.Startup
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class StartupCover : MonoBehaviour
    {
        private const string CoverName = "startup-cover";
        private const string HiddenClass = "startup-cover--hidden";

        [Header("Wait For")]
        [SerializeField] private CoffeeStation coffeeStation;

        [Header("Held Until Fade")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Behaviour[] enabledOnFade;
        [SerializeField] private AudioSource[] playedOnFade;

        private UIDocument _document;
        private VisualElement _cover;
        private bool _isFading;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            if (coffeeStation == null || inputActions == null)
            {
                Debug.LogError("StartupCover has unassigned references.", this);
            }

            if (inputActions != null) inputActions.Disable();
        }

        private void OnEnable()
        {
            _cover = _document.rootVisualElement.Q(CoverName);
            if (_cover == null)
            {
                Debug.LogError("StartupCover's UXML has no element named " + CoverName + ".", this);
                FadeOut();
                return;
            }

            _cover.RegisterCallback<TransitionEndEvent>(HideDocument);
            if (coffeeStation != null) coffeeStation.WarmedUp += FadeOut;
        }

        private void OnDisable()
        {
            if (_cover != null) _cover.UnregisterCallback<TransitionEndEvent>(HideDocument);
            if (coffeeStation != null) coffeeStation.WarmedUp -= FadeOut;
        }

        private void Start()
        {
            var hasNothingToWaitFor = coffeeStation == null || !coffeeStation.isActiveAndEnabled;
            if (hasNothingToWaitFor || coffeeStation.IsWarmedUp) FadeOut();
        }

        private void FadeOut()
        {
            if (_isFading) return;

            _isFading = true;
            if (_cover != null) _cover.AddToClassList(HiddenClass);
            if (inputActions != null) inputActions.Enable();
            foreach (var behaviour in enabledOnFade)
            {
                if (behaviour != null) behaviour.enabled = true;
            }

            foreach (var source in playedOnFade)
            {
                if (source != null) source.Play();
            }
        }

        private void HideDocument(TransitionEndEvent transitionEnd)
        {
            _document.enabled = false;
        }
    }
}
