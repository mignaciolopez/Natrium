# Natrium Product Specification
## 1. Overview
**Problem Statement:** Argentum Online uses very old technology.
**Target Users:** Anyone interested in Argentum Online. Tierras del Sur. Noland Studios. ImperiumAO.
**Phase 1 Goal:** A vertical slice of the core loop: log in, move, fight, loot, chat. It ships with one race (Human), one class (Cleric), and one map.
**Success Metrics:**
- A player can go from launch to standing in the world in under 30 seconds
- 100 concurrent players on one server instance within the server tick budget (target, to be measured)
- Zero character data loss on server restart or crash
- No client-reported value is accepted without server validation

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
- At 0 hit points the character is dead and cannot move, attack, or use items
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

## 3. Business Rules
### BR-001: Server Authority
**Given** any gameplay action (move, attack, pick up, use, chat)
**When** the client sends a request
**Then** the server validates it and applies it, or rejects it with an error code
**And** the client never sets position, hit points, damage, experience, or items directly
### BR-002: Movement and Prediction
**Given** a living character with no active move cooldown
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
**When** requests exceed that type's allowed rate
**Then** excess requests are rejected with `RATE_LIMITED`
**And** repeated abuse is logged with `Log.Warning`
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

## 4. Data Model (Conceptual)
Persisted components must be unmanaged `IComponentData` registered with the LoadSave `Registry`. Content values (stats, tables, items) live in `...Properties` assets, not code. Names follow `conventions.md`.
### ECS
**Components (with fields):**
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
**Tags (no fields):**
- `PlayerTag`, `NpcTag`, `HumanTag`, `ClericTag`
**Flags (enableable, no fields):**
- `DeadFlag`, `SaveEntityFlag` (LoadSave)
**Properties assets (ScriptableObjects):**
- `RaceProperties` (Human), `ClassProperties` (Cleric), `ItemProperties`, `NpcProperties`, `ExperienceTableProperties`, `MapProperties` (size, walkability, respawn point, NPC spawns)
### Persisted data (not ECS)
- **Account:** id (GUID), username, password hash and salt, created timestamp, character ids. Stored in its own file. LoadSave handles components, not accounts, so this needs its own writer.
- **Character:** one LoadSave file per character GUID, holding the persisted components above.
### LoadSave extensions required
The current package does not meet US-009 as is. These changes are needed, and each needs approval under the constitution's ASK FIRST rules before they touch the shared package:
- Choose the save path per character GUID (it currently writes `Save_<timestamp>.json` on every save)
- Write atomically (temp file, then replace)
- Run file I/O off the main thread
- Fail loudly on a version mismatch (it currently skips the load silently)

## 5. Network Contract
Transport is Netcode for Entities (see the constitution's Network Standards). The handshake carries the protocol version. Server responses carry a result or an error with `Code`, `Message`, and `Tick`.
### Client → Server
- `RegisterRequest` (username, password)
- `LoginRequest` (username, password, protocol version)
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
- `ErrorResult` (code, message, tick)
Position and state replication uses Netcode ghosts: the local player's ghost is predicted, all others are interpolated. Replication is limited by view range (BR-007).
### Error Codes
- `INVALID_CREDENTIALS`, `USERNAME_TAKEN`, `NAME_TAKEN`
- `PROTOCOL_MISMATCH`
- `INVALID_INPUT`
- `MOVE_BLOCKED`, `MOVE_TOO_FAST`
- `INVALID_TARGET`, `ATTACK_TOO_FAST`
- `INVENTORY_FULL`, `REQUIREMENTS_NOT_MET`, `ITEM_NOT_FOUND`
- `CHARACTER_DEAD`
- `RATE_LIMITED`
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
- ❌ Admin and game-master tools beyond server logs
- ❌ Mobile and console clients
- ❌ Private messages and global chat channels

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
### DEC-006: Maximum File Length
**Date:** 10/27/2026
**Decision:** 500 lines per file, for Natrium code and for new functionality added to packages.
**Rationale:** Matches the existing packages, where `Log.cs` already exceeds 300 lines.
