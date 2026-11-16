using System.Collections.Generic;
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

            var isWeb = Application.platform == RuntimePlatform.WebGLPlayer;
            var entries = new List<HintEntry>(hints.Length);
            foreach (var hint in hints)
            {
                if (isWeb && hint.IsHiddenOnWeb) continue;

                entries.Add(new HintEntry(hint.GetKeyLabel(), hint.Caption));
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
