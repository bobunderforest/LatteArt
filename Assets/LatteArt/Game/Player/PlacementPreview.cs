using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LatteArt.Player
{
    public sealed class PlacementPreview
    {
        private readonly List<PreviewPart> _parts = new();
        private readonly RenderParams _renderParams;

        public PlacementPreview(Material material)
        {
            _renderParams = new RenderParams(material)
            {
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false
            };
        }

        public void Capture(Transform root)
        {
            _parts.Clear();
            var rootFromWorld = Matrix4x4.TRS(root.position, root.rotation, Vector3.one).inverse;
            foreach (var meshFilter in root.GetComponentsInChildren<MeshFilter>())
            {
                var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                if (meshFilter.sharedMesh == null || meshRenderer == null || !meshRenderer.enabled) continue;
                if (meshFilter.TryGetComponent<HiddenInPlacementPreview>(out _)) continue;

                var rootFromMesh = rootFromWorld * meshFilter.transform.localToWorldMatrix;
                _parts.Add(new PreviewPart(meshFilter.sharedMesh, rootFromMesh));
            }
        }

        public void Clear()
        {
            _parts.Clear();
        }

        public void Draw(Vector3 position, Quaternion rotation)
        {
            var worldFromRoot = Matrix4x4.TRS(position, rotation, Vector3.one);
            foreach (var part in _parts)
            {
                var worldFromMesh = worldFromRoot * part.RootFromMesh;
                for (var subMesh = 0; subMesh < part.Mesh.subMeshCount; subMesh++)
                {
                    Graphics.RenderMesh(_renderParams, part.Mesh, subMesh, worldFromMesh);
                }
            }
        }

        private readonly struct PreviewPart
        {
            public PreviewPart(Mesh mesh, Matrix4x4 rootFromMesh)
            {
                Mesh = mesh;
                RootFromMesh = rootFromMesh;
            }

            public Mesh Mesh { get; }
            public Matrix4x4 RootFromMesh { get; }
        }
    }
}
