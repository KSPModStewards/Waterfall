## 0.11.0 - 2026-04-29

- Fixed a regression in the randomness controller. Effects that used a RandomnessController created prior to 0.10.0 did not include a new required configuration field to determine whether the controller used a randomized seed, which was being defaulted to False. This has been changed to default to True, which is a more generally correct default.
- CustomPullController can now read a value from any kind of module, not just ModuleEngines.
- Added Conditional attribute to logging calls, to reduce performance overhead.  These will now only be emitted in a debug configuration.
- Improved randomness of perlin noise controller.
- Added response rate to Remap controller
- Added "vertical" mode to velocity controller
- Updated controller documentation on the wiki


## v0.10.5

- Fix remap controller that was not working after fixes in 0.10.0
- Add new Engine On/Off controller: returns 0 if an engine is off, 1 if on. 
- Add new Scalar Module controller: returns state of an IScalarModule (e.g. ModuleAnimateGeneric, ModuleAnimateHeat, ModuleColorChanger, etc)
- Fix some additional ungated log calls


## v0.10.4

- Removed a few log messages that were not gated behind flags correctly


## v0.10.3

- Fixed an issue where certain effects would miss their first update (SEP, Tundra RCS blocks) and get stuck in a bad state


## v0.10.2

- Fixed an issue with shader compilation on OpenGL (#173)
- Fixed a test particle patch from development still being around
- Fixed an issue with effects not being updated when their controller value changed if they had a modifier that would hide them sometimes (#174)
- Fixed an issue with custom push controllers where they they would not sleep/wake up correctly


## v0.10.1

- Fixed some debug switches being on in settings


## v0.10.0

- Updated bundled ModuleManger to 4.2.3
- Large swathe of performance updates thanks to JohnnyOThan, including:
 - Effects on inactive versions of parts (e.g. multiple part variants) are no longer evaluated
 - Effects that use controllers that don't exist on a part won't be evaluated
 - Effects only update if one of their controller values changed or they have random inputs
 - Effect modifiers don't re-evaluate curves if the controller value didn't change
 - Integrated gotmachine's faster FloatCurve implementation
 - Many minor optimizations and cleanups that add up to large improvements
 - Some memory leaks fixed
- Fixed engine light layers targeting IVA layers (#130)
- Thrust and Throttle controllers can now have their EngineID set in the UI (#125)
- Added a UI option for Perlin noise to have its seed to an option that will randomize it on effect startup (#127)
- It is now possible to set Engine ID in Engine Event Controllers, and those controllers will only fire on events asocaited with that engine ID (#126)
- Fixed Enable Distortion setting actually doing nothing
- Added a setting toggle to treat random controllers as unchanging, which improves performance at the cost of randomness
- Various logging cleanups and improvements
- Reworked most Editor window UIs to look cleaner and work better, including a lot of UI code cleanup for more coherency
- Added the ability to load arbitrary particle assets from assetbundles and control them
 - Controllability is limited


## v0.9.0

- Significant stack of performance updates that should increase performane significantly on ships/templates with lots of effects (big thanks to Al2Me6, DRVeyl, JohnnyOThan)
- Fixes to Additive Volumetric shader including fix for atmosphere sorting at night sans scatterer


## v0.8.2

- Fix to procedural particles shader


## v0.8.1

- Fix compatibility with KSP < 1.12


## v0.8.0

- Significant rewrite of controller and controller UI code. This resulted in a change to how generated configs look, but existing configs should work fine (thanks ArXen42 for all the hard work for this)
- Improvements to UX for controller selection and configuration
- Added an error notification when you try to add an effect with an invalid effect parent
- Some refactoring of UI code in various areas
- Redid color picker for significantly better UX
- Added the ability to save (within a game session) color swatches to copy/paste between effects and materials
- Shader bundles can now be loaded from anywhere in gamedata (anything with a .waterfallshaders extension)
- Shader bundles are loaded alphabetically and replace existing shaders in the order loaded. e.g if xyz.waterfallshaders contains a shader with the same name as abc.waterfallshaders, the one from xyz.waterfallshaders will be used
- Shader improvements
 - Additive (Volumetric) shader:
  - fixed excessive noise bug
  - the _Noise setting is now 5 times more sensitive (divide your old _Noise setting by 5 to get previous behavior)
  - you may need to slightly increase your _FadeOut setting if your old setting was below 0.5
  - start and end tints will no longer depend on Fresnel or InvertedFresnel settings (so your old color gradient may need some readjustments)
  - added FresnelFadeIn setting so you can gradually fade in the Fresnel effect starting from the nozzle exit.
 - Echo (Dynamic) shader:
  - echo spacing (_EchoLength) setting will now properly scale with the mesh scale
  - new _ExpandLength setting stretches the individual echos


## v0.7.1

- Changed the default blend mode of all additive-type effects to One One, this is controlled by a new Settings item called EnableLegacyBlendModes
- Fixed the Thrust controller which was not working right (al2me6)
- Fixed material picker texture name throwing an error (al2me6)
- Fixed color editor window being too wide


## v0.7.0

- New shaders: Waterfall/Additive (Volumetric), Waterfall/Additive Cones (Volumetric),  Waterfall/Additive Echo (Dynamic) by KnightOfStJohn 
- Added new workflow: volumetric for new shaders
- Fixed randomness seed for different effects on different parts not working right
- Made material labels a little clearer in the Material Editor


## v0.6.8

- Various popup UI windows will now be better at retaining their position if changed when opened (e.g when changing the color you are editing)


## v0.6.7

- Added an option to allow non-uniform parent scaling in effects (al2me6). Don't use this if you don't know exactly what you are doing


## v0.6.6

- Updated compatibility to KSP 1.12


## v0.6.5

- Resolved an issue that would cause waterfall data items (templates, etc) to be unpatchable with Module Manager (thanks al2me6!)


## v0.6.4

- Added a ThrustController controller which keys off engine normalized thrust
- Added a more verbose exception when failing to load a template
- Disabled SMR rebuilding as it wasn't doing much and was negatively impacting performance


## v0.6.3

- Improved hydrolox-lower-3 (RS-68) template (Zorg)
- New hydrolox-upper-2 and methalox-upper-2 templates (KnightOfStJohn)
- Fix issue when deleting effects in the editor


## v0.6.2

- Fixed texture picker not showing texture names
- Fixed black line in shaders when using antialiasing
- Fixed distortion effects not being culled on low intensity


## v0.6.1

- Fixed light editor in UI overwriting light settings with last light's settings
- Minor performance improvements
- Fixed throttle being considered 'on' always with engines that have a minThrust specified
- Added new ramp up and ramp down optional config flags for throttle controllers
- New template updates from Zorg
 - Another alcolox template, alcolox-lower-2 (more accurate to Mercury redstone).
 - Revised alcolox-lower-1 (looked too nice to delete so its another alternative).
 - Revamped hydyne plume
 - Added hydrazine monoprop plume (only for v small engines, dervied from RCS plume).
 - Added Kerosene+Nitric acid, lower and upper (R12, kosmos 2I type).
 - Removed click when looping NFT engine sound loops.
 - Added thalox sustainer template
 - Slight tweaks to RCS template. (existing templates should be fine).


## v0.6.0

- Moved 'engineID' field inside the Throttle controller
- Added 'thrusterTransformName' field to RCS controllers, which will allow them to map to unique ModuleRCSFX 
- Added a new 'minimum' field to perlin noise that allows the generation of negative numbers. Renamed old 'scale' to 'maximum', as that's what it was
- When adding effects, can now select a Workflow, which filters Shaders and Models so they are easier to match up with valid choices
- When selecting modifier types, visible target transofrms are filtered so only valid ones are shows (e.g, no)
- Fix for culling of Alpha (Directional) shader
- Improved algorithm for effect sorting, should be better most of the time
- Fix for all shaders under HDR mode: significantly better looks!
- Better descriptions/text in the UI when selecting models, shaders, etc
- Various UI fixes throughout
- Added support for Lights
 - More or less like EngineLight
 - Add Lights through the usual workflow
 - Apply modifiers to modify light parameters like other effects
- Added new Deformation (Dynamic) shader
- Added new simple plane models for use in various effects
- Templates are now better
 - Specifying multiple templates per module are now supported in configs
 - Editing multiple templates at once is supported in the UI


## v0.5.0

- Rewrote effect application backend with a few enhancements
 - SUBTRACT, ADD modifier modes actually work properly now
 - Performance improvements overall
 - Even more major performance issues when effects are fully transparent
- Somewhat changed how atmospheric depth is calculated, result should be fairly in line with SmokeScreen/RealPlume implemenation for better compatibility
- Fixed an issue where effects would occasionally not be reloaded correctly on game load if there was more than one ModuleWaterfallFX on a part
- Improved light color controllers
- Shaders
  - Updated Additive (Dynamic) shader with a few bug fixes for HDR
  - Updated Alpha (Directional) shader to work sgnificantly better in general
  - Addded Alpha (Dynamic) shader which is broadly the same as Addtive but alpha
  - Added some manual z-sorting handling to deal with alpha/additive transparency sorting issues
- Templates:
 - Major template update from Zorg with dozens of new templates and many fixes/improvements
 - New RS-25 plume by KnightOfStJohn
 - Updates and improvements to existing garbo Nertea templates


## v0.4.1

- Changing TextureScale, TextureOffset and Vector3/4 material properties in the editor will now apply to multiple instances of a plume (e.g on multiple thrust transforms) immediately rather than requiring reload
- EffectRotationModifier now uses local Euler angles instead of local look rotation. 
- Added a Speed parameter to the perlin noise controller
- Exposed _Seed material parameter for noise in the UI
- Added ability to specify Randomize Noise Seed at effect creation, which causes _Seed to be randomized upon effect creation. Disabling this allows the effect _Seed to be modified in the material editor


## v0.4.0

- Fixed shader time offset causing random scaling of shader speed values
- Reorganized the UI overall, with many improvements, some highlights
 - It is now possible to export a complete ModuleWaterfallFX
 - It is now possible to edit Effect names 
 - It is now possible to re-order Effect Modifiers
 - Added more detailed Controller UI
   - Only controllers present on the module are shown
   - Can add, edit and remove controllers
   - Controllers can be overridden individually
   - When not overridden, controllers will try to show the value of the controller
 - Many QOL improvements
- Added ability to specify noise type to Random controller: can use random (old one) or perlin (basic 1d perlin noise)
- Added new controllers
 - Mach: outputs current Mach number
 - Gimbal: outputs gimbal fraction, where 1/-1 is full extension, 0 is centered. Needs axis specified.
 - EngineEvent: event controller, fairly prototypical. Two events are currently available, ignition and flameout.


## v0.3.0

- Fixed some bugs with the RCS controller
- Updates to some templates
- Added waterfall-hydrolox-lower-3: Delta IV lower stage hydrolox engine


## v0.2.10

- KSP 1.11


## v0.2.9

- Fixed OSX shader package
- Improved Light controller
- Added new LightColorModifier, link a Material color to the color of a Light
- Improved spotlight and floodlight templates (cones and flares adjust to RGB colour of the light, flare effect is no longer removed outside of the atmosphere)


## v0.2.8

- Added better HDR handling to shaders, should result in less TUFX breakage in HDR mode
- Added new Billboard (Additive Directional) shader - a billboard that fades out when away from a specific angle
- Added waterfall-spotlight-1 and waterfall-floodlight-1 templates
- Added new Light controller (link effects to a light intensity)
- Added two new flare textures to use


## v0.2.7

- Added IgnoreProjector tag on all shaders; should resolve some interactions with Scatterer


## v0.2.6

- Added new guard for multiple misconfigured modules that should break the game less when there is a critical configuration error


## v0.2.5

- Fixed randomness values for shaders being lost sometime in the past


## v0.2.4

- Added a ShowEffectEditor setting that controls whether the editor toolbar button is shown - this is off by default!
- Fixed an issue where some effect models might be on the wrong layer, causing them to be shown in the part highlighter's advanced mode
- Fixed some log messages that didn't have a log level


## v0.2.3

- Fixed DDS format of some flare textures


## v0.2.2

- More less logspam
- Future improvements


## v0.2.1

- Log levels can now be specified in settings
- Lowered log levels by default (less logspam)
- Fixed a few ModuleManager errors
- Added compiled shaders for Linux and OSX (somewhat experimental)


## v0.2.0

- Split configuration for Restock and Restock+ parts into a different mod
- Fixed plume scaling in the case where multiple plume parent transforms have different scales (i.e. nuPoodle)
- Fixed a couple of minor bugs
- Renamed/restructured templates:
 - waterfall-hydrolox-lower-1: lower stage hydrolox engine 
 - waterfall-hydrolox-lower-2: lower stage hydrolox engine, energetic, orange
 - waterfall-hydrolox-rs25-1: lower stage hydrolox engine based on SSME 
 - waterfall-hydrolox-upper-1: upper stage hydrolox engine, suitable for orbital engines
 - waterfall-kerolox-lower-1: Reddish kerolox lower stage engine, suitable for energetic lifting engine
 - waterfall-kerolox-lower-2: orangeish kerolox lower stage engine, suitable for energetic lifting engine
 - waterfall-kerolox-lower-3: extra burny kerolox lower stage engine, suitable for energetic lifting engine
 - waterfall-kerolox-sustainer-1: reddish kerolox lower stage engine, somewhat based on Titan 3 engine
 - waterfall-kerolox-upper-1: Reddish kerolox, suitable for generic upper stage engine
 - waterfall-kerolox-upper-2:Red/white kerolox, suitable for low thrust upper stage engine
 - waterfall-ntr-lh2-1: LH2-based nuclear rocket engine
 - waterfall-ion-xenon-1: Xenon Gridded Thruster engine
 - waterfall-rcs-jet-1: White RCS jet
- Added a set of sounds for authors to use, from KW and NFT


## v0.1.3

- Fixed a number of bugs related to the Material Editor and new effect creation
- Fixed Color Modifiers not working on multi-engine setups
- Improved behaviour of Transform effect blending modes (still not perfect but eh)


## v0.1.2

- Bugfixes


## v0.1.1

- Fixed parsing of plume rotation in templates
- Fixed plume switching needing specific module orders
- Scaling bugfixes


## v0.1.0

- Initial version controlled release
