# WingNuts Unity Clone — Proof of Concept Plan

## Context

Building a single-level proof-of-concept clone of WingNuts: Temporal
Navigator in Unity. The project repo exists at
`/Users/chuck/Projects/education/unity/unity-wing-nuts` but contains no
Unity project files yet. The goal is a playable demo with:

- Scrolling ocean/island background
- Player plane (always-forward flight, steer with left/right arrows)
- Small and large enemy planes with simple AI
- Bullet system (player and enemy)
- Fuel system (drains over time, tanker plane refueling mechanic)
- Parachute drops (restore shields, fuel, or "rescued colleagues" = bonus
  score)
- Boss-style large plane at the end
- UI matching the original: fuel gauge (radial), shields indicator (radial
  around plane icon), shields count, fuel count, minimap

## World Model

The camera follows the player, keeping them centered on screen at all
times. The player never reaches a map edge — the Tilemap wraps in both
axes around them. When the player flies upward the world shifts downward;
when the player flies right the world shifts left. Enemy and pickup
positions are in world space and shift with the world on each wrap.

The Tilemap is a seamlessly looping map in both dimensions. When it wraps
in either axis, `BackgroundScroller` fires `OnMapWrapped(Vector2 offset)`
and all world objects shift by the same offset so they remain visually
consistent with the terrain beneath them.

**Layers:**

| Layer           | Used by                                     |
| --------------- | ------------------------------------------- |
| `Player`        | Player plane                                |
| `PlayerBullet`  | Bullets fired by the player                 |
| `Enemy`         | Enemy planes (small, large, boss)           |
| `EnemyBullet`   | Bullets fired by enemies                    |
| `Tanker`        | Tanker plane (friendly, no collisions)      |
| `Pickup`        | Parachute pickups                           |
| `Tilemap`       | Ocean and island Tilemap                    |
| `Minimap`       | Dot markers rendered only by minimap camera |
| `UI`            | Canvas elements                             |

**Layer collision matrix** (Physics 2D settings — only listed pairs
collide; all others are disabled):

| Layer          | Collides with |
| -------------- | ------------- |
| `PlayerBullet` | `Enemy`       |
| `EnemyBullet`  | `Player`      |
| `Pickup`       | `Player`      |
| `Enemy`        | `Player`      |

Enemy-to-player body contact deals 25 damage to the player (calls
`PlayerStats.TakeDamage(25)`) and destroys the enemy on contact.

**Main camera:** orthographic, follows player via smooth damp
(`Camera.main` or a dedicated `CameraFollow` component). Orthographic
size set to show a comfortable play area. The minimap camera is a second
orthographic camera with a much larger size, rendering to a RenderTexture.
The minimap camera's culling mask excludes `PlayerBullet` and `EnemyBullet`
so bullets do not appear as dots on the minimap.

## Unity Version

Use **Unity 6 LTS** (6000.x) — it's the current recommended LTS as of
2026 and has stable 2D tooling. The runtime fee concern from 2024 is
resolved for small/educational projects.

---

## Phase 1: Project Setup

1. Open Unity Hub, create a new **2D (URP)** project named `unity-wing-nuts`
   in the existing repo directory.
2. Confirm `ProjectSettings/ProjectVersion.txt` is committed.
3. Folder structure under `Assets/`:

   ```text
   Scripts/
     Player/
     Enemy/
     Pickups/
     UI/
     Core/
   Prefabs/       ← enemy, bullet, pickup, tanker, boss prefabs
   Sprites/
   Scenes/
   ```

---

## Phase 2: World Map

**Files:** `Assets/Scripts/Core/MapGenerator.cs`,
`Assets/Scripts/Core/BackgroundScroller.cs`

**Map generation (once at game start):**

- `MapGenerator` populates a Unity **Tilemap** with ocean tiles as the
  base, then stamps island tiles at random positions using a seeded RNG.
