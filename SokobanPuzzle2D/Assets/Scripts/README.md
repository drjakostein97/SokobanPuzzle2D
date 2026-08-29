# Forklift sprite — wheels split from chassis, with rolling animation

## What's in here

- `chassis_no_wheels.png` — the full 1024x1024 sprite with all four wheels cut out
  (transparent holes). Same canvas size/position as the original, so it drops in
  as a 1:1 replacement.
- `wheel_front_left.png`, `wheel_front_right.png`, `wheel_rear_left.png`, `wheel_rear_right.png`
  — the four wheels, each cropped tight to its own small canvas.
- `wheel_frames/` — 8 pre-shifted animation frames per wheel (`_f0` … `_f7`) that
  scroll the tread pattern to fake rolling motion.
- `spritesheets/*_rollsheet.png` — the same 8 frames stacked vertically in one file
  per wheel, sized for Unity's Sprite Editor grid-slicing (cell height = wheel height,
  8 rows).
- `wheel_info.json` — exact pixel positions/sizes for every wheel, straight from the
  source image.
- `scripts/WheelRoll.cs` — cycles a wheel through the roll frames based on how far
  the vehicle has moved.
- `scripts/WheelSteer.cs` — rotates a front wheel for steering feedback.

## Why rolling isn't just "rotate the wheel sprite"

From directly above, a rolling wheel's axle is horizontal, so the tread doesn't
spin like a clock hand — it scrolls forward/back, like a tank tread. On top of
that, this sprite only shows the sliver of tire that pokes out from under the
fender (the rest is hidden in the original 3D render), so the wheel isn't even
a full circle — rotating that chunk would swing it outside the wheel well.

So instead, each wheel keeps its exact silhouette fixed, and only the tread
pattern inside it scrolls up/down across 8 frames — that's the `wheel_frames/`
set. This is the same trick used in most top-down racers for exactly this reason.

## Unity setup

1. Import all PNGs as **Sprite (2D and UI)**, keep pixels-per-unit consistent
   across all of them (e.g. 100).
2. Hierarchy:
   ```
   Forklift (empty GameObject, your movement script goes here)
   ├─ Chassis          (SpriteRenderer: chassis_no_wheels.png, sorting order 1)
   ├─ Wheel_FrontLeft   (SpriteRenderer, sorting order 0)
   ├─ Wheel_FrontRight  (SpriteRenderer, sorting order 0)
   ├─ Wheel_RearLeft    (SpriteRenderer, sorting order 0)
   └─ Wheel_RearRight   (SpriteRenderer, sorting order 0)
   ```
   Wheels render *behind* the chassis so the fender still visually "covers" part
   of the tire, exactly like the original art.
3. Position each wheel child using the offsets below (in pixels from the
   original 1024×1024 canvas center; divide by your pixels-per-unit to get
   Unity units, Y already flipped to Unity's up-positive convention):

   | Wheel | width×height (px) | local X (px) | local Y (px) |
   |---|---|---|---|
   | front_left  | 54×158 | -215.5 | -12.1 |
   | front_right | 54×160 |  208.3 | -12.5 |
   | rear_left   | 63×156 | -222.7 | -332.4 |
   | rear_right  | 63×159 |  216.0 | -331.4 |

   With default PPU 100, e.g. front_left → `localPosition = (-2.155, -0.121, 0)`.
   Chassis stays at `(0,0,0)`.
4. On each wheel, add `WheelRoll.cs` and drag its 8 frames (from `wheel_frames/`,
   or slice the matching `_rollsheet.png`) into the `frames` array in order.
   It reads how far `vehicleRoot` has moved each frame and advances the tread
   accordingly, including scrolling backward correctly in reverse.
5. Optional — for visible steering, add `WheelSteer.cs` to just the two front
   wheels and call `SetSteerInput(-1..1)` from your input/turn code. Leave the
   rear wheels without this script.

That's it — the chassis and all four wheels recomposite to a pixel-perfect
match of the original sprite when static, and the wheels now animate
independently of the body.
