using UnityEngine;

namespace LatteArt.Tools
{
    public sealed class ToolSelector : MonoBehaviour
    {
        [SerializeField] private ToolSlot[] slots = { };
        [SerializeField, Min(0)] private int initialSlot;

        private void Awake()
        {
            if (!HasValidSetup())
            {
                enabled = false;
                return;
            }

            Select(initialSlot);
        }

        private void Update()
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].SelectAction.action.WasPressedThisFrame()) Select(i);
            }
        }

        private void Select(int selectedSlot)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                slots[i].Tool.enabled = i == selectedSlot;
            }
        }

        private bool HasValidSetup()
        {
            var hasAllSlotsAssigned = slots.Length > 0;
            foreach (var slot in slots)
            {
                hasAllSlotsAssigned &= slot != null && slot.IsAssigned;
            }

            if (hasAllSlotsAssigned && initialSlot < slots.Length) return true;

            Debug.LogError("ToolSelector has unassigned slots or an initial slot out of range.", this);
            return false;
        }
    }
}
