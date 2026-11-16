# CLAUDE.md

## Response style

Each recommend once. No summary, no recap, no "what to do" part repeating steps. Ordered steps: one numbered list, not prose then list again.

## Project

Unity 6.3 (`6000.3.10f1`), URP, new Input System (`Assets/LatteArt/Input/InputSystem_Actions.inputactions`).
Code quality, clear structure, readable git history matter as much as feature.

## Commands

**No build, no batchmode by default.** User compile, run editor self. After code change, say what changed, stop.
Run Unity from command line only when asked direct.

**Never stage, commit, or push.** User always do `git add`/`commit`/`push` self. Run them only when user ask
direct in that message.

Unity editor: `"C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"`.

## Architecture

- Logic in plain C# class; `MonoBehaviour` thin adapter wiring Unity (input, transform, rendering) to logic.
- Config/tune data in `ScriptableObject` asset, not hardcoded, not loose fields spread over components.
- Composition over inheritance: small focused components, no deep `MonoBehaviour` hierarchy.
- No singleton manager by default. Pass dependency via serialized reference or constructor. Truly global
  service needed: say why in response first.
- Cross-system talk via C# `event`/`Action` or interface, not `FindObjectOfType` / `GameObject.Find` /
  `SendMessage`.
- Input via Input System action asset (generated C# class or `PlayerInput`), never legacy `Input.GetKey`.
- **No** shared abstraction by guess: wait for real second consumer.
- Prefab-first: objects belonging together (station with its tools, pivots, anchors; cup with surface,
  highlight, colliders) suggest grouping into one prefab, owner component on root. Parts inside found by
  lookup (see Fields and serialization), only outside-world dependency left as scene override. Shared look
  across variants: base prefab + Prefab Variants, not copy.

## C# codestyle

Standard Microsoft C# convention unless rule below differ.

**Naming**

- `PascalCase` type, method, property, event, public member; `camelCase` serialized field (`[SerializeField]`,
  no underscore, Rider "Unity serialized field" rule); `_camelCase` other private field; `camelCase` local,
  parameter. Serialized name must not hide inherited `Component` member (`camera`, `renderer`): pick
  descriptive name (`viewCamera`). `I` prefix interface. Bool name read as condition (`isGrounded`, `hasTarget`).
- One public type per file, file name = type name.
- Namespace match feature folder under `Assets/LatteArt/Game/` (root namespace `LatteArt`, so
  `Game/Fluid/` is `LatteArt.Fluid`).
- Shaders (HLSL) follow Unity URP ShaderLibrary style, not Rider C++ defaults: `camelCase` local, parameter;
  `PascalCase` function, struct; `_PascalCase` property, uniform, texture; `UPPER_SNAKE` macro;
  `PascalCase` `static const`.

**Comments**

- **Write no comment.** Not on field, block, or class. Not "prefer fewer" with exception list — default zero.
- Intent in code instead: name method for what it do, named variable over explained expression, split branch
  needing heading, bool named for condition. Line seem to need comment = signal code should change.
- `TODO:` only exception — mark work not existing yet, can't be said in code.
- Non-local *why* (execution order, Unity lifecycle quirk, serialization gotcha): still no comment by default.
  Say it in response, user decide if it earn one.
- No XML doc comment (`///`) unless asked.
- Only on specific ask ("comment this") write comment.
- No adding comments to existing code while editing, no "documenting" change just made — diff be record.

**Fields and serialization**

- `[SerializeField] private` for inspector field, never `public` field just for inspector. Read access via
  property (`public float Speed => _speed;`) only when others need it.
- Group inspector fields with `[Header("...")]`; `[Min]`, `[Range]` where valid range exist.
- Init field inline at declare. `readonly` where value fixed after construct.
- Component reference lookup, pick by where dependency live. Look up once in `Awake`, cache in `_camelCase`
  field, never in hot path:
  - Same GameObject, mandatory: `[RequireComponent(typeof(T))]` + `GetComponent<T>()`. No serialized field,
    no null check (Unity guarantee it).
  - Descendant inside same prefab, exactly one of its type: `GetComponentInChildren<T>(true)` (`true` find
    inactive child). Validate in `Awake`, `Debug.LogError` with context if missing. Not when prefab can hold
    several of same type — first match silent pick.
  - Scene object outside prefab, spawn prefab, plain `Transform` anchor, or one of several same-type
    candidates: serialized inspector field.
- Rename serialized field with `[FormerlySerializedAs]` so scene/prefab data survive.

**Methods and lifecycle**

- Guard clause over nesting. One-line `if (x == null) return;` fine for simple guard.
- `Awake` self-init, `OnEnable`/`OnDisable` subscribe/unsubscribe (always paired), `Start` for init needing
  other objects.
- No allocation, LINQ, `GetComponent`, `Find`, string concat in `Update`/`FixedUpdate`/hot path. Physics in
  `FixedUpdate`, camera follow in `LateUpdate`.
- Remove empty Unity message methods (`Update() {}`) — still cost call.
- `var` when type obvious from right side; `const`/`readonly` wherever value fixed.
- No `async void`; prefer coroutine or `Awaitable` (Unity 6), handle cancel on destroy.
- Never `?.` / `??` on `UnityEngine.Object` — Unity override `==` null; use explicit `== null` or implicit bool.

**Diagnostics**

- `Debug.Assert` / `Debug.LogError` with context object (`Debug.LogError("msg", this)`) for invariants.
- No `Debug.Log` spam in committed code.

## Conventions

- All project content under `Assets/LatteArt/`: `Game/`, `Input/`, `Scenes/`, `Settings/`.
  Third-party assets outside `LatteArt`.
- Game feature = one subfolder `Game/<Feature>/` (`Game/Fluid/`, `Game/Pour/`) holding all its content: scripts,
  shaders, materials, `SO_` assets, prefabs, textures, audio. No split by asset type.
- Assembly Definition per area (`LatteArt.Runtime`, `LatteArt.Editor`); editor-only code in `Editor/` folder,
  own asmdef.
- Asset prefix: `PF_` prefab, `SO_` ScriptableObject asset, `M_` material, `T_` texture, `SFX_` audio,
  `SC_` scene.
- Every asset has `.meta` — commit together; move/rename assets only in Unity editor so GUID survive.
- `.unity`, `.prefab`, `.asset` YAML but hand-edit risky (fileID/GUID references). Scene, prefab, inspector
  changes done in editor by user — say what to set, don't edit file.
