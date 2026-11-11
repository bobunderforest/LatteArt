using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace LatteArt.Fluid
{
    internal sealed class PingPongTexture : IDisposable
    {
        public PingPongTexture(int resolution, GraphicsFormat format, string name)
        {
            Read = FluidTexture.Create(resolution, format, name + "A");
            Write = FluidTexture.Create(resolution, format, name + "B");
        }

        public RenderTexture Read { get; private set; }
        public RenderTexture Write { get; private set; }

        public void Swap()
        {
            (Read, Write) = (Write, Read);
        }

        public void Dispose()
        {
            FluidTexture.Release(Read);
            FluidTexture.Release(Write);
        }
    }
}
