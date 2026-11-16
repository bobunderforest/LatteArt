using System;
using System.Collections.Generic;
using LatteArt.Fluid;
using UnityEngine;

namespace LatteArt.CoffeeTools
{
    public sealed class ToolSelector : MonoBehaviour
    {
        private const int NoSlot = -1;

        [SerializeField] private ToolSlot[] slots = { };
        [SerializeField, Min(0)] private int initialSlot;

        private bool _hasValidSetup;
        private int _selectedSlot;

        public event Action ToolSwitched;

        public IReadOnlyList<ToolSlot> Slots => slots;
        public int SelectedSlot => _selectedSlot;
        public ISurfaceTool SelectedTool => _hasValidSetup ? slots[_selectedSlot].SurfaceTool : null;

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
                if (slots[i].SelectAction.action.WasPressedThisFrame()) Select(i);
            }
        }

        public bool TrySelect(int slot)
        {
            if (!enabled || slot < 0 || slot >= slots.Length) return false;

            Select(slot);
            return true;
        }

        public void Activate(FluidSurface surface, Camera viewCamera)
        {
            if (!_hasValidSetup) return;

            foreach (var slot in slots)
            {
                slot.SurfaceTool.Attach(surface, viewCamera);
            }

            _selectedSlot = initialSlot;
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

        private void Select(int slot)
        {
            if (slot == _selectedSlot) return;

            _selectedSlot = slot;
            EnableOnly(_selectedSlot);
            ToolSwitched?.Invoke();
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
