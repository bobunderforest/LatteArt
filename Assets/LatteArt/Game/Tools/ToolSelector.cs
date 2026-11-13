using UnityEngine;

namespace LatteArt.Tools
{
    public sealed class ToolSelector : MonoBehaviour
    {
        private const int NoSlot = -1;

        [SerializeField] private ToolSlot[] slots = { };
        [SerializeField, Min(0)] private int initialSlot;

        private bool _hasValidSetup;
        private int _selectedSlot;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
            if (!_hasValidSetup) return;

            _selectedSlot = initialSlot;
            EnableOnly(NoSlot);
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            EnableOnly(_selectedSlot);
        }

        private void OnDisable()
        {
            if (!_hasValidSetup) return;

            EnableOnly(NoSlot);
        }

        private void Update()
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (!slots[i].SelectAction.action.WasPressedThisFrame()) continue;

                _selectedSlot = i;
                EnableOnly(i);
            }
        }

        private void EnableOnly(int slotToEnable)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                slots[i].Tool.enabled = i == slotToEnable;
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
