using LatteArt.MaterialLibrary.Outline;
using LatteArt.Player;
using UnityEngine;

namespace LatteArt.CoffeeServing
{
    [RequireComponent(typeof(OutlineHighlight))]
    public sealed class CoffeeMachine : MonoBehaviour, IInteractable
    {
        [Header("Dependencies")]
        [SerializeField] private CoffeeStation station;

        private bool _hasValidSetup;
        private OutlineHighlight _highlight;

        private void Awake()
        {
            _highlight = GetComponent<OutlineHighlight>();
            _hasValidSetup = HasValidSetup();
        }

        public void SetHovered(bool isHovered)
        {
            if (!_hasValidSetup) return;

            _highlight.SetVisible(isHovered && station.CanPourCoffee);
        }

        public void Interact(Interactor interactor)
        {
            if (!_hasValidSetup || !station.TryPourCoffee()) return;

            _highlight.SetVisible(false);
        }

        private bool HasValidSetup()
        {
            if (station != null) return true;

            Debug.LogError("CoffeeMachine has unassigned references.", this);
            return false;
        }
    }
}