- The Tilemap is sized to be a multiple of the viewport in both dimensions
  (e.g. 4× tall, 4× wide) so that wrapping in either axis is never
  visually jarring — there is always terrain filling the screen on all
  sides as tiles shift.
- Islands are purely decorative — no collision with the player. Place
  8–12 islands per map, each 3–10 tiles in diameter, at random positions
  using the seeded RNG. The seed is chosen with
  `Random.Range(0, int.MaxValue)` at game start and logged to the Unity
  console so any layout is reproducible.
- The Tilemap is static after generation; nothing moves individual tiles.

**Camera follow (`Assets/Scripts/Core/CameraFollow.cs`):**

- Attached to Main Camera. Each `LateUpdate()` it moves the camera
  position to the player's position via `Vector3.SmoothDamp`, keeping Z
  fixed at -10. This keeps the player centered on screen at all times.

**Scrolling and wrapping:**

- The camera follows the player, keeping them centered on screen at all
  times. The player never reaches an edge — the Tilemap tiles shift around
  them in all directions to create the illusion of an infinite world.
- The Tilemap wraps in both axes. When the player moves right, tiles on
  the left side of the map shift to the right; when the player moves up,
  tiles at the bottom shift to the top. The Tilemap is sized to be a
  multiple of the viewport in both dimensions so the seam is never visible.
- `BackgroundScroller` tracks accumulated camera movement. When movement
  exceeds one full map width horizontally or one full map height
  vertically, it resets the Tilemap's position by that dimension and fires
  `OnMapWrapped(Vector2 offset)`.
- All world objects (enemies, pickups, tanker) subscribe to `OnMapWrapped`
  and shift their `transform.position` by the same offset to stay
  visually consistent with the terrain beneath them.
- The minimap camera renders the full Tilemap and all `Minimap`-layer
  dots at once. It is not parented to the player; its position is
  updated alongside the Tilemap so it always frames the full map.

---

## Phase 3: Art

All sprites are generated programmatically using deterministic C# code
(no external tools). A `SpriteGenerator` class is an Editor tool
(`[MenuItem("WingNuts/Generate Sprites")]`) that the developer runs once
from the Unity menu. It generates each sprite via `Texture2D` API calls
and saves the resulting `.png` assets to `Assets/Sprites/` using
`AssetDatabase`. The committed `.png` files are then referenced directly
in Inspector fields — no runtime generation occurs. This keeps builds
working while keeping art reproducible without external tools.

All art is created as 2D sprites in a WingNuts-inspired style. Sprites
are authored at a consistent scale and placed in `Assets/Sprites/`.

- **Player plane**: top-down view of a small yellow biplane with visible
  wings, fuselage, and tail.
- **Small enemy plane**: top-down red biplane, slightly smaller than the
  player.
- **Large enemy plane**: top-down orange biplane, noticeably larger and
  bulkier than the small enemy.
- **Boss plane**: top-down, very large orange biplane that dominates the
  screen.
- **Tanker plane**: top-down large green or white plane with a visible
  fuel hose/drogue cone at the tail marking the docking point.
- **Bullets**: small elongated oval, color-coded by source (yellow for
  player, red for enemies).
- **Shields pickup**: a parachute canopy with a first aid kit suspended
  below.
- **Fuel pickup**: a parachute canopy with a fuel barrel suspended below.
- **Colleague rescue pickup**: a parachute canopy with a soldier in
  uniform suspended below, hands raised.
- **Ocean background**: tiled deep-blue water texture.
- **Island tiles**: green landmass tiles with terrain detail (trees,
  rocks) to stamp onto the ocean tilemap.

**Animations (sprite sheets, used by `Animator`):**

- **Player crash**: 4-frame sheet — intact → smoking → wing detached →
  explosion flash.
- **Enemy/boss explosion**: 4-frame sheet — flash → fireball → smoke →
  clear. Same sheet reused for all enemy types.
- **Tanker banking away**: 3-frame sheet — level → gentle bank → steep
  bank. Played once as the tanker exits the screen.

