# Winamp visualizers: MilkDrop presets in Bonsai

MilkDrop is the visualizer that shipped with Winamp: a full-screen pattern that warps, echoes and pulses in time with whatever is playing. What it draws is decided by a *preset*, a text file that describes a shader program. There were many authors of these presets, and each preset generated a unique look and feel for reacting to audio, and credit goes to them for actually doing the hard work to design these visuals.

The idea behind `Bonsai.Prism` is to bring these visual presets into Bonsai. At it's core, it uses the same framework that was introduced in [exercise 12](../../worksheets/02-live-demo.md#exercise-12-winamp-visualizer). A generic engine provides the plumbing that all presets expect (buffered audio waveforms, bass, treble, etc), and preset's shaders become GLSL that the `Bonsai.Shaders` package uses to render. A converter takes the original text-based preset files and spits out a composable Bonsai workflow you can open, read and edit like any other. The `Bonsai.Prism.Presets` package is a collection of over five hundred preset files that have been converted into Bonsai workflows, with presets loosely grouped by author. Credit goes to all the authors for making these amazing visuals.

This demo is a curated list of presets, put under the control of eight faders of a MIDI controller so they can be mixed into each other live.

## How a preset works

Three things make up a MilkDrop preset, and each lands somewhere different in Bonsai.

* **Shaders.** A *warp* pass that samples the previous frame through a distorted coordinate field, and a *composite* pass that colours the result on the way to the screen. These translate to GLSL and run as `Bonsai.Shaders` materials. The warp pass reading last frame's output is what produces the endless tunnels and smears.
* **Equations.** Every preset carries a small program in EEL that runs once per frame and decides how much to zoom, rotate and warp, along with `q1` to `q32`, a set of values the shaders read.
* **Shapes and waves.** Rings, polygons and waveform traces drawn over the top, with their own equations.

Everything per-frame sits inside a gate driven by the preset's `Weight`. At weight zero a preset has no subscriptions and costs nothing per frame, which is what makes loading hundreds of them at once reasonable. Raise the weight and the gate subscribes, the equations start running, and the preset fades up over whatever else is drawing.

## Hardware

* A Korg nanoKONTROL2. The demo reads its eight faders on MIDI control numbers 0 to 7, and the two track buttons on control numbers 58 and 59. Any controller that sends those control numbers works; any other controller works too once the numbers in `Devices` are changed to match. You can easily extend this to connect different buttons, knobs, sliders, or whatever to different behaviors, like triggering switches between presents, ramping up or down different uniforms, etc.
* Speakers or headphones playing something, or a microphone to capture audio. The demo only needs to be able to stream audio output from somewhere, and the data streams are agnostic to whether that comes from a microphone, headphones, or anything else. Put on a track with a strong beat and see!

## Setup

This demo needs `Bonsai.Prism`, the preset package, and a handful of release candidates that are not yet published, so it has its own environment in this folder and pulls those packages from `packages/` at the top of the repository.

1. In the unzipped workshop folder, open `docs/examples/winamp-prism/.bonsai`.
2. Double-click `Setup.cmd`. It downloads Bonsai and the packages and closes when finished. The preset package is large, so expect this one to take a while.
3. Double-click `Bonsai.exe` in that folder, then open `WinampPrism.bonsai` from `docs/examples/winamp-prism`.
4. Select the `Devices` group and look at the properties panel. `DeviceName` is the audio device to capture and is saved as a loopback device belonging to one particular laptop. Speakers or headphones ending in `[Loopback]` will capture streams that are being played through a device, whereas choosing a microphone will capture streams coming from outside. `MidiDevice` is the controller, saved as `nanoKONTROL2 _ CTRL`.

## WinampPrism

:::workflow
![WinampPrism](WinampPrism.bonsai)
:::

Six things at the top level. Reading left to right, the first three build and feed the renderer, and the last three decide which presets are on and how loud.

### ShaderEngine

`ShaderEngine` is the renderer, included from `Bonsai.Prism`. It opens the window, declares the meshes and textures every preset shares, loads the preset resources and then runs the render loop, publishing the subjects presets subscribe to.

### Presets

The presets are loaded by chaining them in series one after another. Double-click a Preset to inspect them; each node is one `IncludeWorkflow` pointing at a preset inside `Bonsai.Prism.Presets`, with its `Weight` exposed externally and set to 0.

The chain is a resource configuration: each preset appends the textures and shaders it needs to the collection flowing through, and the result is multicast back to the engine as `PresetConfiguration`, which is what the engine loads before starting the render loop. Adding a preset to the demo means dragging another workflow out of the toolbox and into the chain.

### Devices

Audio in and MIDI in.

