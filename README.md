# Deterministic Roulette

A 3D single-player roulette prototype for Unity 6 (URP). Full European/American rules, win/loss
tracking, save and resume, plus a "deterministic" mode that lets you pick the exact number the
next spin lands on, which is handy for testing or scripted play.

Everything visual and audible is generated at runtime from code: procedural meshes and URP
materials for the wheel, table and chips, a UI built in code, and synthesised sound effects. No
third-party plugins, and no tweening library (DoTween included); the easing/tween layer is
hand-written.

> **Demo video:** [Watch on Google Drive](https://drive.google.com/drive/folders/1-oBLu7EHYVtsg02KgOUTg3HYtUIlhjim?usp=sharing)
>
> **Unity version:** open with **`6000.0.77f1`** (Unity 6.0 LTS, URP). That's the editor it was
> built and tested on.
>
> **Assets:** the brief allowed sourcing art and audio (Asset Store, Google, and so on). I
> generated all of it from code instead, so there's nothing imported and nothing to license.

## Contents
- [How to run](#how-to-run)
- [Controls & gameplay](#controls--gameplay)
- [Roulette rules & payouts](#roulette-rules--payouts)
- [Architecture](#architecture)
- [Design patterns](#design-patterns)
- [Project structure](#project-structure)
- [Testing](#testing)
- [Known issues & future improvements](#known-issues--future-improvements)
- [Assets & licensing](#assets--licensing)

## How to run

1. Open the project with **Unity `6000.0.77f1`** (URP).
2. If Unity prompts to import **TextMesh Pro essentials**, accept (`Window ▸ TextMeshPro ▸ Import
   TMP Essential Resources`). The build step imports them automatically, so usually there's nothing
   to do.
3. Open `Assets/Scenes/Game.unity` and press **Play**.

The scene is built from code. To regenerate it, run **`Roulette ▸ Build Game Scene`**: it recreates
the camera, lighting, wheel, managers and canvas, saves the scene and adds it to Build Settings.

**Platform & input.** Landscape-first, which is how casino and card titles usually ship. Input goes
through Unity's Input System UI module, so the same UI works with a mouse on desktop and touch on
device; chips and bet spots are large tap targets that highlight on press. The HUD is anchored in
screen fractions, scaled by a `CanvasScaler`, and kept clear of notches by a `SafeAreaFitter`, so
it adapts across landscape phone and tablet aspect ratios (preview it with **Window ▸ General ▸
Device Simulator**). A production mobile build would still want a portrait pass and a
post-processing quality toggle for low-end GPUs; see [known issues](#known-issues--future-improvements).

## Controls & gameplay

Pointer-driven, so every "click" below is also a tap on a touch device.

| Action | How |
| --- | --- |
| **Pick a chip** | Click a denomination in the chip tray (1 / 5 / 25 / 100 / 500). |
| **Place a bet** | Click a spot on the felt. Each click stacks the selected chip there. |
| **Undo / Clear** | `UNDO` removes the last chip; `CLEAR` returns every chip this round. |
| **Rebet** | `REBET` re-places the previous round's bets. |
| **Arm a number** | Pick a number with `<` / `>` (incl. `00` on American) and press `ARM`. The next spin lands there; `RANDOM` cancels it. |
| **Spin** | `SPIN` locks the bets and spins. |
| **Switch wheel** | `EUROPEAN` / `AMERICAN` toggles single- vs double-zero (open bets are refunded). |
| **New game** | `NEW GAME` resets balance and statistics. |

Spots highlight on hover or tap, which makes the split/street/corner/six-line zones on the lines
between numbers easy to find. They sit on the grid where the chips physically would:

- **Straight** – a number cell.
- **Split** – the line between two numbers.
- **Street** – the outer edge below a column of three.
- **Corner** – the point where four numbers meet.
- **Six line** – the outer edge between two streets.
- **Outside** – the dozen, column (`2:1`), red/black, even/odd and low/high boxes.

Balance, in-play stake, net profit, spins, win/loss and biggest win update live on the left;
recent results show as a colour-coded strip up top.

**Save / resume.** The game auto-saves after every spin and on close, then resumes the exact state
(balance, table type, open bets, history and any armed number) on the next launch. `NEW GAME`
clears the save.

## Roulette rules & payouts

Standard odds, written as profit-to-stake (a winning straight returns your stake plus 35× it):

| Bet | Numbers | Pays |
| --- | --- | --- |
| Straight | 1 | 35 : 1 |
| Split | 2 | 17 : 1 |
| Street | 3 | 11 : 1 |
| Corner | 4 | 8 : 1 |
| Six line | 6 | 5 : 1 |
| Basket (EU 0-1-2-3) | 4 | 8 : 1 |
| Top line (US 0-00-1-2-3) | 5 | 6 : 1 |
| Column / Dozen | 12 | 2 : 1 |
| Red/Black, Even/Odd, Low/High | 18 | 1 : 1 |

Both wheels use the real pocket order, and every number's colour follows the standard layout
(covered by unit tests).

## Architecture

Four assemblies with a one-way dependency direction, so the rules can be tested without Unity and
the presentation can change without touching them.

```
Roulette.Core      pure C# domain, no UnityEngine reference
        ▲
Roulette.Game      MonoBehaviours: orchestration, save, procedural view, UI, audio, VFX
        ▲
Roulette.Editor    SceneBuilder (scene generation)        Roulette.Tests (EditMode)
```

- **`Roulette.Core`** holds the rules: wheel layouts and colours, the bet library and payouts,
  outcome selection, the wallet and the statistics. It declares `noEngineReferences`, which keeps
  it fast and easy to unit-test.
- **`GameContext`** is the session model. It wires the domain objects together, runs the round
  state machine and raises events. It's pure C# too; the MonoBehaviour layer only sequences the
  visual spin around it.
- **Views** observe the model through C# events and never call back into the rules. The round
  controller drives the wheel through an `ISpinAnimator` interface, not the concrete view.

The landing is computed, not faked: given the target pocket, `WheelView` works out the final rotor
angle that puts that pocket under the drop point, counter-spins the ball, and settles it in with a
couple of damped bounces.

## Design patterns

| Pattern | Where | Why |
| --- | --- | --- |
| **Strategy** | `IOutcomeSelector` (random vs deterministic), `ISaveStorage` (file vs PlayerPrefs) | Swap behaviour without branching. |
| **Decorator** | `DeterministicOutcomeSelector` wraps a fallback selector | Layers the "forced number" on top of any selector. |
| **Observer** | `Wallet`, `GameStatistics`, `GameContext` events to the views | Views react to model events; no polling. |
| **State** | `RoundPhase` machine in `GameContext` (Betting → Spinning → Payout) | One place decides what's allowed when. |
| **Command** | `PlaceBetCommand` + an undo stack | Undo and one-click clear come for free. |
| **Factory** | `BetLibrary`, `MeshFactory`, `MaterialFactory`, `SpriteFactory`, `RouletteWheelBuilder` | Central construction of bets and procedural assets. |
| **Singleton** | `GameManager`, `AudioManager` | Composition root / audio service. |
| **MVC / MVP** | Core + `GameContext` (model), MonoBehaviour views, `RouletteController` | Splits rules, presentation and flow. |
| **Dependency inversion** | `RouletteController` → `ISpinAnimator` | The controller never depends on a concrete view. |

## Project structure

```
Assets/
├── Game/
│   ├── Scripts/
│   │   ├── Core/      Roulette.Core (pure C#): Wheel, Betting, Outcome, Player
│   │   ├── Game/      Roulette.Game: GameContext, GameManager, RouletteController,
│   │   │              Commands/, Save/, View/ (Tween, Procedural, Wheel, UI, Audio, VFX)
│   │   └── Editor/    Roulette.Editor: SceneBuilder, PreviewCapture
│   ├── Tests/         Roulette.Tests (EditMode)
│   └── PlayTests/     Roulette.PlayTests (scene smoke test)
└── Scenes/Game.unity
```

## Testing

EditMode tests cover the parts that matter: wheel layouts and colours, every bet type's payout and
coverage, the round flow (betting, undo, deterministic spin, payout, statistics, table switch) and
a full save → restore round-trip. A PlayMode smoke test loads the scene and runs a forced spin end
to end.

Run from **`Window ▸ General ▸ Test Runner`**, or headless:

```
Unity.exe -batchmode -runTests -projectPath . -testPlatform EditMode -testResults results.xml
```

## Known issues & future improvements

- All of the brief's inside bets (straight, split, street, corner, six-line) and outside bets are
  in. A few zero-corner micro-bets beyond that, such as the `0-1` split or the `0-1-2` trio, aren't
  placeable yet; the bet system is data-driven, so they're a small addition to `BetLibrary`.
- Landscape-first by design (how casino table games ship) and tuned for 16:9. It scales and works
  with touch, but a real portrait layout with bigger, relaid-out spots would be its own pass.
- Sound effects are synthesised in code (chip, spin loop, ball drop, win, lose). They can be
  swapped for recorded clips by changing what `AudioManager` pulls from `ProceduralSfx`.
- No table limits or maximum bet.
- The HUD is built at runtime, so the scene looks empty in edit mode; press Play to see it.

## Assets & licensing

All meshes, materials, sprites, UI and sound are generated at runtime from code: no imported
textures, models or audio, and nothing third-party to license. Only first-party Unity packages are
used (URP, uGUI/TextMesh Pro, Input System, Test Framework).
