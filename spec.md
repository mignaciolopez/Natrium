# Natrium Product Specification
## 1. Overview
**Problem Statement:** Argentum Online uses very old technology.
**Target Users:** Anyone interested in Argentum Online. Tierras del Sur. Noland Studios. ImperiumAO.
**Phase 1 Goal:** A vertical slice of the core loop: log in, move, fight, loot, chat. It ships with one race (Human), one class (Cleric), and one map.
**Success Metrics:**
- A player can go from launch to standing in the world in under 30 seconds
- 100 concurrent players on one server instance within the server tick budget (target, to be measured with the bot load test)
- Zero character data loss on server restart or crash
- No client-reported value is accepted without server validation
- Every security-relevant event is recorded in the audit log

## 2. User Stories
### US-001: Create Account and Log In
**As a** player
**I want** to register an account and log in
**So that** my characters are saved and only I can use them
**Acceptance Criteria:**
- Username is required (3-20 chars: letters, digits, underscore), unique case-insensitive
- Password is required (8-64 chars) and is stored only as a salted hash
- Client and server protocol versions must match, otherwise login is rejected with `PROTOCOL_MISMATCH`
- Wrong credentials return a generic `INVALID_CREDENTIALS` (never reveal which field was wrong)
- Failed logins and registrations are throttled (BR-012)
### US-002: Create Character
**As a** player
**I want** to create a character with a name
**So that** I can start playing
**Acceptance Criteria:**
- Name is required (3-16 chars), unique across the server, case-insensitive
- Race and class are fixed in Phase 1: Human Cleric. There is no race or class selection screen.
- Starting stats come from the Human and Cleric data assets, assigned by the server
- An account holds up to 3 characters
### US-003: Enter the World and Move
**As a** player
**I want** to move my character tile by tile
**So that** I can explore the world
**Acceptance Criteria:**
- Movement is on a tile grid in 4 directions
- The local player's movement is predicted on the client and reconciled with the server
- Other players and NPCs are interpolated, not predicted
- The server rejects blocked tiles (walls, water, other entities) and moves sent faster than the move cooldown
- Entities within view range are visible and update in near real time
- When player is in a death state can freely move, and has lower netcode priority.
### US-004: Chat
**As a** player
**I want** to talk to nearby players
**So that** I can communicate with them
**Acceptance Criteria:**
- Local chat is delivered to players within view range
- Messages are 1-160 chars; empty or whitespace-only messages are rejected
- Chat is rate-limited per character
### US-005: Fight NPCs
**As a** player
**I want** to attack hostile NPCs
**So that** I can gain experience and loot
**Acceptance Criteria:**
- Melee attack targets the tile the character is facing. The Cleric uses a blunt weapon in Phase 1 (no spells).
- Hit chance and damage are computed on the server from stats and equipped weapon
- Combat is never predicted. The client shows results only after the server confirms them.
- The server enforces an attack cooldown
- NPCs have hit points and respawn after a delay
- Hits and misses are shown to nearby players
- Killing an NPC grants experience to the killer and may drop items
### US-006: Inventory and Equipment
**As a** player
**I want** to pick up, carry, drop, use, and equip items
**So that** I can progress and improve my character
**Acceptance Criteria:**
- Inventory has 20 slots; stackable items stack up to their max stack size
- Pick up and drop act on the tile in front of the character
- Weapon and armor can be equipped; level and stat requirements are enforced
- Consumables (for example a health potion) can be used; each use consumes one item
- A full inventory rejects pick up with `INVENTORY_FULL`
### US-007: Leveling
**As a** player
**I want** to gain levels from experience
**So that** my character becomes stronger
**Acceptance Criteria:**
- Experience per level comes from a data asset, not hardcoded values
- Level up increases maximum hit points and base stats according to the Cleric asset
- The level cap for Phase 1 is 20
### US-008: Death and Respawn
**As a** player
**I want** a clear outcome when my character dies
**So that** combat has stakes
**Acceptance Criteria:**
- At 0 hit points the character is dead and cannot attack or use items. A dead character can still move (see US-003).
- Death is never predicted. It is shown only after the server confirms it.
- The player can respawn at the map's respawn point after a delay
- Items are not lost in Phase 1
### US-009: Persistent Characters
**As a** player
**I want** my character saved
**So that** I continue where I left off
**Acceptance Criteria:**
- Character state is saved on logout, on disconnect, and periodically, using the CyberEntt LoadSave systems
- Each character is stored in its own file, keyed by its GUID
- Saves are written to a temporary file and then replaced, so a crash never leaves a half-written save
- Saves run off the main thread; a slow disk never stalls the server tick
- A save failure is logged with `Log.Error` and retried; the player is never silently reverted
- A save file with an unknown version is reported as an error and never loaded as an empty character
- Server restart restores every character to its last saved state
### US-010: Sessions, Duplicate Login, and Reconnect
**As a** player
**I want** my login tied to one connection, and a way back into my character after a dropped connection
**So that** nobody else can act as me, and a bad connection does not cost me my character
**Acceptance Criteria:**
- A successful `LoginRequest` creates a server-side session bound to that connection. The session holds the account id and its state.
- Only one session per account exists. A second valid login replaces the first (BR-010).
- The player can log out with `LogoutRequest`. The character is saved and removed from the world, and the connection returns to the login state.
- When a connection drops, the character is saved. If it was in combat recently, it stays in the world for a short grace period, and a login during that period resumes the same character (BR-011).
- Reconnecting is a normal login. No session token is stored on the client. The client keeps the password in memory for the running app only and never writes it to disk.
- A connection that does not log in within the login timeout is disconnected.
- On server shutdown, all characters are saved and clients receive `SessionEndedEvent` with reason `SERVER_SHUTDOWN`.
### US-011: Brute-Force and Abuse Protection
**As a** server operator
**I want** logins, registrations, and requests limited
**So that** passwords cannot be guessed and the server cannot be flooded
**Acceptance Criteria:**
- Failed logins are throttled per username and per IP address (BR-012)
- Registrations are limited per IP address (BR-012)
- Limits on connections, request rates, message sizes, queue sizes, and concurrent password hashing apply (BR-013)
- Throttled responses do not reveal whether an account exists
- Every limit is a field in `SecurityProperties` and can change without a code change
- Throttling and limit events are written to the audit log
### US-012: Audit and Persistent Logs
**As a** server operator
**I want** a durable record of security and gameplay-critical events, and of the server log
**So that** I can investigate abuse, duplicated items, and crashes after the fact
**Acceptance Criteria:**
- Audit entries are written for the events listed in BR-014, as JSON Lines, in files separate from the general server log
- The general server log is also written to a file, with ISO-8601 UTC timestamps
- Both kinds of file rotate daily and by size, and files older than the retention period are deleted
- Audit entries are not filtered by log level or by the Production preset
- Writes happen off the main thread, so a slow disk does not stall the server tick
- On a normal shutdown, all queued entries are flushed. After a crash, at most the last flush interval of entries can be lost.
- The required changes to CyberEntt.Logging are listed in Section 4