* `AudioCapture` from `Bonsai.Mixer` captures the computer's own output: `HostApi` is `Windows WASAPI` and `DeviceName` is a speaker device with `[Loopback]` in its name, so the demo hears exactly what you hear.
* `Rescale` maps the 16-bit sample range onto the ±128 that MilkDrop's waveform expects, then `Buffer` collects 1024 samples at a time, stepping forward 441 samples per buffer, so the visualizer gets a fresh overlapping window a hundred times a second. That is `BufferedAudio`, which becomes the waveform texture; transposed, it is `Waveforms`.
* `InputDevice` opens the controller and `ControlChange` turns its messages into control number and value pairs, published as `ControllerInput`. Eight near-identical branches follow: each keeps the messages whose `ControlNumber` matches one fader, takes the `ControlValue`, divides by 127 to get a number from 0 to 1, and publishes it as `SliderVal1` to `SliderVal8`.
* The last two branches are the track buttons. Control number 59 with a value of 127, a press rather than a release, emits 1; control number 58 emits -1; `Merge` puts both into `ChangePreset`.

### AudioProcessing

Also included from `Bonsai.Prism`. This is the audio streams get processed into numbers that the equations actually read come from. `BufferedAudio` goes through a `DiscreteFourierTransform` and `Magnitude` to a spectrum, which `EqualizeSpectrum` normalises into the `Spectrum` texture. The spectrum is then split into three bands, bins 1 to 6 for `Bass`, 6 to 46 for `Mid`, 46 up for `Treble`, each published twice: once as the band's current power, and once through an `IirFilter` as a slow-moving average, which is the `*Attack` variant. `Beat` is the ratio between the two on the bass band, clamped: it rises when the bass jumps above its own running average and sits at zero the rest of the time. 

### Controller

Which eight presets the faders are holding.

* Every preset announces itself in the `ActivePresets` registry as it loads. `Distinct` then `Accumulate` counts them, and the `CreateObservable`/`Switch` pair around a one-second `Timer` waits until the count stops changing before publishing it as `PresetsLoaded`.
* `Range` over that count, through `Permutation` with a `CreateRandom` seeded at 1077, gives `RandomPresetsArray`: a shuffled running order. The seed is fixed, so the order is the same every run. Change it for a different deck.
* `CurrentPreset` is an index into that order, the first of a bank of eight. `ChangePreset` multiplied by 8 steps it a whole bank at a time, and the `ExpressionTransform` after it wraps the result back inside the deck at either end.
* Then eight copies of `RandSelectPreset`, one per fader. Each takes `CurrentPreset` plus its own offset, wraps it, reads the shuffled array to find which preset sits at that position, looks the position up in `IndexedPresets` to recover the preset's name, and pairs that name with its fader's value. `GroupBy` keys the pairs by name and multicasts them into `WeightedPresets`.
* `WeightedPresets` is the other half of every preset's weight path: a preset merges the values arriving under its own name with its `Weight` property, last one wins. Nothing addresses a preset by position in the chain, so a scheduler, a beat detector or another controller can drive any preset's weight by publishing under its name.

### PresetWeight

`WeightedPresets` flattened back into name and value pairs. Nothing downstream consumes it; it is there to watch. Run the workflow, open its visualizer and move a fader to see which preset you are actually touching.

## Playing with it

* Start the workflow with every fader down. The window is black, because every preset is at weight zero and nothing is drawing.
* Push fader 1 up slowly. A preset fades in. Push fader 2 up as well and the two mix; pull 1 back down and you have crossfaded between them.
* Press the track buttons to move the bank eight places along the shuffled deck and go hunting. The deck is 165 presets long and wraps, so there is a lot to get through.
* Watch what the music is doing to the picture. Some presets will barely move without a beat; others are mostly time-driven and will look the same with the audio muted. The difference is whether the preset's equations read `bass`, `treb` and `beat` at all, and you can check by opening the preset workflow and looking for those subjects.
* Right-click a preset in the `Presets` chain and choose "Go to Definition" to open it. The resource configuration and the weight path are at the top, and the gated region below holds one small branch per equation variable. Find the branch that ends in a `TryUpdateUniform` named `zoom` and change the expression feeding it.
* **Question:** the eight faders are the only way in, and they are wired to the presets through names rather than positions. What else in the workshop could write to `WeightedPresets`? A `Beat` threshold, the camera from Exercise 4 of [The Live Demo](../../worksheets/02-live-demo.md), the pose measurements from [MoveNet](../movenet/index.md) — any of them is a few operators from driving the mix.

> [!TIP]
> Every loaded preset allocates its own feedback and work textures at the window's size whether or not it is drawing, so 165 presets in a large window is a lot of video memory. If the window is slow or the editor struggles, raise `Width` and `Height` only once you have trimmed the `Presets` chain down to the handful you want to show.
