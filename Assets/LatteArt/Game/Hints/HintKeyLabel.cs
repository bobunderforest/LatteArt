using UnityEngine.InputSystem;

namespace LatteArt.Hints
{
    public static class HintKeyLabel
    {
        private const string BindingGroup = "Keyboard&Mouse";

        public static string For(InputAction action)
        {
            return action.GetBindingDisplayString(InputBinding.MaskByGroup(BindingGroup));
        }
    }
}
