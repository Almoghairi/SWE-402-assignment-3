# SWE-402 Assignment 3 - Creative Core Polish

Project: Knockout Arena - Creative Core Polish
Student: Abdulaziz Almoghairi

Open Assets/Scenes/KnockoutArenaPolished.unity and press Play. Move with W/S or arrow keys and orbit the camera with A/D or left/right arrows.

## Section 1: Shaders and Materials
The arena, player, enemies, powerup, indicator, and arena edge all use custom URP/Lit materials in Assets/Materials. The player switches from Player_Normal_Blue to Player_Powered_Emission at runtime in PlayerController when a powerup is collected, and ArenaLightController shifts the arena tint as waves increase.

## Section 2: Lighting
The scene has a dramatic Directional Light with soft shadows plus a Point Light named Point Light - Wave Reactive. ArenaLightController subscribes to the wave event and increases the light intensity/color each wave so the arena becomes more urgent as difficulty rises.

## Section 3: Animation
The powerup prefab has a looping keyframe float/spin animation, and the player indicator uses PowerupIndicator.controller with Idle and Powered Pulse states. PlayerController drives the Animator bool parameter with SetBool when the powerup starts and expires.

## Section 4: VFX
FX_PowerupCollect, FX_EnemyKnockout, and FX_PlayerTrail are separate Particle Systems saved as prefabs. PlayerController calls Play or Stop from code for collection, enemy impact, and movement trail behavior.

## Section 5: Cameras
The camera remains a child of the required Focal Point orbit object and adds CameraPolish for smooth wave intro movement, wave-based FOV zoom, and a powered-up FOV boost. The orbit still follows the Worksheet 5 requirement that movement uses focalPoint.transform.forward.

## Section 6: Post-Processing
The scene includes a Global Volume named Global Volume - Bloom Vignette Color with Bloom, Vignette, and Color Adjustments in Assets/Settings/CreativeCorePostProcessing.asset. Bloom supports the emissive materials while vignette/color grading reinforce the polished arena mood.

## Worksheet 5 Base Features Included
The project includes player AddForce movement, camera-relative controls, bouncy physics material, enemy AI prefab, powerup coroutine, wave spawning with FindObjectsByType<Enemy>(), event/delegate based GameManager.OnGameOver, and two subscribers: SpawnManager and MusicGameOverResponder.
