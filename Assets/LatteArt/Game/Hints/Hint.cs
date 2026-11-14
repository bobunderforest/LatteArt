using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Hints
{
    [Serializable]
    public sealed class Hint
    {
        private const string BindingGroup = "Keyboard&Mouse";

        [SerializeField] private InputActionReference action;
        [SerializeField] private string keyOverride = "";
        [SerializeField] private string caption = "";

        public string Caption => caption;
        public bool IsAssigned => (action != null || HasKeyOverride) && !string.IsNullOrEmpty(caption);

        private bool HasKeyOverride => !string.IsNullOrEmpty(keyOverride);

        public bool IsFor(InputAction inputAction)
        {
            return action != null && inputAction != null && action.action == inputAction;
        }

        public string GetKeyLabel()
        {
            return HasKeyOverride
                ? keyOverride
                : action.action.GetBindingDisplayString(InputBinding.MaskByGroup(BindingGroup));
        }
    }
}
