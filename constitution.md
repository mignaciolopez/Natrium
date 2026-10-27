# Project Constitution: Natrium
An MMORPG in the style of Argentum Online: 2D, top-down, tile-based, server-authoritative.

## Tech Stack
versions "must be same or higher match on packages-lock.json"
- **Engine:** Unity 6 (6000.7), C#
- **Runtime architecture:** DOTS / ECS (com.unity.entities 6.7.0), Burst, Collections, Mathematics
- **Networking:** Netcode for Entities via the CyberEntt Netcode package (`com.cyberentt.netcode` 7.1.0 )
- **Rendering / 2D:** Universal Render Pipeline 17.7, Tilemap and Physics2D modules
- **Input:** Unity Input System 6.7
- **UI:** UGUI (`com.unity.ugui` 2.7.0)
- **Shared packages:** CyberEntt Core, Logging, Netcode, Resources, Sound, Tools
- **Persistence:** Filesystem with CyberEntt.LoadSave systems. Will need to extend its functionality.

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
- Separate Server and Client assemblies
- Client-supplied strings must never reach file paths.
    - LoadSave builds paths from identifiers, so a character id sent by the client is a path-traversal risk.
    - Fix: parse as GUID and rebuild the path server-side.


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
- Before modifying the CyberEntity-Packages repository (shared code, separate repo)

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
- Namespace: `Natrium.<Area>` (for example `Natrium.Combat`, `Natrium.Inventory`).
- One primary type per file, with the file name matching the type.
- Max function length: 200 lines.
- Max file length: 1000 lines. Applies to Natrium code and to any new code added to packages. Existing files are exempt until modified.
- Existing package code is grandfathered until touched.
- Specify folders and asmdefs per area (Natrium.Shared, .Server, .Client).

## Error Handling Standards
- Never swallow exceptions silently. Log them with `Log.Error` including context.
- Tokens and PII must never be logged
- Use an enum or ushort on the wire. Clients map the code to localized text. Drop Message from the wire.
- Error categories:
  - Validation (rejected client input): return the specific error code. No stack trace is sent.
  - Not found (missing entity, item, or account): return the specific error code. Except authentication.
  - Unexpected (server fault): log the full exception with `Log.Error`. Send the client a generic code only.
- Exceptions must not cross the network boundary.

## Testing Requirements
- Minimum 80% code coverage for gameplay and server logic. Measured with the Unity Code Coverage package (`com.unity.testtools.codecoverage`, already in the manifest).
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

## Decisions
Resolved decisions that shape the rest of this document. Changing any of them is an ASK FIRST item.
1. **Persistence:** filesystem, using the CyberEntt LoadSave systems (`com.cyberentt.tools`). LoadSave will be extended for Natrium. See US-009 in `spec.md`.
2. **Spec:** `spec.md` is the Natrium spec.
3. **Coverage tooling:** Unity Code Coverage is installed (see the manifest).
4. **File length limit:** 500 lines, for Natrium code and for new functionality added to packages.
5. **Server authority:** server-authoritative. Client-side prediction applies only to the local player's movement; other entities are interpolated. Damage, death, and similar mechanics are never predicted.
