using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Hints
{
    [Serializable]
    public sealed class Hint
    {
        [SerializeField] private InputActionReference action;
        [SerializeField] private string keyOverride = "";
        [SerializeField] private string caption = "";

        public string Caption => caption;
        public bool IsAssigned => (action != null || HasKeyOverride) && !string.IsNullOrEmpty(caption);

        private bool HasKeyOverride => !string.IsNullOrEmpty(keyOverride);

        public string GetKeyLabel()
        {
            return HasKeyOverride ? keyOverride : HintKeyLabel.For(action.action);
        }
    }
}
