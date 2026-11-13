using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace LatteArt.Fluid
{
    public sealed class FluidSimulation : IDisposable
    {
        private const int ThreadGroupSize = 8;
        private const int DivergenceSumStride = sizeof(float) * 2;
        private const float InactiveSourceRadius = 1f;
        private const float GridMarginTexels = 1.5f;

        private static readonly Vector2 CupCenterUv = new(0.5f, 0.5f);

        private static readonly int ResolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int ThreadGroupCountId = Shader.PropertyToID("_ThreadGroupCount");
        private static readonly int PartialSumCountId = Shader.PropertyToID("_PartialSumCount");
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
        private static readonly int BlowCenterId = Shader.PropertyToID("_BlowCenter");
        private static readonly int BlowRadiusId = Shader.PropertyToID("_BlowRadius");
        private static readonly int BlowRateId = Shader.PropertyToID("_BlowRate");
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
        private static readonly int DyeId = Shader.PropertyToID("_Dye");
        private static readonly int DyeForwardId = Shader.PropertyToID("_DyeForward");
        private static readonly int DyeBackwardId = Shader.PropertyToID("_DyeBackward");
        private static readonly int DyeOutId = Shader.PropertyToID("_DyeOut");
        private static readonly int PressureId = Shader.PropertyToID("_Pressure");
        private static readonly int PressureInOutId = Shader.PropertyToID("_PressureInOut");
        private static readonly int RedBlackParityId = Shader.PropertyToID("_RedBlackParity");
        private static readonly int RelaxationWeightId = Shader.PropertyToID("_RelaxationWeight");
        private static readonly int DivergenceId = Shader.PropertyToID("_Divergence");
        private static readonly int DivergenceOutId = Shader.PropertyToID("_DivergenceOut");
        private static readonly int PartialSumsId = Shader.PropertyToID("_PartialSums");
        private static readonly int PartialSumsOutId = Shader.PropertyToID("_PartialSumsOut");
        private static readonly int DivergenceTotalId = Shader.PropertyToID("_DivergenceTotal");
        private static readonly int DivergenceTotalOutId = Shader.PropertyToID("_DivergenceTotalOut");

        private readonly ComputeShader _shader;
        private readonly FluidSettings _settings;
        private readonly int _resolution;
        private readonly int _threadGroups;
        private readonly int _halfRowThreadGroups;
        private readonly CommandBuffer _commands = new() { name = "FluidSimulation" };

        private readonly int _addForceKernel;
        private readonly int _stirKernel;
        private readonly int _settleStirKernel;
        private readonly int _injectMilkKernel;
        private readonly int _divergenceKernel;
        private readonly int _sumDivergenceKernel;
        private readonly int _totalDivergenceKernel;
        private readonly int _relaxPressureKernel;
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
        private readonly RenderTexture _pressure;
        private readonly RenderTexture _divergence;
        private readonly RenderTexture _dyeForward;
        private readonly RenderTexture _dyeBackward;
        private readonly GraphicsBuffer _divergencePartialSums;
        private readonly GraphicsBuffer _divergenceTotal;

        private Vector2 _inflowCenter;
        private float _inflowRadius = InactiveSourceRadius;
        private float _inflowRate;
        private Vector2 _blowCenter;
        private float _blowRadius = InactiveSourceRadius;
        private float _blowRate;
        private float _sunkMilkVolume;
        private float _foamTime;
        private bool _hasFoamCoords;

        public FluidSimulation(ComputeShader shader, FluidSettings settings)
        {
            _shader = shader;
            _settings = settings;
            _resolution = settings.Resolution;
            _threadGroups = Mathf.CeilToInt(_resolution / (float)ThreadGroupSize);
            _halfRowThreadGroups = Mathf.CeilToInt(_resolution / (2f * ThreadGroupSize));

            _addForceKernel = shader.FindKernel("AddForce");
            _stirKernel = shader.FindKernel("Stir");
            _settleStirKernel = shader.FindKernel("SettleStir");
            _injectMilkKernel = shader.FindKernel("InjectMilk");
            _divergenceKernel = shader.FindKernel("Divergence");
            _sumDivergenceKernel = shader.FindKernel("SumDivergence");
            _totalDivergenceKernel = shader.FindKernel("TotalDivergence");
            _relaxPressureKernel = shader.FindKernel("RelaxPressure");
            _subtractGradientKernel = shader.FindKernel("SubtractGradient");
            _advectVelocityKernel = shader.FindKernel("AdvectVelocity");
            _advectDyeKernel = shader.FindKernel("AdvectDye");
            _advectSinkKernel = shader.FindKernel("AdvectSink");
            _correctDyeKernel = shader.FindKernel("CorrectDye");
            _advectFoamCoordsKernel = shader.FindKernel("AdvectFoamCoords");

            _velocity = new PingPongTexture(_resolution, GraphicsFormat.R16G16_SFloat, "FluidVelocity");
            _stir = new PingPongTexture(_resolution, GraphicsFormat.R16G16_SFloat, "FluidStir");
            var sinkFormat = FluidTexture.FilterableFormat(GraphicsFormat.R32_SFloat, GraphicsFormat.R16_SFloat);
            var foamCoordsFormat = FluidTexture.FilterableFormat(
                GraphicsFormat.R32G32B32A32_SFloat, GraphicsFormat.R16G16B16A16_SFloat);
            _sink = new PingPongTexture(_resolution, sinkFormat, "FluidSink");
            _dye = new PingPongTexture(_resolution, GraphicsFormat.R16_SFloat, "FluidDye");
            _foamCoords = new PingPongTexture(_resolution, foamCoordsFormat, "FluidFoamCoords");
            _pressure = FluidTexture.Create(_resolution, GraphicsFormat.R32_SFloat, "FluidPressure");
            _divergence = FluidTexture.Create(_resolution, GraphicsFormat.R32_SFloat, "FluidDivergence");
            _dyeForward = FluidTexture.Create(_resolution, GraphicsFormat.R16_SFloat, "FluidDyeForward");
            _dyeBackward = FluidTexture.Create(_resolution, GraphicsFormat.R16_SFloat, "FluidDyeBackward");
            _divergencePartialSums = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured, _threadGroups * _threadGroups, DivergenceSumStride);
            _divergenceTotal = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, DivergenceSumStride);
        }

        public RenderTexture Dye => _dye.Read;
        public RenderTexture FoamCoords => _foamCoords.Read;
        public float FoamPhase => Mathf.Repeat(_foamTime / _settings.FoamRefreshTime, 1f);
        public float TexelSize => 1f / _resolution;
        public float CupRadius => Mathf.Min(_settings.CupRadius, 0.5f - GridMarginTexels / _resolution);
        public float CoffeeDilution => Mathf.Clamp01(_sunkMilkVolume / (CupArea * _settings.FullDilutionVolume));

        private float CupArea => Mathf.PI * CupRadius * CupRadius;

        public void AddForce(Vector2 fromUv, Vector2 toUv, Vector2 velocity, float radius)
        {
            SetGridParameters();
            SetVector(ForceFromId, fromUv);
            SetVector(ForceToId, toUv);
            SetVector(ForceVelocityId, velocity);
            SetFloat(ForceRadiusId, radius);

            SetTexture(_addForceKernel, VelocityId, _velocity.Read);
            SetTexture(_addForceKernel, VelocityOutId, _velocity.Write);
            Dispatch(_addForceKernel);
            _velocity.Swap();
        }

        public void Stir(Vector2 fromUv, Vector2 toUv, Vector2 velocity, float radius, float coupling)
        {
            SetGridParameters();
            SetVector(StirFromId, fromUv);
            SetVector(StirToId, toUv);
            SetVector(StirVelocityId, velocity);
            SetFloat(StirRadiusId, radius);
            SetFloat(StirCouplingId, coupling);

            SetTexture(_stirKernel, StirId, _stir.Read);
            SetTexture(_stirKernel, StirOutId, _stir.Write);
            Dispatch(_stirKernel);
            _stir.Swap();
        }

        public void InjectMilk(Vector2 fromUv, Vector2 toUv, float radius)
        {
            SetGridParameters();
            SetVector(MilkFromId, fromUv);
            SetVector(MilkToId, toUv);
            SetFloat(MilkRadiusId, radius);

            SetTexture(_injectMilkKernel, DyeId, _dye.Read);
            SetTexture(_injectMilkKernel, DyeOutId, _dye.Write);
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
            _inflowRadius = InactiveSourceRadius;
            _inflowRate = 0f;
        }

        public void SetBlow(Vector2 uv, float radius, float rate)
        {
            _blowCenter = uv;
            _blowRadius = radius;
            _blowRate = rate;
        }

        public void StopBlow()
        {
            _blowRadius = InactiveSourceRadius;
            _blowRate = 0f;
        }

        public void Step(float deltaTime)
        {
            SetGridParameters();
            SetFloat(DeltaTimeId, deltaTime);
            SetFloat(SinkRetentionId, Mathf.Exp(-deltaTime / _settings.MilkSinkTime));

            ProjectVelocity();
            AdvectDye();
            AdvectSink(deltaTime);
            AdvectFoamCoords(deltaTime);
            AdvectVelocity();
            SettleStir(deltaTime);
        }

        public void ExecuteRecordedCommands()
        {
            Graphics.ExecuteCommandBuffer(_commands);
            _commands.Clear();
        }

        public void Dispose()
        {
            _commands.Release();
            _velocity.Dispose();
            _stir.Dispose();
            _sink.Dispose();
            _dye.Dispose();
            _foamCoords.Dispose();
            FluidTexture.Release(_pressure);
            FluidTexture.Release(_divergence);
            FluidTexture.Release(_dyeForward);
            FluidTexture.Release(_dyeBackward);
            _divergencePartialSums.Release();
            _divergenceTotal.Release();
        }

        private void SetGridParameters()
        {
            SetInt(ResolutionId, _resolution);
            SetFloat(TexelSizeId, 1f / _resolution);
            SetFloat(CupRadiusId, CupRadius);
        }

        private void ProjectVelocity()
        {
            ComputeDivergence();
            SumDivergence();
            SolvePressure();
            SubtractPressureGradient();
        }

        private void ComputeDivergence()
        {
            SetVector(InflowCenterId, _inflowCenter);
            SetFloat(InflowRadiusId, _inflowRadius);
            SetFloat(InflowRateId, _inflowRate);
            SetVector(BlowCenterId, _blowCenter);
            SetFloat(BlowRadiusId, _blowRadius);
            SetFloat(BlowRateId, _blowRate);

            SetTexture(_divergenceKernel, VelocityId, _velocity.Read);
            SetTexture(_divergenceKernel, StirId, _stir.Read);
            SetTexture(_divergenceKernel, SinkId, _sink.Read);
            SetTexture(_divergenceKernel, DivergenceOutId, _divergence);
            Dispatch(_divergenceKernel);
        }

        private void SumDivergence()
        {
            SetInt(ThreadGroupCountId, _threadGroups);
            SetInt(PartialSumCountId, _threadGroups * _threadGroups);

            SetTexture(_sumDivergenceKernel, DivergenceId, _divergence);
            SetBuffer(_sumDivergenceKernel, PartialSumsOutId, _divergencePartialSums);
            Dispatch(_sumDivergenceKernel);

            SetBuffer(_totalDivergenceKernel, PartialSumsId, _divergencePartialSums);
            SetBuffer(_totalDivergenceKernel, DivergenceTotalOutId, _divergenceTotal);
            _commands.DispatchCompute(_shader, _totalDivergenceKernel, 1, 1, 1);
        }

        private void SolvePressure()
        {
            SetFloat(RelaxationWeightId, _settings.PressureRelaxation);
            SetTexture(_relaxPressureKernel, DivergenceId, _divergence);
            SetBuffer(_relaxPressureKernel, DivergenceTotalId, _divergenceTotal);
            SetTexture(_relaxPressureKernel, PressureInOutId, _pressure);
            for (var i = 0; i < _settings.PressureIterations; i++)
            {
                RelaxPressureCells(0);
                RelaxPressureCells(1);
            }
        }

        private void RelaxPressureCells(int redBlackParity)
        {
            SetInt(RedBlackParityId, redBlackParity);
            _commands.DispatchCompute(_shader, _relaxPressureKernel, _halfRowThreadGroups, _threadGroups, 1);
        }

        private void SubtractPressureGradient()
        {
            SetTexture(_subtractGradientKernel, VelocityId, _velocity.Read);
            SetTexture(_subtractGradientKernel, StirId, _stir.Read);
            SetTexture(_subtractGradientKernel, PressureId, _pressure);
            SetTexture(_subtractGradientKernel, VelocityOutId, _velocity.Write);
            Dispatch(_subtractGradientKernel);
            _velocity.Swap();
        }

        private void AdvectDye()
        {
            AdvectDyeInto(_dye.Read, _dyeForward, 1f);
            AdvectDyeInto(_dyeForward, _dyeBackward, -1f);

            SetTexture(_correctDyeKernel, VelocityId, _velocity.Read);
            SetTexture(_correctDyeKernel, StirId, _stir.Read);
            SetTexture(_correctDyeKernel, DyeId, _dye.Read);
            SetTexture(_correctDyeKernel, DyeForwardId, _dyeForward);
            SetTexture(_correctDyeKernel, DyeBackwardId, _dyeBackward);
            SetTexture(_correctDyeKernel, DyeOutId, _dye.Write);
            Dispatch(_correctDyeKernel);
            _dye.Swap();
        }

        private void AdvectDyeInto(RenderTexture source, RenderTexture destination, float advectionSign)
        {
            SetFloat(AdvectionSignId, advectionSign);
            SetTexture(_advectDyeKernel, VelocityId, _velocity.Read);
            SetTexture(_advectDyeKernel, StirId, _stir.Read);
            SetTexture(_advectDyeKernel, DyeId, source);
            SetTexture(_advectDyeKernel, DyeOutId, destination);
            Dispatch(_advectDyeKernel);
        }

        private void AdvectSink(float deltaTime)
        {
            var sinkFraction = _settings.MilkSinkFraction;
            SetFloat(SinkFractionId, sinkFraction);
            SetTexture(_advectSinkKernel, VelocityId, _velocity.Read);
            SetTexture(_advectSinkKernel, StirId, _stir.Read);
            SetTexture(_advectSinkKernel, SinkId, _sink.Read);
            SetTexture(_advectSinkKernel, SinkOutId, _sink.Write);
            Dispatch(_advectSinkKernel);
            _sink.Swap();

            if (_inflowRate <= 0f) return;

            var inflowMass = GaussianSource.MassInsideCircle(_inflowCenter, _inflowRadius, CupCenterUv, CupRadius);
            var inflowVolume = _inflowRate * inflowMass * deltaTime;
            _sunkMilkVolume += sinkFraction * inflowVolume;
        }

        private void AdvectFoamCoords(float deltaTime)
        {
            var refreshTime = _settings.FoamRefreshTime;
            var previousTime = _foamTime;
            _foamTime += deltaTime;
            var resetsFirstLayer = !_hasFoamCoords || EntersNewCycle(previousTime, _foamTime, 0f, refreshTime);
            var resetsSecondLayer = !_hasFoamCoords || EntersNewCycle(previousTime, _foamTime, 0.5f, refreshTime);
            _hasFoamCoords = true;

            SetVector(FoamResetMaskId, new Vector2(resetsFirstLayer ? 1f : 0f, resetsSecondLayer ? 1f : 0f));
            SetTexture(_advectFoamCoordsKernel, VelocityId, _velocity.Read);
            SetTexture(_advectFoamCoordsKernel, StirId, _stir.Read);
            SetTexture(_advectFoamCoordsKernel, FoamCoordsId, _foamCoords.Read);
            SetTexture(_advectFoamCoordsKernel, FoamCoordsOutId, _foamCoords.Write);
            Dispatch(_advectFoamCoordsKernel);
            _foamCoords.Swap();
        }

        private static bool EntersNewCycle(float previousTime, float time, float cycleOffset, float cycleTime)
        {
            return Mathf.Floor(previousTime / cycleTime + cycleOffset) != Mathf.Floor(time / cycleTime + cycleOffset);
        }

        private void AdvectVelocity()
        {
            SetFloat(VelocityDampingId, _settings.VelocityDamping);
            SetTexture(_advectVelocityKernel, VelocityId, _velocity.Read);
            SetTexture(_advectVelocityKernel, StirId, _stir.Read);
            SetTexture(_advectVelocityKernel, VelocityOutId, _velocity.Write);
            Dispatch(_advectVelocityKernel);
            _velocity.Swap();
        }

        private void SettleStir(float deltaTime)
        {
            SetFloat(StirRetentionId, Mathf.Exp(-deltaTime / _settings.StirSettleTime));
            SetTexture(_settleStirKernel, StirId, _stir.Read);
            SetTexture(_settleStirKernel, StirOutId, _stir.Write);
            Dispatch(_settleStirKernel);
            _stir.Swap();
        }

        private void SetInt(int id, int value)
        {
            _commands.SetComputeIntParam(_shader, id, value);
        }

        private void SetFloat(int id, float value)
        {
            _commands.SetComputeFloatParam(_shader, id, value);
        }

        private void SetVector(int id, Vector2 value)
        {
            _commands.SetComputeVectorParam(_shader, id, value);
        }

        private void SetTexture(int kernel, int id, RenderTexture texture)
        {
            _commands.SetComputeTextureParam(_shader, kernel, id, texture);
        }

        private void SetBuffer(int kernel, int id, GraphicsBuffer buffer)
        {
            _commands.SetComputeBufferParam(_shader, kernel, id, buffer);
        }

        private void Dispatch(int kernel)
        {
            _commands.DispatchCompute(_shader, kernel, _threadGroups, _threadGroups, 1);
        }
    }
}
