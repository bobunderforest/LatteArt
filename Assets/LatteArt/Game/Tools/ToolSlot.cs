using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LatteArt.Tools
{
    [Serializable]
    public sealed class ToolSlot
    {
        [SerializeField] private Behaviour tool;
        [SerializeField] private InputActionReference selectAction;

        public Behaviour Tool => tool;
        public InputActionReference SelectAction => selectAction;
        public bool IsAssigned => tool != null && selectAction != null;
    }
}