All sprites are imported at **32 pixels per unit**. Plane sprites are
64×64 px; tiles are 32×32 px; bullets are 8×16 px.

---

## Phase 4: UI

**Layout** (matches original — right panel):

```text
[ Radial gauge:                ]
[   outer ring = fuel (E→F)    ]
[   concentric rings = shields ]
[   plane icon at center       ]
[ SHIELDS [====100]            ]
[ FUEL    [====96 ]            ]
[ SCORE   [  0    ]  LIVES 3  ]
[ Minimap                      ]
```

**Files:** `Assets/Scripts/UI/HUDController.cs`,
`Assets/Scripts/UI/MinimapController.cs`

### Radial Gauge (combined fuel + shields widget)

The gauge is a single compound UI element:

- **Fuel ring**: outermost UI Image, Fill Type = Radial 360. Its
  `fillAmount` tracks `fuel / 100f`. Color shifts green → yellow → red
  as fuel depletes.
- **Shield rings**: 5 concentric UI Images inside the fuel ring. Each
  ring maps to a 20-point shields band. A ring is fully opaque when
  shields are in its band; dimmed (alpha 0.2) otherwise. All 5 dimmed
  means shields = 0.

  | Ring          | Band   | Color        |
  | ------------- | ------ | ------------ |
  | 1 (outermost) | 81–100 | Green        |
  | 2             | 61–80  | Yellow-green |
  | 3             | 41–60  | Yellow       |
  | 4             | 21–40  | Orange       |
  | 5 (innermost) | 1–20   | Red          |

- **Plane icon**: static image at the center of the gauge.
- All elements are children of a single `RadialGauge` Canvas GameObject,
  bound to `PlayerStats.OnShieldsChanged` and `PlayerStats.OnFuelChanged`.

### Text Counters

- `TextMeshPro` labels for Shields value, Fuel value, Score, Lives.

### Minimap

- Second orthographic camera (`MinimapCamera`), not parented to the player.
  Subscribes to `BackgroundScroller.OnMapWrapped` and shifts its own position
  by the wrap offset on each wrap, keeping it aligned with the full Tilemap.
- Renders to a 256×256 `RenderTexture` displayed in a UI Raw Image.
- Culling mask includes the Tilemap layer (so islands appear) plus the
  `Minimap` layer (for dots).
- Dot markers on the `Minimap` layer: enemies = yellow, parachutes =
  white, tanker = cyan, player = bright blue. Each enemy and pickup has
  a small colored dot as a child GameObject on the `Minimap` layer. The
  player's dot is also a child GameObject on the `Minimap` layer,
  attached to the Player GameObject in the scene at edit time (not
  instantiated at runtime).
- As the Tilemap scrolls and wraps, the minimap reflects those same world
  positions — no extra logic needed since the camera sees the Tilemap
  directly.

---

## Phase 5: Player Plane

**Files:** `Assets/Scripts/Player/PlayerController.cs`,
`Assets/Scripts/Player/PlayerShooter.cs`

- **Rigidbody2D**, Gravity Scale = 0, Collision Detection = Continuous.
- **NoseCollider**: child GameObject at the plane's nose tip with a
  `CircleCollider2D` (trigger, radius ≈ 0.2 units) used for tanker
  docking detection.
- The plane always moves forward in the direction it is currently facing
  (like a real aircraft). Velocity = `transform.up * currentSpeed` applied
  each `FixedUpdate()`.
- Default tuning values (tweak as needed during testing):
  - `baseSpeed` = 5 units/s
  - `turnSpeed` = 90 °/s
  - `fuelDrainRate` = 2 fuel points/s
  - `refuelRate` = 10 fuel points/s
- Fuel drain formula: `actualDrain = fuelDrainRate × drainMultiplier` fuel
  points per second, where `drainMultiplier` = 1 (no key), 2 (up held),
  0.9 (down held). `PlayerController` sets `PlayerStats.drainMultiplier`
  each frame based on the held arrow key before `PlayerStats.Update()`
  reads it.
