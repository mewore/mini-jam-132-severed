# Multiversal Mold: A Journey Through Time and Space

## TODO

- [x] 💚 MVP idea - a rhythm game where the player places lines to restrict the movement of mold or destroy it in order to create shapes 
    - The better-filled a shape is compared to the rest, the higher the score is
    - The tempo of the songs is faster in later levels
    - Main menu concept:
      ![concept-main-menu.png](../../blob/main/concept-main-menu.png?raw=true)
    - Level concept:
      ![concept-level.png](../../blob/main/concept-level.png?raw=true)

### MVP

- [x] 💙 Bare main menu
- [x] 💙 Ability to pick a level in the main menu
- [x] 💙 Enable navigation to levels depending on whether the previous level has been cleared or not 
- [x] 💙 Slime mold spawns in the level
- [x] 💙 Time limit, with a timer bar on top of the level
- [x] 💙 Beat - slight flash, the mold thickens and speeds up
- [ ] 💙 (Probably not) Line power meter - pressing the [FILL] (W/S/X/RMB) key fills it (the better-synced, the better)
- [x] 💙 Line creation with the [LINE] (Q/A/Z/Space/LMB) key (pressing it at one beat and letting go of it on another beat) - the line power meter is depleted and how much of it is utilized properly depends on how synced the pressing of the key is
- [x] 💙 Slime molds avoid growing past lines
- [x] 💙 The line creation is cancelled if the maximum line success is no matter at which point the [LINE] button is released
- [x] 💙 When a line is created onto a slime mold, the slime mold is cut and disintegrates partially
- [x] 💙 When the level ends, the lines fade out and the mold is enlarged
- [x] 💙 Shapes
- [x] 💙 At the end of the level, the score is displayed (based on how much of the shapes are covered)
- [x] 💙 Optimize the molds
- [x] 💙 Lines decay and when they reach a size of 0, they disappear
- [x] 💙 Do not allow lines to be created too quickly
- [x] 💟 Publish `0.1.0`

### Basic features

- [ ] 💙 Optimize the molds more - do not update all segment lines when only the growing one has changed
- [x] 💙 The slime mold destroyed parts flash red before disappearing
- [x] 💚 Areas and levels based on them
  ![concept-areas.png](../../blob/main/concept-areas.png?raw=true)
- [x] 💙 Lock levels that haven't been reached yet
- [x] 💙 Pause menu
- [x] 💙 Auto-saving and auto-loading
- [x] 💙 Show the best scores of levels
- [x] 💙 Ability to navigate to or display level backgrounds from the main menu
- [x] 💙 Master volume controls
- [ ] 💜 Backgrounds
- [x] 💛 Main menu music
- [x] 💛 Main menu ambience
- [ ] 💛 Gameplay music
- [x] 💛 Line start creation SFX - https://www.zapsplat.com/music/cartoon-water-drip-single-drop-4/
- [x] 💛 Line finish creation SFX - https://www.zapsplat.com/music/cartoon-water-drip-single-drop-3/
- [x] 💛 Line fail creation SFX - https://www.zapsplat.com/music/cartoon-water-spill-or-drop-1/
- [x] 💛 Mold disintegration SFX - https://www.zapsplat.com/music/bicycle-hand-pump-pumping-air-out-into-slime-or-gunge-with-hiss-and-bubbles-version-1/
- [x] 💛 Choose level SFX - https://www.zapsplat.com/music/a-pair-of-soccer-boots-set-down-on-dirt-1/
- [x] 💛 Clear level SFX - https://www.zapsplat.com/music/alert-tone-wooden-mallet-and-a-bright-chime-positive-sound-3/
- [ ] 💛 Lab ambience - https://www.zapsplat.com/music/empty-warehouse-ambience-room-tone-quiet-hum-fans/
- [ ] 💛 Calculate score SFX
- [ ] 💜 Cover art
- [ ] 💟 Publish `0.2.0`

### Advanced features

- [ ] 💛 Pause/unpause SFX
- [ ] 💙 SFX/Music balance volume controls
- [ ] 💙 Exit button
- [ ] 💙 Show warnings for slime molds that are about to spawn
- [ ] 💙 Show the user if they have tried to make an obstacle too soon (red flashing text, camera shake, etc.)
- [ ] 💙 Show the level background at the end of the level
- [x] 💙 Nice transitions
- [ ] 💙 Ability to view backgrounds
- [ ] 💜 Icon
- [ ] 💟 Publish `0.3.0`

### Expert features

- [ ] 💙 The slime molds gradually become gray-ish as they become inactive?
- [ ] 💙 Make it easier for lines to hit the mold (i.e., take the width of the lines into account)
- [ ] 💙 Optional non-mouse inputs (instead, having to move the cursor)
- [ ] 💜 Scientist sprite
- [ ] 💙💚 Monologue
- [ ] 💟 Publish `0.4.0`

---

#### Legend

- 💙 Code/Godot
- 💜 Art
- 💚 Design
- 💛 Audio
- 💟 Special
