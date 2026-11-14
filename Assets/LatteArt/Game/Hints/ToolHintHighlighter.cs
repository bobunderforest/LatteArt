using LatteArt.CoffeeTools;
using UnityEngine;

namespace LatteArt.Hints
{
    public sealed class ToolHintHighlighter : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ToolSelector toolSelector;
        [SerializeField] private HintPanel hints;

        private bool _hasValidSetup;

        private void Awake()
        {
            _hasValidSetup = HasValidSetup();
        }

        private void OnEnable()
        {
            if (!_hasValidSetup)
            {
                enabled = false;
                return;
            }

            toolSelector.ToolSelected += hints.Highlight;
        }

        private void OnDisable()
        {
            if (!_hasValidSetup || toolSelector == null || hints == null) return;

            toolSelector.ToolSelected -= hints.Highlight;
        }

        private bool HasValidSetup()
        {
            if (toolSelector != null && hints != null) return true;

            Debug.LogError("ToolHintHighlighter has unassigned references.", this);
            return false;
        }
    }
}
