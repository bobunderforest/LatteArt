using LatteArt.Bar;
using UnityEngine;

namespace LatteArt.Hints
{
    public sealed class HintSwitcher : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CoffeeStation station;

        [Header("Panels")]
        [SerializeField] private HintPanel firstPersonHints;
        [SerializeField] private HintPanel stationHints;

        private bool _hasValidSetup;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            station.InUseChanged += ShowHintsFor;
            ShowHintsFor(station.IsInUse);
        }

        private void OnDisable()
        {
            if (!_hasValidSetup) return;

            if (station != null) station.InUseChanged -= ShowHintsFor;
            if (firstPersonHints != null) firstPersonHints.enabled = false;
            if (stationHints != null) stationHints.enabled = false;
        }

        private void ShowHintsFor(bool isStationInUse)
        {
            firstPersonHints.enabled = !isStationInUse;
            stationHints.enabled = isStationInUse;
        }

        private bool HasValidSetup()
        {
            if (station != null && firstPersonHints != null && stationHints != null) return true;

            Debug.LogError("HintSwitcher has unassigned references.", this);
            return false;
        }
    }
}
