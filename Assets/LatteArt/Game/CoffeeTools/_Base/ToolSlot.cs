using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.CoffeeTools
{
    [Serializable]
    public sealed class ToolSlot
    {
        [SerializeField] private Behaviour tool;
        [SerializeField] private InputActionReference selectAction;
        [SerializeField] private string displayName = "";

        public Behaviour Tool => tool;
        public InputActionReference SelectAction => selectAction;
        public string DisplayName => displayName;
        public ISurfaceTool SurfaceTool => tool as ISurfaceTool;
        public bool IsAssigned => tool != null && tool is ISurfaceTool && selectAction != null;
    }
}
