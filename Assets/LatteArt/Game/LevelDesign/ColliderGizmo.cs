using UnityEngine;

namespace LatteArt.LevelDesign
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ColliderGizmo : MonoBehaviour
    {
        private const float FillAlpha = 0.25f;

        [SerializeField] private Color color = new(1f, 0.3f, 0.3f, 1f);

        private BoxCollider _boxCollider;

        private void OnDrawGizmos()
        {
            if (_boxCollider == null) _boxCollider = GetComponent<BoxCollider>();

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(color.r, color.g, color.b, FillAlpha);
            Gizmos.DrawCube(_boxCollider.center, _boxCollider.size);
            Gizmos.color = color;
            Gizmos.DrawWireCube(_boxCollider.center, _boxCollider.size);
        }
    }
}
