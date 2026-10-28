# Project Constitution: Natrium
An MMORPG in the style of Argentum Online: 2D, top-down, tile-based, server-authoritative.

## Tech Stack
versions "must be same or higher match on packages-lock.json"
- **Engine:** Unity 6 (6000.7), C#
- **Runtime architecture:** DOTS / ECS (com.unity.entities 6.7.0), Burst, Collections, Mathematics
- **Networking:** Netcode for Entities via the CyberEntt Netcode package (`com.cyberentt.netcode` 0.1.0 )
- **Rendering / 2D:** Universal Render Pipeline 17.7, Tilemap and Physics2D modules
- **Input:** Unity Input System 6.7
- **UI:** UGUI (`com.unity.ugui` 2.7.0)
- **Shared packages:** CyberEntt Core, Logging, Netcode, Resources, Sound, Tools
- **Persistence:** Filesystem with CyberEntt.LoadSave systems. Will need to extend its functionality.
- **Logging:** CyberEntt.Logging, extended with file and audit sinks (see `spec.md` Section 4, Logging extensions).

## Architecture Principles
- Layered runtime: Authoring (MonoBehaviour, edit-time) → Baker → ECS data (components) → Systems (behavior) → Netcode transport
- Server-authoritative: the server owns all game state. Clients send requests; the server validates, simulates, and broadcasts results.
  - Client-side prediction applies only to the local player's movement.
  - Other entities are interpolated.
  - Damage, death, and other mechanics are never predicted; they wait for server authorization.
- Data-oriented: components hold data only; logic lives in systems and jobs.
- Single Responsibility Principle for every system, job, and class.
- Reuse CyberEntt packages (Logging, Netcode, Tools) before writing new infrastructure.
- Extend CyberEntt packages (Netcode, Tools) with generic functionality.
- Persistence, network transport, and other external services sit behind interfaces so they can be faked in tests.
- Server code and client code live in separate assemblies. See Project Structure and Assemblies.
- Client-supplied strings must never reach file paths. Identifiers are parsed as GUIDs and paths are rebuilt server-side. LoadSave builds paths from identifiers, so an unchecked character id is a path-traversal risk.