## 3. Business Rules
### BR-001: Server Authority
**Given** any gameplay action (move, attack, pick up, use, chat)
**When** the client sends a request
**Then** the server validates it and applies it, or rejects it with an error code
**And** the client never sets position, hit points, damage, experience, or items directly
### BR-002: Movement and Prediction
**Given** a character (living or dead) with no active move cooldown
**When** a `MoveRequest` arrives for an adjacent, walkable, unoccupied tile
**Then** the server moves the character and starts the move cooldown
**And** otherwise rejects it with `MOVE_BLOCKED` or `MOVE_TOO_FAST`
**And** a rejected move makes the client snap the predicted local player back to the server position
### BR-003: Combat Resolution
**Given** an attacker in range with no attack cooldown
**When** an `AttackRequest` arrives
**Then** the server rolls hit chance, then damage, with the server-side random source
**And** damage is never below 0 and hit points never go below 0
**And** the same seed and inputs produce the same result (deterministic for tests)
### BR-004: Death
**Given** a character or NPC reaches 0 hit points
**When** the server processes the damage
**Then** it is marked dead exactly once and credited to one killer
**And** further damage to a dead entity is ignored
### BR-005: Inventory Integrity
**Given** any inventory change (pick up, drop, use, equip)
**When** the server applies it
**Then** the change is atomic: it fully succeeds or leaves the inventory unchanged
**And** items are never duplicated or lost
### BR-006: Rate Limiting
**Given** any client request type
**When** requests exceed that type's allowed rate (see BR-013 and `SecurityProperties`)
**Then** excess requests are rejected with `RATE_LIMITED`
**And** repeated abuse is recorded in the audit log (BR-014)
### BR-007: View Range
**Given** an entity changes (moves, takes damage, dies)
**When** the server replicates the change
**Then** only clients within that entity's view range receive it
**And** server-only data (hidden stats, loot tables) is never replicated
### BR-008: Save Integrity
**Given** a character is saved
**When** the write completes
**Then** the previous save is replaced atomically
**And** if the write fails, the previous save is untouched and the failure is logged
### BR-009: Session Binding
**Given** an authenticated session on a connection
**When** any request arrives on that connection
**Then** the server takes the account and session from its own connection-to-session mapping
**And** any account id or session id in the payload is ignored
**And** a request naming a character, slot, or target that the session does not own is rejected with `NOT_AUTHORIZED`
**And** a request other than `RegisterRequest` or `LoginRequest` from an unauthenticated connection is rejected with `NOT_AUTHENTICATED`
### BR-010: Duplicate Login
**Given** an account with an active session
**When** a valid `LoginRequest` for that account arrives on another connection
**Then** the server saves the character, removes it from the world, sends `SessionEndedEvent` with reason `REPLACED` to the old connection, and disconnects it
**And** the new session enters the world only after that save completes, so one character never exists twice
**And** a login with a wrong password never affects the existing session
### BR-011: Disconnect and Reconnect
**Given** an authenticated connection drops
**When** the server detects the disconnect
**Then** a character that was in combat within `ReconnectGraceSeconds` stays in the world as a lingering character until the grace period ends; any other character is saved and removed immediately
**And** a valid login for that account during the grace period reattaches to the same entity, without a second spawn
**And** when the grace period ends, the character is saved and removed
### BR-012: Login and Registration Throttling
**Given** failed login attempts (wrong password, or unknown username)
**When** the failures reach a limit in `SecurityProperties` (per username, per IP address, or per connection)
**Then** further attempts are rejected with `RATE_LIMITED` without checking the password
**And** counters are keyed on the submitted username whether or not the account exists
**And** a login for an unknown username does the same password-hash work as for a known one and returns the same code, so neither the response nor its timing reveals which accounts exist
**And** registrations beyond the per-IP limits are rejected with `RATE_LIMITED`
**And** every lockout is recorded in the audit log
Known trade-off: a username lockout lets an attacker lock a victim out for up to the lock duration. Phase 1 accepts this with short, capped lockouts (DEC-009). Counters live in server memory and reset on restart. Several players behind one IP address (for example an internet cafe) share the per-IP limits, so those defaults are generous.
### BR-013: Connection and Request Limits
**Given** any connection
**When** it exceeds a limit in `SecurityProperties` (connections, request rate, message size, queue size, pending password hashes, login timeout)
**Then** the server drops or rejects the excess without doing the requested work
**And** repeated violations disconnect the connection and block its IP address for `IpBlockMinutes`
**And** the server never queues unbounded work for a connection
**And** at most `MaxConcurrentHashes` password hashes run at once, with at most `MaxPendingLogins` waiting; further logins are rejected with `RATE_LIMITED`
### BR-014: Audit Events
**Given** any event in the audit list below
**When** it occurs
**Then** exactly one audit entry is written, with: timestamp (ISO-8601 UTC), server tick, sequence number, category, event name, account id and character id when known, source IP address (Account and Security events only), and a short detail field
**And** the entry never contains a password, a hash, a token, or chat text
Audit list:
- **Account:** RegisterSucceeded, RegisterFailed, LoginSucceeded, LoginFailed, Logout
- **Security:** LoginThrottled, IpBlocked, SessionReplaced, ProtocolMismatch, ConnectionRejected, RateLimitViolation (one entry per connection per 10 seconds, with a count), UnauthenticatedRequest (same aggregation)
- **Character:** CharacterCreated, EnterWorld, LeaveWorld (with reason), Disconnected, Reconnected
- **Gameplay:** LevelUp, Death, ItemAcquired, ItemDropped, ItemUsed, ItemEquipped
- **System:** ServerStarted, ServerStopping, SaveFailed, SaveRecovered, AuditDropped
### BR-015: Audit Delivery
**Given** an audit entry is produced, from the main thread or from Burst code
**When** the entry is enqueued
**Then** a background writer appends it to the current audit file
**And** if the queue is full, Gameplay entries are dropped before Security and Account entries, and an `AuditDropped` entry with the count is written when space frees up
**And** if the file cannot be written, the failure is reported with `Log.Error` on the console and the server keeps running

