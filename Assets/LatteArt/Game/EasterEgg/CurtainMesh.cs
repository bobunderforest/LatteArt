using UnityEngine;

namespace LatteArt.EasterEgg
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public sealed class CurtainMesh : MonoBehaviour
    {
        [Header("Size")]
        [SerializeField, Min(0.01f)] private float width = 3f;
        [SerializeField, Min(0.01f)] private float height = 3f;

        [Header("Density")]
        [SerializeField, Min(1)] private int columnsPerMeter = 30;
        [SerializeField, Min(1)] private int rowsPerMeter = 4;

        [Header("Bounds")]
        [SerializeField, Min(0f)] private float maxFoldDepth = 0.3f;

        private bool _isDirty;
        private Mesh _mesh;
        private MeshFilter _meshFilter;

        private void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = new Mesh { name = "CurtainMesh", hideFlags = HideFlags.DontSave };
            _meshFilter.sharedMesh = _mesh;
            Rebuild();
        }

        private void OnDisable()
        {
            if (_meshFilter.sharedMesh == _mesh) _meshFilter.sharedMesh = null;

            if (Application.isPlaying) Destroy(_mesh);
            else DestroyImmediate(_mesh);
            _mesh = null;
        }

        private void OnValidate()
        {
            _isDirty = true;
        }

        private void Update()
        {
            if (_isDirty) Rebuild();
        }

        private void Rebuild()
        {
            _isDirty = false;
            var size = new Vector2(width, height);
            var segments = new Vector2Int(
                Mathf.CeilToInt(width * columnsPerMeter),
                Mathf.CeilToInt(height * rowsPerMeter));
            CurtainMeshBuilder.Fill(_mesh, size, segments, maxFoldDepth);
        }
    }
}