- Controls:
  - **Left arrow**: rotate plane counter-clockwise at `turnSpeed` deg/s.
  - **Right arrow**: rotate plane clockwise at `turnSpeed` deg/s.
  - **Up arrow (held)**: speed multiplier ×1.5, drain multiplier ×2.
  - **Down arrow (held)**: speed multiplier ×0.75, drain multiplier ×0.9.
  - No modifier key: base speed and base fuel drain.
  - **Spacebar**: fire one bullet from the nose per key press
    (`Input.GetKeyDown` so holding spacebar does not repeat). The bullet
    travels in the direction the player's plane is currently facing.
- Up arrow takes priority if up and down are held simultaneously.
- The player is never clamped — the world wraps around them in all
  directions.

**File:** `Assets/Scripts/Player/PlayerShooter.cs`

- On `Input.GetKeyDown(KeyCode.Space)`, calls
  `BulletPool.Instance.GetBullet(BulletType.Player)`, positions the
  bullet at the `NoseCollider` world position, and sets its direction
  to `transform.up` (the direction the plane faces).
- Disabled (will not fire) while `PlayerController.state == Refueling`.
- No automatic fire-rate limiting beyond `GetKeyDown` (one shot per
  press).

**File:** `Assets/Scripts/Player/PlayerStats.cs`

- `int shields` (max 100), `int fuel` (max 100), `int lives` (start 3).
- Drain is accumulated as a float each `Update()` and floored to int, so
  displayed values step in whole numbers.
- Fuel drains at `fuelDrainRate × drainMultiplier` fuel points per second
  in `Update()`. `drainMultiplier` is a public property set by
  `PlayerController` each frame (1, 2, or 0.9 depending on held key).
- On death (fuel hits 0 or shields reach 0): play a crash animation, then
  decrement `lives`. If `lives > 0`, respawn the player at the center of
  the screen with full shields and fuel, and grant 5 seconds of
  invincibility (the `Player` layer temporarily stops colliding with
  `EnemyBullet` and `Enemy`; the sprite flashes by toggling the
  `SpriteRenderer` on/off at ~10 Hz for the duration). If `lives == 0`,
  show Game Over.
- `TakeDamage(int amount)`: reduces shields; triggers death at 0. Enemy
  bullets deal 10 damage per hit (10 hits to deplete full shields).
- Events: `OnShieldsChanged`, `OnFuelChanged`, `OnLivesChanged` for UI
  binding.

---

## Phase 6: Bullet System (Object Pool)

**File:** `Assets/Scripts/Core/BulletPool.cs`

- Singleton `BulletPool` pre-instantiates 20 player bullets and 50 enemy
  bullets at Start. All bullets travel at 15 units/s (3× player base
  speed).
- `GetBullet(BulletType type)` returns an inactive bullet from the pool
  and activates it.
- `ReturnBullet(GameObject bullet)` deactivates and returns to pool.
- Bullet prefab: `Rigidbody2D` (kinematic), moves in a direction set on
  activation, `OnTriggerEnter2D` checks collision with its target layer
  and calls `ReturnBullet` on self. Also returns to pool when its distance
  from the player exceeds a `maxRange` threshold (checked each `Update()`),
  ensuring bullets don't travel forever off-screen.
- Player bullets are on the `PlayerBullet` layer; enemy bullets on the
  `EnemyBullet` layer.

**File:** `Assets/Scripts/Core/Bullet.cs`

- Fields set on activation by `BulletPool`: `Vector2 direction`,
  `float speed`, `float maxRange`, `Vector3 spawnPosition`.
- `Update()`: moves `transform.position +=
  (Vector3)(direction * speed * Time.deltaTime)`. If distance from
  `spawnPosition` exceeds `maxRange`, calls
  `BulletPool.Instance.ReturnBullet(gameObject)`.
