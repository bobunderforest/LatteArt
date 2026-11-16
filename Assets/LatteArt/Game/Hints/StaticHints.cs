using UnityEngine;

namespace LatteArt.Hints
{
    [RequireComponent(typeof(HintPanel))]
    public sealed class StaticHints : MonoBehaviour
    {
        [SerializeField] private Hint[] hints = { };

        private void Awake()
        {
            if (!HasValidSetup()) return;

            var entries = new HintEntry[hints.Length];
            for (var i = 0; i < hints.Length; i++)
            {
                entries[i] = new HintEntry(hints[i].GetKeyLabel(), hints[i].Caption);
            }

            GetComponent<HintPanel>().SetEntries(entries);
        }

        private bool HasValidSetup()
        {
            var hasAllHintsAssigned = hints.Length > 0;
            foreach (var hint in hints)
            {
                hasAllHintsAssigned &= hint != null && hint.IsAssigned;
            }

            if (hasAllHintsAssigned) return true;

            Debug.LogError("StaticHints has hints without an action or caption.", this);
            return false;
        }
    }
}
