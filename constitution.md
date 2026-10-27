# Project Constitution: Natrium
An MMORPG in the style of Argentum Online: 2D, top-down, tile-based, server-authoritative.

## Tech Stack
- **Engine:** Unity 6 (6000.7), C#
- **Runtime architecture:** DOTS / ECS (com.unity.entities 7.1.0), Burst, Collections, Mathematics
- **Networking:** Netcode for Entities via the CyberEntt Netcode package (`com.cyberentt.netcode`)
- **Rendering / 2D:** Universal Render Pipeline 17.7, Tilemap and Physics2D modules
- **Input:** Unity Input System 6.7
- **UI:** UGUI (`com.unity.ugui` 2.7.0)
- **Shared packages:** CyberEntt Core, Logging, Netcode, Resources, Sound, Tools
- **Persistence:** Filesystem with CyberEntt.LoadSave systems. Will need to extend its functionality.

## Architecture Principles
- Layered runtime: Authoring (MonoBehaviour, edit-time) → Baker → ECS data (components) → Systems (behavior) → Netcode transport
- Server-authoritative: the server owns all game state. Clients send requests; the server validates, simulates, and broadcasts results, using client side prediction on movement.
- Data-oriented: components hold data only; logic lives in systems and jobs.
- Single Responsibility Principle for every system, job, and class.
- Dependency injection where applicable. In ECS, prefer passing dependencies through singletons or system parameters over static access.
- Reuse CyberEntt packages (Logging, Netcode, Core) before writing new infrastructure.
- Persistence, network transport, and other external services sit behind interfaces so they can be faked in tests.

## Boundaries
### ✅ ALWAYS DO:
- Write tests for all gameplay logic before finalizing a task
- Follow `conventions.md` for naming (Microsoft C# standard plus the domain suffixes)
- Log through `CyberEntt.Logging.Log` (`Log.Info`, `Log.Warning`, `Log.Error`), not `Debug.Log`
- Add XML `<summary>` comments to all public types and members
- Validate every client input on the server before acting on it
- Use `[BurstCompile]` on Burst-eligible jobs and structs
- Dispose every `NativeContainer` you allocate (`OnDestroy` or the owning system's dispose path)
- Use async/await for I/O (persistence, external services). Never block the main thread on I/O.
- Use Allman braces, 4-space indentation, and CRLF line endings (matches the CyberEntt packages)

### ⚠️ ASK FIRST:
- Before adding any new third-party package or library (including any package outside `Packages/manifest.json`)
- Before choosing or changing the persistence technology
- Before modifying the database schema or the persisted save format
- Before adding new persistent components, networked messages, or RPCs
- Before changing the network protocol, message formats, or the server endpoint (the default is `0.0.0.0:7979` in `ServerSystem`)
- Before changing architectural patterns, including the server-authority model
- Before modifying the CyberEntity-Packages repository (shared code, separate repo)

### 🚫 NEVER DO:
- Never hardcode secrets, API keys, connection strings, or credentials in code or scenes
- Never skip writing tests for gameplay logic
- Never trust client-provided state (position, damage, gold, item counts, cooldowns) without server validation
- Never store passwords in plain text. Store salted hashes only.
- Never use `SELECT *` in persistence queries
- Never allocate managed objects in per-frame hot paths (ECS systems and jobs)
- Never commit generated or cache folders (`Library/`, `obj/`, `Temp/`, `Logs/`, `UserSettings/`)
- Never add features not defined in the Natrium spec (see `spec.md`)

## Code Style
- Naming follows `conventions.md`: Microsoft C# rules, plus these domain suffixes:
  - Data components with no fields: `...Tag`
  - Enableable components with no fields: `...Flag`
  - Job structs: `...Job`
  - Systems: `...System`
  - Authoring scripts: `...Authoring`, containing a private `Baker` class
  - ScriptableObjects: `...Properties`
  - Tests: `[Name]Test.cs` (matches `RuntimeExampleTest.cs` / `EditorExampleTest.cs` in the packages)
- Methods and public members: PascalCase. Locals and parameters: camelCase. Private fields: `_camelCase`. Where this template and `conventions.md` differ, `conventions.md` wins.
- Descriptive names, minimum 3 characters.
- Namespace: `Natrium.<Area>` (for example `Natrium.Combat`, `Natrium.Inventory`).
- One primary type per file, with the file name matching the type.
- Max function length: 50 lines.
- Max file length: 500 lines. Note: the existing `Log.cs` in CyberEntt.Logging is 568 lines. This limit applies to new Natrium code. And code for New functionality added to packages.

## Error Handling Standards
- Never swallow exceptions silently. Log them with `Log.Error` including context.
- Errors returned to clients use this shape (a network message, not HTTP):
  - `Code`: a constant from one central `ErrorCodes` class (for example `INVENTORY_FULL`, `INVALID_TARGET`)
  - `Message`: a human-readable string
  - `Tick`: the server simulation tick when the error occurred
- Error categories:
  - Validation (rejected client input): return the specific error code. No stack trace is sent.
  - Not found (missing entity, item, or account): return the specific error code.
  - Unexpected (server fault): log the full exception with `Log.Error`. Send the client a generic code only.
- Exceptions must not cross the network boundary.
- Log timestamps are ISO-8601 UTC.

## Testing Requirements
- Minimum 80% code coverage for gameplay and server logic. The Unity Code Coverage package is not yet in the manifest (see Open Decisions).
- Test file naming: `[Name]Test.cs`, in `Tests/Runtime` (play-mode) or `Tests/Editor` (edit-mode) of the owning package or assembly, with an `.asmdef` in each folder.
- Use AAA pattern: Arrange, Act, Assert.
- Test pure logic (combat math, inventory rules, validation) in edit-mode tests where possible.
- Fake external dependencies (persistence, transport, time) behind interfaces.
- Network and simulation logic must be deterministic for a given seed and input sequence, so tests can reproduce results.

## Network Standards
- Every network message is versioned. The handshake includes a protocol version, and mismatched clients are rejected with an error code.
- Naming:
  - Client → server requests: `[Action]Request` (for example `GoInGameRequest`, `MoveRequest`)
  - Server → client notifications: `[Action]Event` (for example `DamageEvent`)
  - Results: `[Action]Result`
- Every server response carries a result or error code (see Error Handling Standards).
- Messages carry only the data the client needs. Never send server-only state.

## Open Decisions
These are not decided. Each one needs an answer before the related work begins.
1. **Persistence:** which database and hosting model (for example SQLite for development, PostgreSQL in production). Blocks US-009 in `spec.md`.
   answer: Filesystem with CyberEntt.LoadSave systems. Will need to extend its functionality.
2. **Spec confirmation:** `spec.md` holds the Natrium Phase 1 spec. Its decisions are marked "Proposed" until confirmed.
3. **Coverage tooling:** add the Unity Code Coverage package to measure the 80% target.
    Done. Is Installed. Check Manifest.
4. **File length limit:** confirm whether the 300-line limit applies to CyberEntt packages or only Natrium code.
    500 lines for both.
5. **Server authority model:** confirm server-authoritative, with or without client prediction.
    with client side prediction only for movement on the local player, others will be interpolated. Mechanics like death or damage can not be predicted they are subject to server authorization.