## Boundaries
### ✅ ALWAYS DO:
- Write tests for all gameplay logic before finalizing a task
- Follow `conventions.md` for naming (Microsoft C# standard plus the domain suffixes)
- Log through `CyberEntt.Logging.Log` (`Log.Verbose`, `Log.Debug`, `Log.Info`, `Log.Warning`, `Log.Error`), not `Debug.Log`
- Add XML `<summary>` comments to all public types and members
- Default to ISystem with Burst. Use SystemBase only for managed access such as UI or file I/O.
- Every ISystem and IJobEntity gets [BurstCompile] unless it touches managed types
- Dispose every `NativeContainer` you allocate (`OnDestroy` or the owning system's dispose path)
- Use Allman braces, 4-space indentation, and CRLF line endings (matches the CyberEntt packages)
- Add CHANGELOG entry for existing packages

### ⚠️ ASK FIRST:
- Before adding any new third-party package or library (including any package outside `Packages/manifest.json`)
- Before choosing or changing the persistence technology
- Before modifying the persisted save format (LoadSave file layout, serialized components, or save version)
- Before adding new persistent components, networked messages, or RPCs
- Before changing the network protocol, message formats, or the server endpoint (the default is `0.0.0.0:7979` in `ServerSystem`)
- Before changing architectural patterns, including the server-authority model
- Before modifying the CyberEntity-Packages repository (shared code, separate repo). Pre-approved: the LoadSave and Logging extensions listed in `spec.md` Section 4. Each still needs tests, a CHANGELOG entry, and a version bump.

### 🚫 NEVER DO:
- Never hardcode secrets, API keys, connection strings, or credentials in code or scenes (file outside the repo, added to .gitignore.)
- Never skip writing tests for gameplay logic
- Never trust client-provided state (position, damage, gold, item counts, cooldowns) without server validation
- Never store passwords in plain text. Store salted hashes only.
- Never overwrite a save file in place. Write to a temporary file, then replace the original.
- Never allocate managed objects in per-frame hot paths (ECS systems and jobs), except logging strings.
- Never commit generated or cache folders (`Library/`, `obj/`, `Temp/`, `Logs/`, `UserSettings/`)
- Never add features not defined in the Natrium spec (see `spec.md`).
- A spec change needs a Decision Log entry before any code.
- Never reference `Natrium.Server` from `Natrium.Client` or `Natrium.Shared`, or `Natrium.Client` from `Natrium.Server` or `Natrium.Shared`.

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
- Descriptive names, allow loop counters and conventional terms such as ecb and dt, and give tile coordinates explicit names like tileX.
- Namespace mirrors the assembly: `Natrium.Shared.<Area>`, `Natrium.Server.<Area>`, `Natrium.Client.<Area>` (for example `Natrium.Server.Combat`).
- One primary type per file, with the file name matching the type.
- Max function length: 200 lines.
- Max file length: 1000 lines. Applies to Natrium code and to any new code added to packages. Existing files are exempt until modified.
- Existing package code is grandfathered until touched.
- Folders and assemblies follow Project Structure and Assemblies.

## Project Structure and Assemblies
Natrium code lives under `Assets/Natrium/`, split by side first and feature area second. This mirrors the Client, Server, and Shared folders of `com.cyberentt.netcode`.

```
Assets/Natrium/
  Shared/<Area>/{Components, Systems, Authoring}
  Server/<Area>/{Components, Systems, Authoring}
  Client/<Area>/{Components, Systems, Authoring, UI}
  Tests/{Shared, Server, Client}/{Editor, Runtime}
```

Assemblies (one `.asmdef` per side):
- `Natrium.Shared`: components, request/event/result messages, error codes, constants, and pure logic used by both sides. Contains no server-only data and no presentation code. References the CyberEntt packages, Entities, and Netcode.
- `Natrium.Server`: authoritative systems (validation, combat, sessions, authentication, persistence, audit) and server-only `...Properties` assets (loot tables, `SecurityProperties`). References `Natrium.Shared`. Define constraint: `UNITY_SERVER || UNITY_EDITOR`, so it is not compiled into player builds.
- `Natrium.Client`: input, UI, rendering, and the presentation side of prediction and interpolation. References `Natrium.Shared`. Define constraint: `!UNITY_SERVER || UNITY_EDITOR`.

Rules:
- Dependencies point one way: Client → Shared and Server → Shared. Client and Server never reference each other. Shared references neither.
- Server-only data (loot tables, hashing code, security settings, save code) lives under `Server/`. It is never placed in `Resources/`, Addressables, or a scene that ships with the client.
- An Authoring class and its Baker live in the assembly of the data they bake. Anything baked from server-only data is Server.
- Each assembly has test assemblies, `Natrium.<Side>.Tests` (play mode) and `Natrium.<Side>.Editor.Tests`, with the same define constraints as the assembly under test.
- A client build must not contain `Natrium.Server`. A build check lists the assemblies in the built player and fails if it is present (see Testing Requirements, CI gate).

## Error Handling Standards
- Never swallow exceptions silently. Log them with `Log.Error` including context.
- Tokens, passwords, password hashes, and PII must never be written to the general log. Chat text is not logged. The one exception is the audit log (see Security Standards), which may contain the client IP address.
- Use an enum or ushort on the wire. Clients map the code to localized text. Drop Message from the wire.
- Error categories:
  - Validation (rejected client input): return the specific error code. No stack trace is sent.
  - Not found (missing entity, item, or account): return the specific error code. Except authentication.
  - Unexpected (server fault): log the full exception with `Log.Error`. Send the client a generic code only.
- Exceptions must not cross the network boundary.

## Security Standards
Behavior and default numbers are defined in `spec.md` (US-010 to US-012, BR-009 to BR-015). The rules below apply to all code.

Authentication and sessions:
- An unauthenticated connection may send only `RegisterRequest` and `LoginRequest`. Every other request is rejected with `NOT_AUTHENTICATED` and counts toward that connection's limits.
- The account and session for a request come from the server's own connection-to-session mapping. They are never read from a request payload.
- Any identifier in a payload (character id, slot index, target) is checked against what the session owns before it is used.
- One active session per account. Duplicate login and reconnect follow BR-010 and BR-011.

Throttling and DoS:
- Every limit (login, registration, request rates, connection counts, queue sizes, timeouts) is a field in `SecurityProperties`, never a literal in code.
- Limits are enforced before any expensive work (password hashing, file I/O, simulation).
- Over-limit input is dropped or rejected, never queued without bound. Every queue has a fixed maximum size.
- Throttle counters are keyed on the submitted username whether or not the account exists, and responses are identical either way. Throttling must not reveal which accounts exist.
- Password hashing is CPU-expensive. The number of concurrent hashes is capped (BR-013).

Audit:
- Security-relevant events (the list in BR-014) go to the audit log through the Audit API, never through `Log.*`. Audit is not subject to the global log level, the environment presets, or compile-time stripping.
- Audit entries never contain passwords, hashes, tokens, or chat text.
- Audit and server log files are stored outside the repository and are listed in `.gitignore`.
- A failed audit write never stops the server. It is reported with `Log.Error` on the console.

## Testing Requirements
### Coverage
- Minimum 80% line coverage on `Natrium.Shared` and `Natrium.Server`. Measured with the Unity Code Coverage package (`com.unity.testtools.codecoverage`, in the manifest).
- Excluded from the threshold: `Natrium.Client` presentation and UI code, Authoring and Baker classes, samples, and generated code. Excluded code that contains logic still needs tests for that logic.
- Coverage runs are done with Burst compilation disabled, so that jobs and Burst systems are counted.
- Coverage does not replace the negative tests below. Meeting the threshold without rejection tests is incomplete.

### Test layers
1. Unit (EditMode): pure logic (combat math, inventory rules, validation, throttle counters, experience tables), written as static or struct functions with explicit inputs.
2. System (EditMode or PlayMode): create a `World`, add entities, run the system, assert on component data. One test class per system.
3. Integration (PlayMode): a server world and a client world in one process, connected over loopback. Covers login, sessions, movement with prediction and reconciliation, replication within view range, and disconnect and reconnect.
4. Persistence: see Required cases.
5. Load: a headless bot client runs against a server build. Phase 1 target: 100 bots with p99 tick time within the tick budget (spec Success Metrics). Run before each milestone, not on every commit.

### Required cases
- Every Request type has negative tests for: not authenticated, character dead (where it applies), cooldown not elapsed, out-of-range or negative index and quantity, entity not owned by the session, over the rate limit, and malformed fields (empty, oversized).
- Authentication, throttling, and lockout (BR-009 to BR-013) are tested with a fake clock: limit reached, lockout expiry, identical responses for existing and non-existing usernames, duplicate login, and reconnect inside and outside the grace period.
- Persistence: a crash during a write leaves the previous save intact; a corrupted file is reported; an unknown save version is an error, not an empty character; a restart restores state; two saves of one character at once do not corrupt it.
- Audit: each listed event produces exactly one entry with the required fields; no entry contains a password, hash, or token; queue overflow writes a drop-summary entry; rotation and retention work.

### Determinism
- Gameplay random numbers come from `Unity.Mathematics.Random`, seeded per entity from a server-held seed. `UnityEngine.Random` and `System.Random` are not used in gameplay code.
- Gameplay time is the simulation tick (`NetworkTime`), never `DateTime` or wall-clock time. Session and throttle logic read time through an injectable clock.
- Tests set the seed and the tick explicitly, so the same inputs reproduce the same results.

### Conventions
- Files: `[Name]Test.cs`, in `Tests/Editor` or `Tests/Runtime` under the owning assembly's folder, with an `.asmdef` in each.
- Methods: `Method_Scenario_Expected`, with Arrange, Act, Assert sections.
- Fake external dependencies (persistence, transport, clock, log sinks) behind interfaces.
- Every bug fix starts with a failing test that reproduces the bug.

### Definition of done
A task is done when the tests are written and pass (EditMode and PlayMode), coverage is not below the threshold, there are no new compiler warnings, the CHANGELOG is updated for any package change, and the task is recorded in `progress.md`.

### CI gate
Before merge, a headless run (`Unity -batchmode -runTests`) executes the EditMode and PlayMode tests with coverage. A build check confirms that a client build contains no `Natrium.Server` assembly. The CI environment is an open decision (see Open Decisions).

## Network Standards
- Every network message is versioned. The handshake includes a protocol version, and mismatched clients are rejected with an error code.
- Naming:
  - Client → server requests: `[Action]Request` (for example `GoInGameRequest`, `MoveRequest`)
  - Server → client notifications: `[Action]Event` (for example `DamageEvent`)
  - Results: `[Action]Result`
- Every server response carries a result or error code (see Error Handling Standards).
- Messages carry only the data the client needs. Never send server-only state.
- Every message uses fixed-size fields only (for example `FixedString512Bytes`), so its maximum size is known.

## Decisions
Resolved decisions that shape the rest of this document. Changing any of them is an ASK FIRST item.
1. **Persistence:** filesystem, using the CyberEntt LoadSave systems (`com.cyberentt.tools`). LoadSave will be extended for Natrium. See US-009 in `spec.md`.
2. **Spec:** `spec.md` is the Natrium spec.
3. **Coverage tooling:** Unity Code Coverage is installed (see the manifest).
4. **Length limits:** functions 200 lines, files 1000 lines, for Natrium code and for new functionality added to packages.
5. **Server authority:** server-authoritative. Client-side prediction applies only to the local player's movement; other entities are interpolated. Damage, death, and similar mechanics are never predicted.
6. **Logging:** CyberEntt.Logging is extended with file and audit sinks. See `spec.md` Section 4.

## Open Decisions
1. **CI environment:** `Packages/manifest.json` references the CyberEntt packages by absolute path (`file:D:/Projects/...`). A CI machine or another developer needs the CyberEntity-Packages repository at the same path, or the manifest needs a different reference (git URL or relative path).
2. **Proposed numbers:** the limits, timeouts, grace periods, and retention periods in `spec.md` (Security and Logging defaults) are proposals until confirmed.
3. **Client IP in the audit log:** the audit log stores the client IP address as an exception to the no-PII rule. Confirm this, or switch to a keyed hash of the IP.
