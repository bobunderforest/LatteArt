using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace LatteArt.Fluid
{
    public sealed class FluidSimulation : IDisposable
    {
        private const int ThreadGroupSize = 8;
        private const float InactiveInflowRadius = 1f;
        private const float GridMarginTexels = 1.5f;
        private const int InflowMassSamplesPerAxis = 16;
        private const float InflowExtentInRadii = 3f;

        private static readonly Vector2 CupCenterUv = new(0.5f, 0.5f);

        private static readonly int ResolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int TexelSizeId = Shader.PropertyToID("_TexelSize");
        private static readonly int CupRadiusId = Shader.PropertyToID("_CupRadius");
        private static readonly int DeltaTimeId = Shader.PropertyToID("_DeltaTime");
        private static readonly int ForceFromId = Shader.PropertyToID("_ForceFrom");
        private static readonly int ForceToId = Shader.PropertyToID("_ForceTo");
        private static readonly int ForceVelocityId = Shader.PropertyToID("_ForceVelocity");
        private static readonly int ForceRadiusId = Shader.PropertyToID("_ForceRadius");
        private static readonly int StirFromId = Shader.PropertyToID("_StirFrom");
        private static readonly int StirToId = Shader.PropertyToID("_StirTo");
        private static readonly int StirVelocityId = Shader.PropertyToID("_StirVelocity");
        private static readonly int StirRadiusId = Shader.PropertyToID("_StirRadius");
        private static readonly int StirCouplingId = Shader.PropertyToID("_StirCoupling");
        private static readonly int StirRetentionId = Shader.PropertyToID("_StirRetention");
        private static readonly int MilkFromId = Shader.PropertyToID("_MilkFrom");
        private static readonly int MilkToId = Shader.PropertyToID("_MilkTo");
        private static readonly int MilkRadiusId = Shader.PropertyToID("_MilkRadius");
        private static readonly int InflowCenterId = Shader.PropertyToID("_InflowCenter");
        private static readonly int InflowRadiusId = Shader.PropertyToID("_InflowRadius");
        private static readonly int InflowRateId = Shader.PropertyToID("_InflowRate");
        private static readonly int VolumeBalanceId = Shader.PropertyToID("_VolumeBalance");
        private static readonly int SinkFractionId = Shader.PropertyToID("_SinkFraction");
        private static readonly int SinkRetentionId = Shader.PropertyToID("_SinkRetention");
        private static readonly int FoamResetMaskId = Shader.PropertyToID("_FoamResetMask");
        private static readonly int VelocityDampingId = Shader.PropertyToID("_VelocityDamping");
        private static readonly int AdvectionSignId = Shader.PropertyToID("_AdvectionSign");
        private static readonly int VelocityId = Shader.PropertyToID("_Velocity");
        private static readonly int VelocityOutId = Shader.PropertyToID("_VelocityOut");
        private static readonly int StirId = Shader.PropertyToID("_Stir");
        private static readonly int StirOutId = Shader.PropertyToID("_StirOut");
        private static readonly int SinkId = Shader.PropertyToID("_Sink");
        private static readonly int SinkOutId = Shader.PropertyToID("_SinkOut");
        private static readonly int FoamCoordsId = Shader.PropertyToID("_FoamCoords");
        private static readonly int FoamCoordsOutId = Shader.PropertyToID("_FoamCoordsOut");
        private static readonly int DyeId =Shader.PropertyToID("_Dye");
        private static readonly int DyeForwardId = Shader.PropertyToID("_DyeForward");
        private static readonly int DyeBackwardId = Shader.PropertyToID("_DyeBackward");
        private static readonly int DyeOutId = Shader.PropertyToID("_DyeOut");
        private static readonly int PressureId = Shader.PropertyToID("_Pressure");
        private static readonly int PressureOutId = Shader.PropertyToID("_PressureOut");
        private static readonly int DivergenceId = Shader.PropertyToID("_Divergence");
        private static readonly int DivergenceOutId = Shader.PropertyToID("_DivergenceOut");

        private readonly ComputeShader _shader;
        private readonly FluidSettings _settings;
        private readonly int _resolution;
        private readonly int _threadGroups;

        private readonly int _addForceKernel;
        private readonly int _stirKernel;
        private readonly int _settleStirKernel;
        private readonly int _injectMilkKernel;
        private readonly int _divergenceKernel;
        private readonly int _jacobiKernel;
        private readonly int _subtractGradientKernel;
        private readonly int _advectVelocityKernel;
        private readonly int _advectDyeKernel;
        private readonly int _advectSinkKernel;
        private readonly int _correctDyeKernel;
        private readonly int _advectFoamCoordsKernel;

        private readonly PingPongTexture _velocity;
        private readonly PingPongTexture _stir;
        private readonly PingPongTexture _sink;
        private readonly PingPongTexture _dye;
        private readonly PingPongTexture _foamCoords;
        private readonly PingPongTexture _pressure;
        private readonly RenderTexture _divergence;
        private readonly RenderTexture _dyeForward;
        private readonly RenderTexture _dyeBackward;

        private Vector2 _inflowCenter;
        private float _inflowRadius = InactiveInflowRadius;
        private float _inflowRate;
        private float _inflowMass;
        private float _sinkMass;
        private float _sinkRetention;
        private float _sunkMilkVolume;
        private float _foamTime;
        private bool _hasFoamCoords;

        public FluidSimulation(ComputeShader shader, FluidSettings settings)
        {
            _shader = shader;
            _settings = settings;
            _resolution = settings.Resolution;
            _threadGroups = Mathf.CeilToInt(_resolution / (float)ThreadGroupSize);

            _addForceKernel = shader.FindKernel("AddForce");
            _stirKernel = shader.FindKernel("Stir");
            _settleStirKernel = shader.FindKernel("SettleStir");
            _injectMilkKernel = shader.FindKernel("InjectMilk");
            _divergenceKernel = shader.FindKernel("Divergence");
            _jacobiKernel = shader.FindKernel("Jacobi");
            _subtractGradientKernel = shader.FindKernel("SubtractGradient");
            _advectVelocityKernel = shader.FindKernel("AdvectVelocity");
            _advectDyeKernel = shader.FindKernel("AdvectDye");
            _advectSinkKernel = shader.FindKernel("AdvectSink");
            _correctDyeKernel = shader.FindKernel("CorrectDye");
            _advectFoamCoordsKernel = shader.FindKernel("AdvectFoamCoords");

            _velocity = new PingPongTexture(_resolution, GraphicsFormat.R16G16_SFloat, "FluidVelocity");
            _stir = new PingPongTexture(_resolution, GraphicsFormat.R16G16_SFloat, "FluidStir");
            _sink = new PingPongTexture(_resolution, GraphicsFormat.R32_SFloat, "FluidSink");
            _dye = new PingPongTexture(_resolution, GraphicsFormat.R16_SFloat, "FluidDye");
            _foamCoords = new PingPongTexture(_resolution, GraphicsFormat.R32G32B32A32_SFloat, "FluidFoamCoords");
            _pressure = new PingPongTexture(_resolution, GraphicsFormat.R32_SFloat, "FluidPressure");
            _divergence = FluidTexture.Create(_resolution, GraphicsFormat.R32_SFloat, "FluidDivergence");
            _dyeForward = FluidTexture.Create(_resolution, GraphicsFormat.R16_SFloat, "FluidDyeForward");
            _dyeBackward = FluidTexture.Create(_resolution, GraphicsFormat.R16_SFloat, "FluidDyeBackward");
        }

        public RenderTexture Dye => _dye.Read;
        public RenderTexture FoamCoords => _foamCoords.Read;
        public float FoamPhase => Mathf.Repeat(_foamTime / _settings.FoamRefreshTime, 1f);
        public float CupRadius => Mathf.Min(_settings.CupRadius, 0.5f - GridMarginTexels / _resolution);
        public float CoffeeDilution => Mathf.Clamp01(_sunkMilkVolume / (CupArea * _settings.FullDilutionVolume));

        private float CupArea => Mathf.PI * CupRadius * CupRadius;

        public void AddForce(Vector2 fromUv, Vector2 toUv, Vector2 velocity, float radius)
        {
            SetGridParameters();
            _shader.SetVector(ForceFromId, fromUv);
            _shader.SetVector(ForceToId, toUv);
            _shader.SetVector(ForceVelocityId, velocity);
            _shader.SetFloat(ForceRadiusId, radius);

            _shader.SetTexture(_addForceKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_addForceKernel, VelocityOutId, _velocity.Write);
            Dispatch(_addForceKernel);
            _velocity.Swap();
        }

        public void Stir(Vector2 fromUv, Vector2 toUv, Vector2 velocity, float radius, float coupling)
        {
            SetGridParameters();
            _shader.SetVector(StirFromId, fromUv);
            _shader.SetVector(StirToId, toUv);
            _shader.SetVector(StirVelocityId, velocity);
            _shader.SetFloat(StirRadiusId, radius);
            _shader.SetFloat(StirCouplingId, coupling);

            _shader.SetTexture(_stirKernel, StirId, _stir.Read);
            _shader.SetTexture(_stirKernel, StirOutId, _stir.Write);
            Dispatch(_stirKernel);
            _stir.Swap();
        }

        public void InjectMilk(Vector2 fromUv, Vector2 toUv, float radius)
        {
            SetGridParameters();
            _shader.SetVector(MilkFromId, fromUv);
            _shader.SetVector(MilkToId, toUv);
            _shader.SetFloat(MilkRadiusId, radius);

            _shader.SetTexture(_injectMilkKernel, DyeId, _dye.Read);
            _shader.SetTexture(_injectMilkKernel, DyeOutId, _dye.Write);
            Dispatch(_injectMilkKernel);
            _dye.Swap();
        }

        public void SetInflow(Vector2 uv, float radius, float rate)
        {
            _inflowCenter = uv;
            _inflowRadius = radius;
            _inflowRate = rate;
        }

        public void StopInflow()
        {
            _inflowRadius = InactiveInflowRadius;
            _inflowRate = 0f;
        }

        public void Step(float deltaTime)
        {
            SetGridParameters();
            _shader.SetFloat(DeltaTimeId, deltaTime);
            _sinkRetention = Mathf.Exp(-deltaTime / _settings.MilkSinkTime);
            _shader.SetFloat(SinkRetentionId, _sinkRetention);

            ProjectVelocity(deltaTime);
            AdvectDye();
            AdvectSink(deltaTime);
            AdvectFoamCoords(deltaTime);
            AdvectVelocity();
            SettleStir(deltaTime);
        }

        public void Dispose()
        {
            _velocity.Dispose();
            _stir.Dispose();
            _sink.Dispose();
            _dye.Dispose();
            _foamCoords.Dispose();
            _pressure.Dispose();
            FluidTexture.Release(_divergence);
            FluidTexture.Release(_dyeForward);
            FluidTexture.Release(_dyeBackward);
        }

        private void SetGridParameters()
        {
            _shader.SetInt(ResolutionId, _resolution);
            _shader.SetFloat(TexelSizeId, 1f / _resolution);
            _shader.SetFloat(CupRadiusId, CupRadius);
        }

        private void ProjectVelocity(float deltaTime)
        {
            ComputeDivergence(deltaTime);
            SolvePressure();
            SubtractPressureGradient();
        }

        private void ComputeDivergence(float deltaTime)
        {
            _inflowMass = _inflowRate > 0f ? InflowMassInsideCup(CupRadius) : 0f;
            var sinkingRate = _sinkMass * (1f - _sinkRetention) / deltaTime;
            var volumeBalance = (_inflowRate * _inflowMass - sinkingRate) / CupArea;
            _shader.SetVector(InflowCenterId, _inflowCenter);
            _shader.SetFloat(InflowRadiusId, _inflowRadius);
            _shader.SetFloat(InflowRateId, _inflowRate);
            _shader.SetFloat(VolumeBalanceId, volumeBalance);

            _shader.SetTexture(_divergenceKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_divergenceKernel, StirId, _stir.Read);
            _shader.SetTexture(_divergenceKernel, SinkId, _sink.Read);
            _shader.SetTexture(_divergenceKernel, DivergenceOutId, _divergence);
            Dispatch(_divergenceKernel);
        }

        private float InflowMassInsideCup(float cupRadius)
        {
            var extent = _inflowRadius * InflowExtentInRadii;
            var sampleSpacing = 2f * extent / InflowMassSamplesPerAxis;
            var cupRadiusSquared = cupRadius * cupRadius;
            var inflowRadiusSquared = _inflowRadius * _inflowRadius;
            var mass = 0f;

            for (var y = 0; y < InflowMassSamplesPerAxis; y++)
            {
                for (var x = 0; x < InflowMassSamplesPerAxis; x++)
                {
                    var offset = new Vector2(
                        (x + 0.5f) * sampleSpacing - extent,
                        (y + 0.5f) * sampleSpacing - extent);
                    var isInsideCup = (_inflowCenter + offset - CupCenterUv).sqrMagnitude < cupRadiusSquared;
                    if (!isInsideCup) continue;

                    mass += Mathf.Exp(-offset.sqrMagnitude / inflowRadiusSquared);
                }
            }

            return mass * sampleSpacing * sampleSpacing;
        }

        private void SolvePressure()
        {
            _shader.SetTexture(_jacobiKernel, DivergenceId, _divergence);
            for (var i = 0; i < _settings.PressureIterations; i++)
            {
                _shader.SetTexture(_jacobiKernel, PressureId, _pressure.Read);
                _shader.SetTexture(_jacobiKernel, PressureOutId, _pressure.Write);
                Dispatch(_jacobiKernel);
                _pressure.Swap();
            }
        }

        private void SubtractPressureGradient()
        {
            _shader.SetTexture(_subtractGradientKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_subtractGradientKernel, StirId, _stir.Read);
            _shader.SetTexture(_subtractGradientKernel, PressureId, _pressure.Read);
            _shader.SetTexture(_subtractGradientKernel, VelocityOutId, _velocity.Write);
            Dispatch(_subtractGradientKernel);
            _velocity.Swap();
        }

        private void AdvectDye()
        {
            AdvectDyeInto(_dye.Read, _dyeForward, 1f);
            AdvectDyeInto(_dyeForward, _dyeBackward, -1f);

            _shader.SetTexture(_correctDyeKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_correctDyeKernel, StirId, _stir.Read);
            _shader.SetTexture(_correctDyeKernel, DyeId, _dye.Read);
            _shader.SetTexture(_correctDyeKernel, DyeForwardId, _dyeForward);
            _shader.SetTexture(_correctDyeKernel, DyeBackwardId, _dyeBackward);
            _shader.SetTexture(_correctDyeKernel, DyeOutId, _dye.Write);
            Dispatch(_correctDyeKernel);
            _dye.Swap();
        }

        private void AdvectDyeInto(RenderTexture source, RenderTexture destination, float advectionSign)
        {
            _shader.SetFloat(AdvectionSignId, advectionSign);
            _shader.SetTexture(_advectDyeKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_advectDyeKernel, StirId, _stir.Read);
            _shader.SetTexture(_advectDyeKernel, DyeId, source);
            _shader.SetTexture(_advectDyeKernel, DyeOutId, destination);
            Dispatch(_advectDyeKernel);
        }

        private void AdvectSink(float deltaTime)
        {
            var sinkFraction = _settings.MilkSinkFraction;
            _shader.SetFloat(SinkFractionId, sinkFraction);
            _shader.SetTexture(_advectSinkKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_advectSinkKernel, StirId, _stir.Read);
            _shader.SetTexture(_advectSinkKernel, SinkId, _sink.Read);
            _shader.SetTexture(_advectSinkKernel, SinkOutId, _sink.Write);
            Dispatch(_advectSinkKernel);
            _sink.Swap();

            var sunkVolume = sinkFraction * _inflowRate * _inflowMass * deltaTime;
            _sinkMass = _sinkMass * _sinkRetention + sunkVolume;
            _sunkMilkVolume += sunkVolume;
        }

        private void AdvectFoamCoords(float deltaTime)
        {
            var refreshTime = _settings.FoamRefreshTime;
            var previousTime = _foamTime;
            _foamTime += deltaTime;
            var resetsFirstLayer = !_hasFoamCoords || EntersNewCycle(previousTime, _foamTime, 0f, refreshTime);
            var resetsSecondLayer = !_hasFoamCoords || EntersNewCycle(previousTime, _foamTime, 0.5f, refreshTime);
            _hasFoamCoords = true;

            _shader.SetVector(FoamResetMaskId, new Vector2(resetsFirstLayer ? 1f : 0f, resetsSecondLayer ? 1f : 0f));
            _shader.SetTexture(_advectFoamCoordsKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_advectFoamCoordsKernel, StirId, _stir.Read);
            _shader.SetTexture(_advectFoamCoordsKernel, FoamCoordsId, _foamCoords.Read);
            _shader.SetTexture(_advectFoamCoordsKernel, FoamCoordsOutId, _foamCoords.Write);
            Dispatch(_advectFoamCoordsKernel);
            _foamCoords.Swap();
        }

        private static bool EntersNewCycle(float previousTime, float time, float cycleOffset, float cycleTime)
        {
            return Mathf.Floor(previousTime / cycleTime + cycleOffset) != Mathf.Floor(time / cycleTime + cycleOffset);
        }

        private void AdvectVelocity()
        {
            _shader.SetFloat(VelocityDampingId, _settings.VelocityDamping);
            _shader.SetTexture(_advectVelocityKernel, VelocityId, _velocity.Read);
            _shader.SetTexture(_advectVelocityKernel, StirId, _stir.Read);
            _shader.SetTexture(_advectVelocityKernel, VelocityOutId, _velocity.Write);
            Dispatch(_advectVelocityKernel);
            _velocity.Swap();
        }

        private void SettleStir(float deltaTime)
        {
            _shader.SetFloat(StirRetentionId, Mathf.Exp(-deltaTime / _settings.StirSettleTime));
            _shader.SetTexture(_settleStirKernel, StirId, _stir.Read);
            _shader.SetTexture(_settleStirKernel, StirOutId, _stir.Write);
            Dispatch(_settleStirKernel);
            _stir.Swap();
        }

        private void Dispatch(int kernel)
        {
            _shader.Dispatch(kernel, _threadGroups, _threadGroups, 1);
        }
    }
}
