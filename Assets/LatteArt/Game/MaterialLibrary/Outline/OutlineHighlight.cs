using System.Collections.Generic;
using UnityEngine;

namespace LatteArt.MaterialLibrary.Outline
{
    public sealed class OutlineHighlight : MonoBehaviour
    {
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Renderer[] outlinedRenderers = { };

        private readonly List<Material> _materials = new();
        private bool _hasValidSetup;
        private bool _isVisible;

        private void Awake()
        {
            if (outlinedRenderers.Length == 0) outlinedRenderers = GetComponentsInChildren<Renderer>();
            _hasValidSetup = HasValidSetup();
        }

        private void OnDisable()
        {
            SetVisible(false);
        }

        public void SetVisible(bool isVisible)
        {
            if (!_hasValidSetup || isVisible == _isVisible) return;

            _isVisible = isVisible;
            foreach (var outlinedRenderer in outlinedRenderers)
            {
                outlinedRenderer.GetSharedMaterials(_materials);
                if (isVisible) _materials.Add(outlineMaterial);
                else _materials.Remove(outlineMaterial);

                outlinedRenderer.SetSharedMaterials(_materials);
            }
        }

        private bool HasValidSetup()
        {
            if (outlineMaterial != null && outlinedRenderers.Length > 0) return true;

            Debug.LogError("OutlineHighlight has no material or no renderers.", this);
            return false;
        }
    }
}