- `OnTriggerEnter2D`: if the collider is on the correct target layer,
  calls `target.TakeDamage(10)` (or `PlayerStats.TakeDamage(10)` for
  enemy bullets), then calls
  `BulletPool.Instance.ReturnBullet(gameObject)`.

---

## Phase 7: Enemy System

**Files:** `Assets/Scripts/Enemy/EnemyBase.cs`,
`Assets/Scripts/Enemy/SmallEnemy.cs`,
`Assets/Scripts/Enemy/LargeEnemy.cs`,
`Assets/Scripts/Enemy/BossPlane.cs`,
`Assets/Scripts/Core/AircraftMover.cs`

`AircraftMover` is a shared component used by all non-player aircraft
(enemy formations, boss, tanker). It implements the patrol wander
algorithm (smooth random direction changes via `Mathf.PerlinNoise`) and
exposes `speed` and `turnRate` so each aircraft type can tune its feel
while reusing the same logic.

**Enemy types:**

All enemy planes fly slower than the player.

| Type        | HP      | Fire Rate       | Notes                       |
| ----------- | ------- | --------------- | --------------------------- |
| Small plane | 1 hit   | 1 shot / 2 s    | Formation flying, can fire  |
| Large plane | 3 hits  | 1 shot / 4 s    | May break formation to hunt |
| Boss        | 20 hits | 1 spread / 0.5 s| End of level, spread fire   |

**Boss movement:**

The boss patrols slowly back and forth in a horizontal sweep across the
upper third of the screen (approximately 60% of viewport width). It
uses `AircraftMover` with a low speed (≈ 1.5 units/s) and reverses
direction when it reaches each sweep endpoint. It fires its 5-bullet
spread toward the player every 0.5 s regardless of movement direction.

**Formations:**

- Enemies always spawn in formation groups of 4–6 planes.
- A formation is a mix of small planes. Exactly 3 of the 5 formations
  include one large plane; the remaining 2 are small planes only.
- Planes in a formation maintain fixed offsets from a shared formation
  anchor — an invisible GameObject with `AircraftMover` attached, parented
  under the formation's root object. Individual planes follow by setting
  their position to `anchor.position + offset` each frame.
- The formation anchor flies a random wandering pattern (smooth random
  direction changes via `Mathf.PerlinNoise`) while off-screen.

**AI state machine (EnemyBase):**

- **Patrol** (off-screen): formation anchor wanders in a random pattern.
  Individual planes hold their offset from the anchor. No firing.
- **Active**: triggered when the formation anchor comes within the main
  camera's visible range. Because the minimap camera renders all world
  objects at all times, `OnBecameVisible()` cannot be used — it would
  fire immediately at game start. Instead, each formation checks each
  frame whether its anchor is within 1.5× the main camera's half-height
  of the player's position; when true, all planes in the formation switch
  to Active simultaneously. They continue flying in formation and may
  begin firing at the player. If the formation contains a large plane,
  there is a per-formation chance (40%) that it breaks formation and
  enters **Hunt** state. This chance is rolled once, at the moment the
  formation enters Active state.
- **Hunt** (large plane only): the large plane smoothly steers toward the
  player using a gradual turn rate (`Mathf.MoveTowardsAngle`) — no
  instant direction changes. It fires while hunting. Remaining small
  planes continue their pattern independently.
- **Death**: play explosion, destroy. Remaining formation members continue
  in their current state.
- **Body collision with Player**: any enemy plane that physically collides
  with the player calls `PlayerStats.TakeDamage(25)` and immediately
  destroys itself (plays explosion). Handled in
  `EnemyBase.OnCollisionEnter2D`.

**Enemy firing:** all non-boss enemies fire a single bullet aimed
generally toward the player (direction to player with a random angular
offset in the range ±15°). The boss fires a spread of 5 bullets evenly
distributed across a 90° forward arc,
centered on the direction toward the player.

