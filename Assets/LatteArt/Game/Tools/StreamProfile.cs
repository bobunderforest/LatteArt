using System;
using UnityEngine;

namespace LatteArt.Tools
{
    [Serializable]
    public sealed class StreamProfile
    {
        [Header("Size")]
        [SerializeField, Range(0.005f, 0.2f)] private float radius = 0.035f;
        [SerializeField, Range(0.005f, 0.2f)] private float minRadius = 0.01f;
        [SerializeField, Range(0.005f, 0.2f)] private float maxRadius = 0.12f;
        [SerializeField, Range(1.01f, 2f)] private float resizeFactorPerScroll = 1.15f;

        [Header("Growth")]
        [SerializeField, Range(0.001f, 0.05f)] private float startRadius = 0.01f;
        [SerializeField, Range(0.01f, 2f)] private float growSpeed = 0.25f;
        [SerializeField, Range(0.01f, 0.5f)] private float growEaseTime = 0.03f;
        [SerializeField, Range(0.05f, 1f)] private float taperTime = 0.15f;

        public ToolRadius CreateRadius()
        {
            return new ToolRadius(radius, minRadius, maxRadius, resizeFactorPerScroll);
        }

        public StreamGrowth CreateGrowth()
        {
            return new StreamGrowth(startRadius, growSpeed, growEaseTime, taperTime);
        }

        public void Validate()
        {
            maxRadius = Mathf.Max(maxRadius, minRadius);
            radius = Mathf.Clamp(radius, minRadius, maxRadius);
        }
    }
}
