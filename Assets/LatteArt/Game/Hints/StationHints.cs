using System.Collections.Generic;
using LatteArt.CoffeeServing;
using LatteArt.CoffeeTools;
using UnityEngine;

namespace LatteArt.Hints
{
    [RequireComponent(typeof(HintPanel))]
    public sealed class StationHints : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CoffeeStation station;

        [Header("Captions")]
        [SerializeField] private string serveCaption = "Complete";
        [SerializeField] private string resetCaption = "Reset";
        [SerializeField] private string sizeCaption = "Change size";
        [SerializeField] private string sizeKeyLabel = "Mouse wheel";

        private ToolSelector _toolSelector;

        private void Start()
        {
            _toolSelector = station != null ? station.ToolSelector : null;
            if (!HasValidSetup()) return;

            GetComponent<HintPanel>().SetEntries(CreateEntries());
        }

        private List<HintEntry> CreateEntries()
        {
            var entries = new List<HintEntry>();
            for (var i = 0; i < _toolSelector.Slots.Count; i++)
            {
                entries.Add(CreateToolEntry(i));
            }

            entries.Add(new HintEntry(HintKeyLabel.For(station.ServeAction), serveCaption,
                () => station.TryServe()));
            entries.Add(new HintEntry(HintKeyLabel.For(station.ResetAction), resetCaption,
                () => station.TryResetSurface()));
            entries.Add(new HintEntry(sizeKeyLabel, sizeCaption,
                readSlider: ReadSelectedToolSize, writeSlider: ResizeSelectedTool));
            return entries;
        }

        private HintEntry CreateToolEntry(int slot)
        {
            var toolSlot = _toolSelector.Slots[slot];
            return new HintEntry(HintKeyLabel.For(toolSlot.SelectAction.action), toolSlot.DisplayName,
                () => _toolSelector.TrySelect(slot), () => _toolSelector.SelectedSlot == slot);
        }

        private float ReadSelectedToolSize()
        {
            var tool = _toolSelector.SelectedTool;
            return tool != null ? tool.NormalizedRadius : 0f;
        }

        private void ResizeSelectedTool(float normalizedRadius)
        {
            var tool = _toolSelector.SelectedTool;
            if (tool != null) tool.NormalizedRadius = normalizedRadius;
        }

        private bool HasValidSetup()
        {
            if (station == null || _toolSelector == null || station.ServeAction == null ||
                station.ResetAction == null || !HasNamedSlots())
            {
                Debug.LogError("StationHints has unassigned references or tool slots without a name.", this);
                return false;
            }

            return true;
        }

        private bool HasNamedSlots()
        {
            foreach (var slot in _toolSelector.Slots)
            {
                if (slot == null || !slot.IsAssigned || string.IsNullOrEmpty(slot.DisplayName)) return false;
            }

            return _toolSelector.Slots.Count > 0;
        }
    }
}