**Level Initializer:** `Assets/Scripts/Core/LevelInitializer.cs`

- At game start, instantiates **5 formations** and all pickups for the
  entire level at once. No enemy planes or pickups are created during play.
  The tanker is not pre-placed; it spawns on demand (see Phase 9).
- Each formation anchor is placed at a random world position at least
  20 units from the player's spawn point (center of world), ensuring all
  formations start off-screen and in **Patrol** state. They become
  **Active** when the anchor drifts within 1.5× the camera's half-height
  of the player.
- The boss is not placed in the world at game start. When the last
  non-boss plane is destroyed, `GameManager` spawns the boss at the top
  of the screen and it flies in.
- The level is complete when the boss is destroyed.

---

## Phase 8: Pickups (Parachutes)

**File:** `Assets/Scripts/Pickups/Parachute.cs`

- Three variants: Shields restore (+30, capped at 100), Fuel restore
  (+40, capped at 100), Colleague rescue (score bonus +500).
- **5 of each variant** (15 total) placed at random world positions by
  `LevelInitializer` at game start. None are created during play.
- Drift very slowly in a random direction assigned at game start
  (`Rigidbody2D` with a fixed low velocity, no drag).
- Subscribe to `BackgroundScroller.OnMapWrapped` and shift position by
  the wrap offset on each wrap, same as enemies.
- `OnTriggerEnter2D` with player: apply effect, play pickup sound, destroy.
- Visual: parachute canopy sprite with the variant's payload sprite
  suspended below (first aid kit, fuel barrel, or soldier).

---

## Phase 9: Tanker Plane Refueling

**File:** `Assets/Scripts/Core/TankerPlane.cs`

The tanker is not pre-placed. When `PlayerStats.fuel` drops below 25%,
`GameManager` spawns a tanker off the top edge of the screen. It does
NOT use `AircraftMover`'s random wander. On spawn, it is assigned a
random horizontal direction (left or right) and flies in a straight
line at a constant slow speed (2 units/s). It does not steer. The
player uses the minimap cyan dot to intercept it. The tanker subscribes
to `OnMapWrapped` and shifts position on each wrap, keeping it
perpetually reachable.

**Tanker behavior:**

- Distinct visual (large, friendly — e.g. green or white) so the player
  can identify it immediately.
- Shows a distinctly colored dot on the minimap (e.g. bright cyan) so the
  player can locate it easily.
- Has a circular trigger zone (`CircleCollider2D`, trigger) centered on
  its tail/rear. A visible circle sprite marks this docking zone.

**Docking sequence (`PlayerController` + `TankerPlane`):**

1. When the player's plane nose (`OnTriggerEnter2D`) enters the docking
   circle, `TankerPlane` raises `OnDockingInitiated`.
2. `PlayerController` enters `Refueling` state:
   - Arrow key input is ignored.
   - Player is smoothly guided to the docking point behind the tanker
     (`Vector2.MoveTowards` each `FixedUpdate`, locked to tanker's
     docking offset).
   - `PlayerShooter` firing is disabled.
3. While docked, `PlayerStats.fuel` increases at `refuelRate` per second.
4. Refueling ends when any of:
   - Fuel reaches 100% → automatic disconnect.
   - Player presses any arrow key → immediate disconnect.
5. On disconnect: `PlayerController` returns to normal state; tanker plays
   a banking animation and flies off one side of the screen, then destroys
   itself.

Only one tanker active at a time. If fuel drops below 25% again after a
successful refuel, a new tanker spawns. The tanker subscribes to
`BackgroundScroller.OnMapWrapped` on spawn and shifts its position by
the wrap offset on each wrap, same as enemies and pickups.

---

## Phase 10: Game Flow

**File:** `Assets/Scripts/Core/GameManager.cs`

