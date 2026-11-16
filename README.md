# Latte Art

A first-person café demo: make a coffee and draw latte art on a live fluid surface.

## Setup

1. Install [Git LFS](https://git-lfs.com) before cloning; audio and the reflection probe are stored in LFS.
   Already cloned without it? Run `git lfs install`, then `git lfs pull`.
2. Open the project in Unity 6.3 (`6000.3.10f1`, URP).
3. Import the free [Stylized Low Poly Coffee Shop Props Pack – Free](https://assetstore.unity.com/packages/3d/props/stylized-low-poly-coffee-shop-props-pack-free-368786)
   via Package Manager > My Assets. It is not committed because the Asset Store license forbids redistribution.
   It must land in `Assets/Stylized Low Poly Coffee Shop Props Pack – Free/` with its original GUIDs, or meshes
   will be missing.
4. Open `Assets/LatteArt/Scenes/SC_Cafe.unity` and press Play.

## Architecture

Game code lives in `Assets/LatteArt/Game/`, one folder per feature, in the `LatteArt.Runtime` assembly.

- **Fluid** — GPU simulation on a compute shader: force and milk injection, pressure solve, and advection of
  velocity, dye and foam. `FluidSimulation` is plain C#; `FluidSurface` is the thin `MonoBehaviour` driving it.
  `FluidSnapshot` and `FrozenFluidSurface` keep the finished drawing on the served cup without simulating.
- **CoffeeServing** — `CoffeeStation` owns the drawing session: enter, reset, serve, leave. `CupTray` and
  `CoffeeMachine` handle the steps before drawing.
- **CoffeeTools** — tools implement `ISurfaceTool`. `ToolSelector` owns the slots, attaches tools to the
  surface and keeps one enabled at a time. Tuning lives in `SO_` assets.
- **Player** — movement, interaction raycasts (`Interactor` + `IInteractable`), carrying and placing.
- **CameraUtils** — blends the camera between first-person and station views.
- **Hints** — UI Toolkit panels. `HintPanel` is a pure view over `HintEntry` data. `StaticHints` and
  `StationHints` build entries from their owners' input actions, so bindings are defined once.
- **Audio** — listeners play sounds on gameplay events.
- **Compatibility** — `ComputeSupportNotice` tells players when their browser can't run compute shaders.

The Web build targets WebGPU, because the fluid simulation needs compute shaders. WebGL 2 stays as a fallback that
shows the notice.

### Principles

- Logic in plain C#; `MonoBehaviour`s are thin adapters.
- Tuning in `ScriptableObject` assets, not hardcoded.
- Systems talk through events and interfaces. No singletons, no `Find`.
- Prefab-first: the station prefab holds its tools, anchors and pivots. Only outside dependencies, such as the
  camera rig, are scene overrides.
- Input goes through the action asset in `Assets/LatteArt/Input/`.
