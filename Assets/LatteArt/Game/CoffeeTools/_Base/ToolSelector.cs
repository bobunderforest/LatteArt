using System;
using LatteArt.Fluid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeTools
{
    public sealed class ToolSelector : MonoBehaviour
    {
        private const int NoSlot = -1;

        [SerializeField] private ToolSlot[] slots = { };
        [SerializeField, Min(0)] private int initialSlot;

        private bool _hasValidSetup;
        private int _selectedSlot;

        public event Action<InputAction> ToolSelected;

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

            ApplySelection();
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
                if (i == _selectedSlot || !slots[i].SelectAction.action.WasPressedThisFrame()) continue;

                _selectedSlot = i;
                ApplySelection();
            }
        }

        public void Activate(FluidSurface surface, Camera viewCamera)
        {
            if (!_hasValidSetup) return;

            foreach (var slot in slots)
            {
                slot.SurfaceTool.Attach(surface, viewCamera);
            }

            enabled = true;
        }

        public void Deactivate()
        {
            enabled = false;
            foreach (var slot in slots)
            {
                if (slot.Tool != null) slot.SurfaceTool.Detach();
            }
        }

        private void ApplySelection()
        {
            EnableOnly(_selectedSlot);
            ToolSelected?.Invoke(slots[_selectedSlot].SelectAction.action);
        }

        private void EnableOnly(int slotToEnable)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                var tool = slots[i].Tool;
                if (tool != null) tool.enabled = i == slotToEnable;
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
