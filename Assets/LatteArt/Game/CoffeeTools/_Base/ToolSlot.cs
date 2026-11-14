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

        public Behaviour Tool => tool;
        public InputActionReference SelectAction => selectAction;
        public ISurfaceTool SurfaceTool => tool as ISurfaceTool;
        public bool IsAssigned => tool != null && tool is ISurfaceTool && selectAction != null;
    }
}