## 4. Data Model (Conceptual)
Persisted components must be unmanaged `IComponentData` registered with the LoadSave `Registry`. Content values (stats, tables, items) live in `...Properties` assets, not code. Names follow `conventions.md`. Types are split across the `Natrium.Shared`, `Natrium.Server`, and `Natrium.Client` assemblies (see the constitution's Project Structure and Assemblies).
### ECS
**Components (with fields), in Natrium.Shared:**
- `CharacterIdentity`: Guid (LoadSave), name
- `Level`: level, experience
- `Health`: current, max
- `Stats`: strength, agility, intelligence, constitution
- `TilePosition`: mapId, tileX, tileY; `Facing`: direction
- `MoveCooldown`, `AttackCooldown`: ready tick
- `InventorySlot` (dynamic buffer, 20 entries): itemId, quantity, equipped
- `NpcSpawn`: template id, tile, respawn seconds
- `NpcStats`: max hit points, damage range, hit chance, experience reward
- `ChatMessage` (request buffer): text
**Components (with fields), in Natrium.Server only, never replicated:**
- `Session`: account GUID, state (Authenticated | InWorld | Lingering), connection entity, character entity, linger end tick, last combat tick
- Throttle state (singleton with fixed-capacity native hash maps): failures per username and per IP address, registrations per IP address, lock expiry ticks
**Tags (no fields):**
- `PlayerTag`, `NpcTag`, `HumanTag`, `ClericTag`
**Flags (enableable, no fields):**
- `DeadFlag`, `SaveEntityFlag` (LoadSave)
**Properties assets (ScriptableObjects):**
- Shared: `RaceProperties` (Human), `ClassProperties` (Cleric), `ItemProperties`, `NpcProperties`, `ExperienceTableProperties`, `MapProperties` (size, walkability, respawn point, NPC spawns)
- Server only: `SecurityProperties` (limits below)
### SecurityProperties defaults (Proposed)
All values are fields in `SecurityProperties`. These are starting points to tune after the load test.
- **Connections:** `MaxConnections` 150; `MaxConnectionsPerIp` 5; `LoginTimeoutSeconds` 10; `MaxLoginAttemptsPerConnection` 3
- **Login throttling:** `UsernameFailureLimit` 5 within `UsernameFailureWindowMinutes` 15 locks the username for `UsernameLockMinutes` 5, doubling on each repeated lockout up to 30; the counter resets after a successful login or 24 hours without failures. `IpFailureLimit` 20 within `IpFailureWindowMinutes` 10 blocks login from that IP for `IpBlockMinutes` 15.
- **Registration:** `MaxRegistrationsPerIpPerHour` 3; `MaxRegisterAttemptsPerIpPerHour` 10
- **Request rates (per connection):** 60 requests per second overall; `MaxRequestsPerConnectionPerTick` 10 (the rest are dropped); chat 1 per second with a burst of 3; inventory requests 5 per second; move and attack at most 2x the rate their cooldowns allow
- **Violation escalation:** 50 `RATE_LIMITED` rejections within 10 seconds disconnect the connection and block its IP for `IpBlockMinutes`
- **Password hashing:** `MaxConcurrentHashes` 4; `MaxPendingLogins` 32
- **Message size:** every request uses fixed-size fields only. Chat text is limited to 160 characters and to the size of its `FixedString512Bytes`; anything longer is rejected.
- **Reconnect:** `ReconnectGraceSeconds` 15, applied only to characters in combat
- **Saves:** `MinSaveIntervalSeconds` 30 per character; `MaxPendingSaves` 64
- **Logs:** `AuditRetentionDays` 30; `LogRetentionDays` 14; `MaxLogFileMegabytes` 100; `AuditQueueCapacity` 10000; `AuditFlushIntervalMs` 1000; `LogDirectory` outside the repository (default: under `Application.persistentDataPath`)
### Persisted data (not ECS)
- **Account:** id (GUID), username, password hash and salt, created timestamp, character ids. Stored in its own file. LoadSave handles components, not accounts, so this needs its own writer.
- **Character:** one LoadSave file per character GUID, holding the persisted components above.
- **Audit and server log files:** `audit-YYYY-MM-DD.jsonl` and `server-YYYY-MM-DD.log` (UTC dates), with a numeric suffix when the size cap rolls a file.
### LoadSave extensions required
The current package does not meet US-009 as is. These changes are needed, and are pre-approved by the constitution:
- Choose the save path per character GUID (it currently writes `Save_<timestamp>.json` on every save)
- Write atomically (temp file, then replace)
- Run file I/O off the main thread
- Fail loudly on a version mismatch (it currently skips the load silently)
### Logging extensions required
CyberEntt.Logging does not meet US-012 as is. What it does today:
- Every message goes through `UnityEngine.Debug.Log*`; there is no other output and no file logging.
- The `Production` preset keeps only errors, and compile-time stripping removes disabled levels, so Info and Warning audit entries would be lost.
- Deferred (Burst) log entries are queued and can be lost if the application crashes, exits, or the world is disposed before they are flushed.

These changes are needed, and are pre-approved by the constitution. Each ships with tests, a CHANGELOG entry, and a version bump. Existing behavior stays the default.
- A sink abstraction (`ILogSink`) behind `Log`. The console sink keeps today's behavior and remains the default.
- A file sink: rolling by UTC date and by size, retention cleanup, ISO-8601 UTC timestamps, a background writer with a bounded queue, and a flush on shutdown and on world disposal.
- A minimum level per sink, in `SettingsProperties`, so the console can stay at Error in Production while the file keeps Info.
- An Audit channel that is exempt from the global level, the environment presets, and compile-time stripping.
- A Burst-compatible audit API that takes a blittable entry (category, event id, account GUID, character GUID, tick, `FixedString128Bytes` detail), plus a managed overload. Entries are queued with `BurstQueue` (CyberEntt.Core) and drained by a system.
- JSON Lines output for the audit sink.
- File and audit sinks are active only in server builds and in the editor (`UNITY_SERVER || UNITY_EDITOR`); clients log to the console only.
- Settings for the log directory, retention, size cap, queue capacity, and flush interval.

## 5. Network Contract
Transport is Netcode for Entities (see the constitution's Network Standards). The handshake carries the protocol version. Server responses carry a result or an error with `Code` and `Tick`.
### Client → Server
- `RegisterRequest` (username, password)
- `LoginRequest` (username, password, protocol version)
- `LogoutRequest`
- `CreateCharacterRequest` (name)
- `GoInGameRequest` (character id)
- `MoveRequest` (direction)
- `AttackRequest` (no payload; targets the facing tile)
- `PickUpRequest`, `DropRequest` (slot index, quantity)
- `UseItemRequest`, `EquipItemRequest` (slot index)
- `ChatRequest` (text)
- `RespawnRequest`
### Server → Client
- `LoginResult` (success or error code, character list)
- `CharacterCreatedResult`
- `GoInGameResult` (initial world state)
- `DamageEvent` (target, amount, hit or miss)
- `DeathEvent`, `RespawnEvent`
- `ChatEvent` (sender name, text)
- `InventoryChangedEvent`, `StatsChangedEvent` (owning client only)
- `SessionEndedEvent` (reason: `REPLACED`, `KICKED`, `SERVER_SHUTDOWN`, `TIMEOUT`)
- `ErrorResult` (code, tick)
Position and state replication uses Netcode ghosts: the local player's ghost is predicted, all others are interpolated. Replication is limited by view range (BR-007).
### Error Codes
- `INVALID_CREDENTIALS`, `USERNAME_TAKEN`, `NAME_TAKEN`
- `PROTOCOL_MISMATCH`
- `NOT_AUTHENTICATED`, `NOT_AUTHORIZED`
- `SERVER_FULL`
- `INVALID_INPUT`
- `MOVE_BLOCKED`, `MOVE_TOO_FAST`
- `INVALID_TARGET`, `ATTACK_TOO_FAST`
- `INVENTORY_FULL`, `REQUIREMENTS_NOT_MET`, `ITEM_NOT_FOUND`
- `CHARACTER_DEAD`
- `RATE_LIMITED` (also used for all login and registration throttling)
- `SERVER_ERROR` (generic; details only in server logs)

## 6. Out of Scope (Phase 1)
- ❌ Other races and classes (Phase 1 is Human Cleric only), and any race or class selection
- ❌ Spells, mana, and ranged combat
- ❌ PvP, guilds, parties, and friends lists
- ❌ Player-to-player trading and NPC merchants
- ❌ Skills, professions, and crafting
- ❌ Item loss or drops on death
- ❌ Multiple maps and map transitions (Phase 1 uses one map)
- ❌ Mounts, pets, and quests
- ❌ Admin and game-master tools beyond server logs and the audit log
- ❌ Mobile and console clients
- ❌ Private messages and global chat channels
- ❌ Email verification, password reset, and two-factor authentication
- ❌ Chat logging and moderation tools
- ❌ Remote log shipping, dashboards, and alerting

## 7. Decision Log
### DEC-001: Server Authority and Prediction
**Date:** 10/27/2026
**Decision:** The server owns all game state. Only the local player's movement is predicted; other entities are interpolated; combat and death are never predicted.
**Rationale:** Prevents cheating, the main risk in an MMO, while keeping movement responsive.
### DEC-002: Tile-Based Movement
**Date:** 10/27/2026
**Decision:** Movement is one tile per request, in 4 directions, with a server cooldown.
**Rationale:** Matches the Argentum Online feel and keeps validation simple.
### DEC-003: Data-Driven Content
**Date:** 10/27/2026
**Decision:** Items, NPCs, race, class, and experience tables live in `...Properties` assets.
**Rationale:** Content can be tuned and extended (more races and classes later) without code changes.
### DEC-004: Phase 1 Scope
**Date:** 10/27/2026
**Decision:** One map, one race (Human), one class (Cleric), melee only.
**Rationale:** Proves the core loop before adding content.
### DEC-005: Persistence on the Filesystem
**Date:** 10/27/2026
**Decision:** Characters are persisted as files through the CyberEntt LoadSave systems, one file per character. LoadSave is extended as listed in Section 4.
**Rationale:** Reuses existing code and needs no database server for Phase 1. Revisit if concurrent load or account volume outgrows file storage.
### DEC-006: Maximum Length
**Date:** 10/27/2026
**Decision:** Functions up to 200 lines and files up to 1000 lines, for Natrium code and for new functionality added to packages.
**Rationale:** Matches the existing packages, where `Log.cs` is 568 lines and `Manager.SaveMultipleEntities` is about 95 lines.
### DEC-007: Duplicate Login Replaces the Old Session
**Date:** 10/27/2026
**Status:** Proposed
**Decision:** A second valid login for an account saves and removes the first session's character and disconnects the old connection (BR-010).
**Rationale:** Rejecting the new login would lock players out while a dead connection waits for a timeout. Replacement needs a valid password, and the save-before-enter order prevents duplicated characters.
### DEC-008: Reconnect Is a Login
**Date:** 10/27/2026
**Status:** Proposed
**Decision:** There is no session token. A reconnect is a normal login that reattaches to a lingering character, and only characters in combat linger (15 seconds).
**Rationale:** Fewer secrets to store and steal on the client. Lingering only after combat stops players from escaping a fight by disconnecting, without keeping idle characters in the world.
### DEC-009: Throttling Keyed on Username, Not Account
**Date:** 10/27/2026
**Status:** Proposed
**Decision:** Failure counters key on the submitted username whether or not the account exists, with identical responses and equal hashing work. Lockouts are short (5 minutes, doubling to a 30-minute cap).
**Rationale:** Prevents account enumeration. The cost is that an attacker can lock a victim out for a short time. Accepted for Phase 1.
### DEC-010: Audit Is a Separate Channel, and May Contain the IP Address
**Date:** 10/27/2026
**Status:** Proposed
**Decision:** Audit entries bypass log levels and presets. Only the audit log may contain the client IP address, and it is deleted after 30 days.
**Rationale:** The Production preset would otherwise drop the events needed for investigations. IP addresses are needed to trace brute-force attacks. Alternative: store a keyed hash of the IP.
### DEC-011: Separate Server and Client Assemblies
**Date:** 10/27/2026
**Status:** Proposed
**Decision:** `Natrium.Shared`, `Natrium.Server`, and `Natrium.Client`, with define constraints so the server assembly is not compiled into player builds.
**Rationale:** Keeps loot tables, hashing, and security code out of the client, where it could be extracted.
