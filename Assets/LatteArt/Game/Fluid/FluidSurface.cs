using UnityEngine;

namespace LatteArt.Fluid
{
    [DefaultExecutionOrder(AfterToolsExecutionOrder)]
    [RequireComponent(typeof(Renderer))]
    public sealed class FluidSurface : MonoBehaviour
    {
        private const int AfterToolsExecutionOrder = 100;

        private static readonly Vector2 CupCenterUv = new(0.5f, 0.5f);

        [Header("Simulation")]
        [SerializeField] private FluidSettings settings;
        [SerializeField] private ComputeShader simulationShader;

        private FluidSimulation _simulation;
        private MaterialPropertyBlock _surfaceProperties;
        private Renderer _surfaceRenderer;
        private float _unsimulatedTime;
        private Vector2 _brushCenter;
        private float _brushRadius;

        public FluidSimulation Simulation => _simulation;

        public bool TryGetSurfaceUv(Ray ray, out Vector2 uv)
        {
            uv = default;
            if (_simulation == null) return false;

            var surfacePlane = new Plane(transform.forward, transform.position);
            if (!surfacePlane.Raycast(ray, out var distance)) return false;

            var localPoint = transform.InverseTransformPoint(ray.GetPoint(distance));
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

            _simulation.Clear();
            _unsimulatedTime = 0f;
        }

        public FluidSnapshot TakeSnapshot()
        {
            if (_simulation == null) return null;

            _simulation.ExecuteRecordedCommands();
            return new FluidSnapshot(_simulation);
        }

        private void Awake()
        {
            _surfaceRenderer = GetComponent<Renderer>();
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
            FluidSurfaceMaterial.Apply(_surfaceRenderer, _surfaceProperties, _simulation, _brushCenter, _brushRadius);
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

            if (settings == null || simulationShader == null)
            {
                Debug.LogError("FluidSurface has unassigned references.", this);
                return false;
            }

            return true;
        }
    }
}