- States: `Playing`, `Dead`, `Victory`
- On player death: play crash animation, decrement lives, respawn if
  `lives > 0`, otherwise freeze time and show "Game Over" overlay with
  restart button. Restart reloads the scene via `SceneManager.LoadScene`,
  which re-runs `LevelInitializer` and `MapGenerator` for a fresh level.
- On boss death: show "Level Complete" overlay, final score.
- Score: incremented on enemy kill (small = 100, large = 300, boss = 5000)
  and colleague rescue (+500). Score persists across deaths.

**Scene:** `Assets/Scenes/GameScene.unity` — single scene, no main menu
needed for PoC.

**Scene hierarchy:**

```text
GameScene
├── GameManager          (GameManager.cs, LevelInitializer.cs)
├── Main Camera          (CameraFollow.cs — tracks player)
├── Minimap Camera       (renders to RenderTexture, large ortho size)
├── Tilemap              (Grid → Tilemap — ocean + islands)
├── BackgroundScroller   (BackgroundScroller.cs)
├── Player               (PlayerController, PlayerShooter, PlayerStats,
│                          Rigidbody2D, layer=Player, minimap dot child,
│                          NoseCollider child — small CircleCollider2D
│                          trigger at the front for tanker docking)
├── Enemies              (parent — formations instantiated here at start)
├── Pickups              (parent — parachutes instantiated here at start)
├── BulletPool           (BulletPool.cs — pooled bullet GameObjects)
└── Canvas               (Screen Space — Camera)
    ├── RadialGauge      (fuel ring, shield rings, plane icon)
    ├── ShieldsLabel
    ├── FuelLabel
    ├── ScoreLabel
    ├── LivesLabel
    └── MinimapDisplay   (Raw Image — displays RenderTexture)
```

---

## Implementation Order

1. Unity project creation + folder structure
2. Map generation + scrolling background
3. Art (all sprites)
4. UI layout (radial gauge, text counters, minimap placeholder)
5. Player movement (steering, speed, fuel drain)
6. Bullet pool
7. Player shooting
8. Player stats (shields + fuel drain)
9. Minimap camera + RenderTexture
10. Single enemy type (small plane) with basic AI
11. Level initializer (all enemies placed at game start)
12. Pickups (parachutes — shields, fuel, colleague)
13. Tanker plane refueling mechanic
14. Large enemy type
15. Boss plane
16. Game over / victory flow
17. Polish: sounds, particle explosions

---

## Verification

- **Play in Editor**: Run GameScene, verify background scrolls, player
  steers with arrow keys, up arrow speeds up, down arrow slows down, fuel
  drains accordingly, spacebar fires once per press.
- **Minimap**: Confirm enemy dots appear on minimap; tanker shows as cyan
  dot; islands visible.
- **Pickups**: Collect each parachute variant, verify shields increase,
  fuel increases, or score increases accordingly.
- **Tanker**: Fly nose into docking circle, confirm player is guided to
  docking point and fuel fills. Press an arrow key mid-refuel, confirm
  refueling stops and tanker flies off.
- **Boss**: Destroy all formations; confirm boss spawns. Confirm it takes
  multiple hits and triggers victory on death.
- **Lives**: Take enough damage to die; confirm lives decrement and player
  respawns. Lose all three lives; confirm Game Over screen appears.
- **Score**: Kill a small enemy (expect +100), a large enemy (+300),
  collect a colleague parachute (+500). Confirm the score label updates
  correctly after each event.
- **Fuel death**: Let fuel drain to 0 without taking any bullet damage.
  Confirm the crash animation plays and a life is decremented.
- **Enemy AI states**: Fly away from a formation until it is well
  off-screen; confirm enemies do not fire. Return within range; confirm
  they switch to Active and begin firing. Confirm that, for one of the
  3 formations containing a large plane, the large plane eventually
  enters Hunt state and steers toward the player.
- **SpriteGenerator**: From the Unity Editor menu, run
  **WingNuts → Generate Sprites**. Confirm all expected `.png` files
  appear in `Assets/Sprites/` with no console errors.
