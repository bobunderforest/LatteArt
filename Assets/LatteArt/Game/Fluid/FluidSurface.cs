using UnityEngine;

namespace LatteArt.Fluid
{
    [DefaultExecutionOrder(AfterToolsExecutionOrder)]
    public sealed class FluidSurface : MonoBehaviour
    {
        private const int AfterToolsExecutionOrder = 100;

        private static readonly int DyeTextureId = Shader.PropertyToID("_DyeTex");
        private static readonly int FoamCoordsTextureId = Shader.PropertyToID("_FoamCoordsTex");
        private static readonly int FoamPhaseId = Shader.PropertyToID("_FoamPhase");
        private static readonly int CoffeeDilutionId = Shader.PropertyToID("_CoffeeDilution");
        private static readonly int CupRadiusId = Shader.PropertyToID("_CupRadius");
        private static readonly int SimulationTexelSizeId = Shader.PropertyToID("_SimulationTexelSize");
        private static readonly int BrushCenterId = Shader.PropertyToID("_BrushCenter");
        private static readonly int BrushRadiusId = Shader.PropertyToID("_BrushRadius");
        private static readonly Vector2 CupCenterUv = new(0.5f, 0.5f);

        [Header("Simulation")]
        [SerializeField] private FluidSettings settings;
        [SerializeField] private ComputeShader simulationShader;

        [Header("Rendering")]
        [SerializeField] private Renderer surfaceRenderer;

        private FluidSimulation _simulation;
        private MaterialPropertyBlock _surfaceProperties;
        private float _unsimulatedTime;
        private Vector2 _brushCenter;
        private float _brushRadius;

        public FluidSimulation Simulation => _simulation;

        public bool TryGetSurfaceUv(Ray ray, out Vector2 uv)
        {
            uv = default;
            if (_simulation == null) return false;

            var surfaceTransform = surfaceRenderer.transform;
            var surfacePlane = new Plane(surfaceTransform.forward, surfaceTransform.position);
            if (!surfacePlane.Raycast(ray, out var distance)) return false;

            var localPoint = surfaceTransform.InverseTransformPoint(ray.GetPoint(distance));
            uv = new Vector2(localPoint.x + 0.5f, localPoint.y + 0.5f);
            return Vector2.Distance(uv, CupCenterUv) <= _simulation.CupRadius;
        }

        public void ShowBrush(Vector2 uv, float radius)
        {
            _brushCenter = uv;
            _brushRadius = radius;
        }

        public void HideBrush()
        {
            _brushRadius = 0f;
        }

        public void ResetSimulation()
        {
            if (_simulation == null) return;

            _simulation.Dispose();
            _simulation = new FluidSimulation(simulationShader, settings);
            _unsimulatedTime = 0f;
        }

        private void Awake()
        {
            if (!HasValidSetup())
            {
                enabled = false;
                return;
            }

            _simulation = new FluidSimulation(simulationShader, settings);
            _surfaceProperties = new MaterialPropertyBlock();
        }

        private void Update()
        {
            var stepDuration = 1f / settings.StepsPerSecond;
            var maxCatchUpTime = stepDuration * settings.MaxStepsPerFrame;
            _unsimulatedTime = Mathf.Min(_unsimulatedTime + Time.deltaTime, maxCatchUpTime);

            while (_unsimulatedTime >= stepDuration)
            {
                _simulation.Step(stepDuration);
                _unsimulatedTime -= stepDuration;
            }

            _simulation.ExecuteRecordedCommands();
        }

        private void LateUpdate()
        {
            UpdateSurfaceMaterial();
        }

        private void OnDestroy()
        {
            _simulation?.Dispose();
        }

        private bool HasValidSetup()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("Fluid simulation needs compute shader support.", this);
                return false;
            }

            if (settings == null || simulationShader == null || surfaceRenderer == null)
            {
                Debug.LogError("FluidSurface has unassigned references.", this);
                return false;
            }

            return true;
        }

        private void UpdateSurfaceMaterial()
        {
            surfaceRenderer.GetPropertyBlock(_surfaceProperties);
            _surfaceProperties.SetTexture(DyeTextureId, _simulation.Dye);
            _surfaceProperties.SetTexture(FoamCoordsTextureId, _simulation.FoamCoords);
            _surfaceProperties.SetFloat(FoamPhaseId, _simulation.FoamPhase);
            _surfaceProperties.SetFloat(CoffeeDilutionId, _simulation.CoffeeDilution);
            _surfaceProperties.SetFloat(CupRadiusId, _simulation.CupRadius);
            _surfaceProperties.SetFloat(SimulationTexelSizeId, _simulation.TexelSize);
            _surfaceProperties.SetVector(BrushCenterId, _brushCenter);
            _surfaceProperties.SetFloat(BrushRadiusId, _brushRadius);
            surfaceRenderer.SetPropertyBlock(_surfaceProperties);
        }
    }
}
