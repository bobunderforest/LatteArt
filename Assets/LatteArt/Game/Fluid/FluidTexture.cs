using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace LatteArt.Fluid
{
    internal static class FluidTexture
    {
        public static RenderTexture Create(int resolution, GraphicsFormat format, string name)
        {
            var texture = new RenderTexture(resolution, resolution, format, GraphicsFormat.None)
            {
                name = name,
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.Create();
            Clear(texture);
            return texture;
        }

        public static RenderTexture Copy(RenderTexture source, string name)
        {
            var copy = Create(source.width, source.graphicsFormat, name);
            Graphics.CopyTexture(source, copy);
            return copy;
        }

        public static GraphicsFormat WritableFilterableFormat(params GraphicsFormat[] candidates)
        {
            foreach (var format in candidates)
            {
                if (SystemInfo.IsFormatSupported(format, GraphicsFormatUsage.LoadStore) &&
                    SystemInfo.IsFormatSupported(format, GraphicsFormatUsage.Linear))
                {
                    return format;
                }
            }

            return candidates[candidates.Length - 1];
        }

        public static void Release(RenderTexture texture)
        {
            texture.Release();
            Object.Destroy(texture);
        }

        public static void Clear(RenderTexture texture)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
        }
    }
}
