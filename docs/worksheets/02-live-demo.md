# The Live Demo

This is the workflow we build on stage, in the order we build it. Each exercise adds one
idea and ends at a **checkpoint** you can copy into your own Bonsai to catch up. By the end,
a camera and a microphone feed one image through a shader on the GPU, and a keyboard or a
mouse bends it live.

The five parts follow the plan in the talk:

1. **Camera**: acquire, transform, measure motion, track a coloured object.
2. **Microphone**: acquire, measure loudness, feed a music visualizer.
3. **Combine**: let the sound change the picture.
4. **Shaders**: put the picture on the GPU and warp it.
5. **Control**: drive the warp from the outside world, and send the room's signals out to
   other tools such as TouchDesigner.

> [!NOTE]
> Every workflow figure below is a checkpoint. The copy button in its corner puts the
> finished workflow on your clipboard: click the empty canvas in Bonsai and paste. Build it
> by hand the first time if you can, that is where the ideas stick, but copying is the fast
> way to catch up.

## Part 1: Camera

Bonsai sees the world as streams. A camera is a stream of images. Every operator we add
turns one stream into another, and every stream can be watched while it runs.

### **Exercise 1:** Acquire

:::workflow
![Camera capture](../workflows/02-live-demo-01.bonsai)
:::

* Add a `CameraCapture` source.
* Press **Start** and double-click the node to open its visualizer.
* Stop. Right-click the canvas, choose **Add group** and drag `CameraCapture` into it,
  then set the group's `Name` to `Camera`. Not required, but the workflow is about to grow.

### **Exercise 2:** Transform

:::workflow
![Transform the camera image](../workflows/02-live-demo-02.bonsai)
:::

* Add a `Grayscale` transform after `CameraCapture`.
* Add a `Canny` transform after that. Set `Threshold1` to 50 and `Threshold2` to 150.
  Run, and open the visualizer on `Canny`: edges only.
* Swap `Canny` for `Smooth` (set `Size` to 15), then for `Flip` (set `Mode` to `Horizontal`).
  Same input, three different pictures.
* **Question:** what happens when you open visualizers on *two* nodes at once? Each one
  is a window onto the stream at that point in the chain.

### **Exercise 3:** Motion energy

:::workflow
![Motion energy](../workflows/02-live-demo-03.bonsai)
:::

We want one number that says "how much is moving right now".

* After `Grayscale`, start a new branch: right-click `Grayscale`, choose **Create branch**
  and add a `BackgroundSubtraction` transform. Set `AdaptationRate` to 0.1. Run: still
  parts fade to black, moving parts glow.
* Add a `Sum` transform (the one from **Dsp**). It adds up every pixel.
* Right-click `Sum` and select `Scalar` > `Val0` so the output is a plain number.
* Add a `PublishSubject` and set its `Name` to `Motion`. A subject is a named stream that
  any other part of the workflow can subscribe to. We will use it later.
* Run and open the visualizer on `Motion`. Hold still. Wave.

> [!TIP]
> If the number sits at zero, check that the visualizer on `BackgroundSubtraction` shows
> anything at all. Very low light gives the camera nothing to subtract.

### **Exercise 4:** Track a coloured object

:::workflow
![Track a coloured object](../workflows/02-live-demo-04.bonsai)
:::

* In a new branch from `CameraCapture`, add `ConvertColor` and set `Conversion` to
  `Bgr2Hsv`.
* Add `HsvThreshold`. Run, open its visualizer, and drag the `Lower` and `Upper` bounds
  until only your object is white. Hue is the top row.
* Add `FindContours`, then `BinaryRegionAnalysis`, then `LargestBinaryRegion`.
* Right-click `LargestBinaryRegion` and select `ConnectedComponent` > `Centroid`.
* Add a `PublishSubject` named `Position`.
* Run: the visualizer on `Position` draws the object's position over time.

## Part 2: Microphone

Audio arrives as *buffers*: chunks of a few hundred samples at a time. Bonsai treats a
buffer as a small matrix, so the same maths operators that work on images work on sound.

### **Exercise 5:** Acquire

:::workflow
![Audio capture](../workflows/02-live-demo-05.bonsai)
:::

* Add an `AudioCapture` source. Leave `DeviceName` empty for the default microphone.
* Run and open its visualizer. Talk. Clap.
* **Optional:** add an `AudioWriter` sink with `FileName` set to `test.wav`, run for a few
  seconds, stop, and play the file.

### **Exercise 6:** Loudness

:::workflow
![Loudness](../workflows/02-live-demo-06.bonsai)
:::

We want one number for "how loud is it right now", the audio twin of `Motion`.

* After `AudioCapture`, add `ConvertScale` and set `Depth` to `F32`. The microphone delivers
  whole numbers; this turns them into decimals so the next step has room to work.
* Add `Pow` (from **Dsp**) and set `Power` to 2. Negative and positive swings now both count.
* Add `RunningAverage` with `Alpha` 0.2. This blends each buffer with the ones before it, so
  the number moves smoothly instead of jittering. A smaller `Alpha` is smoother but slower
  to react.
* Add `Average`. One number per buffer: the mean power.
* Right-click `Average` and select `Scalar` > `Val0`.
* Add another `Pow` and set `Power` to 0.5. This is the square root, so the number is back
  in the units of the waveform. Engineers call the whole chain RMS.
* Add a `PublishSubject` named `Loudness`.
* Run and open the visualizer on `Loudness`. Whisper. Shout.

> [!NOTE]
> The raw value depends on your microphone and its gain. Note the range you see when quiet
> and when loud, you will map it in the next part. With a laptop microphone, a quiet room
> sits around 30 to 50, talking around 100 to 300, and shouting reaches 1000 or more. The
> checkpoints treat 600 as "loud".

> [!TIP]
> Why convert to decimals first? Squaring a loud sample gives a number too big for the
> microphone's whole-number format, so it gets cut off at the top. Without `ConvertScale`
> the loudness can never climb past about 180, however loud the room gets.

### **Exercise 7:** Spectrum

:::workflow
![Spectrum](../workflows/02-live-demo-07.bonsai)
:::

A Fourier transform splits each buffer of sound into its frequencies, from low to high.

* In a new branch from `AudioCapture`, add `DiscreteFourierTransform`, then `Magnitude`.
* Run and open the visualizer on `Magnitude`. Notice the peaks sit at both edges. The
  transform outputs every frequency twice, mirrored, with the low notes at each end.
* Add a `Submatrix` after `Magnitude` and set `EndCol` to 60. Each column is 100 Hz wide, so
  this keeps 0 to 6000 Hz, which covers voices and whistles, with low notes on the left.
* Open the visualizer on `Submatrix`. Whistle and watch the peak slide as the pitch changes.
* **Question:** hum low, then high. Which end of the plot lights up each time?

### **Exercise 8:** Winamp-style visualizers

<!-- TODO(nick): placeholder. Nick's visualizers: Winamp-style visuals with their presets,
driven by audio channels after microphone capture and audio decomposition in Bonsai.
Fill in: the package or files to add to .bonsai/Bonsai.config, the decomposition step
(which bands or features, built from Exercises 5 to 7), how the channels wire into the
visualizer, how to switch presets, and a checkpoint saved as
docs/workflows/02-live-demo-08.bonsai. Then add the :::workflow container above this
comment. -->

The microphone is now a handful of numbers: loudness, and the spectrum split into bands.
That is exactly what a music visualizer eats. Here we wire those channels into a
Winamp-style visualizer and flick through its presets while the room makes noise.

* *Steps to come.*

## Part 3: Combine

Two numbers, `Motion` and `Loudness`, and a picture. Now the sound changes the picture.

### **Exercise 9:** Sound sets the threshold

:::workflow
![Sound sets the threshold](../workflows/02-live-demo-09.bonsai)
:::

* In the camera branch, add a `Threshold` transform after `Grayscale`. Set
  `ThresholdValue` to 128 and run: a black and white cut-out.
* Add a `SubscribeSubject`, set its `Name` to `Loudness`, and add `Rescale` after it. Set
  `Min` and `Max` to the quiet and loud values you noted, `RangeMin` to 255,
  `RangeMax` to 0 and `RescaleType` to `Clamp`.
* Add a `PropertyMapping` after `Rescale`. In its `PropertyMappings`, add `ThresholdValue`.
* Connect `PropertyMapping` into `Threshold`. Every number that flows in now overwrites the
  `ThresholdValue` property.
* Run. Silence: mostly black. Talk: the picture floods in.
* **Question:** swap `RangeMin` and `RangeMax`. What does sound do now?

### **Exercise 10:** Sound draws the lines

:::workflow
![Sound draws the lines](../workflows/02-live-demo-10.bonsai)
:::

`Canny` from Exercise 2 finds edges using two thresholds. High thresholds keep only the
strongest edges; low ones let every faint line through. Hand those thresholds to the room.

* Replace `Threshold` with `Canny`, after `Grayscale`.
* Add a `SubscribeSubject` to `Loudness`, then `Rescale` with `Min` and `Max` from your notes,
  `RangeMin` 200, `RangeMax` 10 and `RescaleType` `Clamp`. Add a `PropertyMapping` after it
  for `Threshold1`, and connect it into `Canny`.
* From the same `SubscribeSubject`, add a second `Rescale` with `RangeMin` 400 and
  `RangeMax` 30, then a `PropertyMapping` for `Threshold2`, also into `Canny`.
* Run and open the visualizer on `Canny`. Quiet: a few bold outlines. Loud: the picture
  fills with lines.
* **Question:** what happens if you drive the thresholds from `Motion` instead?

## Part 4: Shaders

So far the CPU has done all the work. A **shader** is a tiny program that runs on the GPU
once for every pixel on the screen, thousands at a time. We hand it the camera image as a
*texture* and let it paint the window.

### **Exercise 11:** Camera on the GPU

:::workflow
![Camera on the GPU](../workflows/02-live-demo-11.bonsai)
:::

The shader files are in the `shaders` folder at the top of the workshop folder: `quad.vert`
places a full-screen rectangle and `camera.frag` colours it from a texture.

* Add a `CreateWindow` source, then `ShaderResources`, `TextureResources`, `MeshResources`
  and `LoadResources` in a chain. This branch sets the stage.
* Double-click `ShaderResources` and add a **Material**. Name it `Camera`, set
  `VertexShader` to `shaders/quad.vert` and `FragmentShader` to `shaders/camera.frag`.
* Double-click `TextureResources` and add a **Texture2D** named `CameraTexture`.
* Double-click `MeshResources` and add a **TexturedQuad** named `Quad`.
* In a new branch, add a `RenderFrame` source, then `BindTexture` with `ShaderName`
  `Camera` and `TextureName` `CameraTexture`, then `DrawMesh` with `ShaderName` `Camera`
  and `MeshName` `Quad`.
* Go back to `CameraCapture` and add a `PublishSubject` named `Image` after it. In a third
  branch, add a `SubscribeSubject` to `Image`, then `Flip` (`Mode` `Vertical`, textures are
  upside down) and `UpdateTexture` with `TextureName` `CameraTexture`.
* Run. The camera is now drawn by the GPU, in its own window.

> [!WARNING]
> If Bonsai says it cannot find `shaders\quad.vert`, your workflow is not open from the
> workshop folder. Shader paths are relative to the open workflow's folder. Choose
> **File > Open**, open `playground.bonsai` from the workshop folder, and paste again. Or
> save your own workflow at the top of the workshop folder and reopen it.

### **Exercise 12:** Warp

:::workflow
![Warp](../workflows/02-live-demo-12.bonsai)
:::

`camera.frag` has two `uniform` inputs, `time` and `amount`. It is four short steps: ripple
the picture, read the camera colour, turn brightness into a rainbow, then blend the rainbow
in. `amount` controls both the ripple and the blend, so a whisper barely moves the picture
and a shout bends it into bands of colour. A uniform is a value the workflow can set from
outside the shader.

* From `RenderFrame`, create a branch and select `TimeStep` > `ElapsedTime`. Add
  `Accumulate`, then `ExpressionTransform` with `Expression` set to `Convert.ToSingle(it)`,
  then `UpdateUniform` with `ShaderName` `Camera` and `UniformName` `time`.
* Add a `SubscribeSubject` to `Loudness`, then `Rescale` (`Min` 60, `Max` 600, `RangeMin` 0,
  `RangeMax` 0.3, `RescaleType` `Clamp`), then `ExpressionTransform` with
  `Convert.ToSingle(it)`, then `UpdateUniform` with `UniformName` `amount`. Setting `Min`
  to 60 instead of 0 means background chatter leaves the picture still; only real noise
  moves it.
* Run. Speak and the picture ripples. Shout and the colours bleed into rainbow.
* **Optional:** open `camera.frag` in a text editor, change the `20.0` in the `sin` line
  to `5.0`, save, and restart the workflow. Shaders reload on start, so restart after every
  edit.

## Part 5: Control

Any stream can drive a uniform, so anything Bonsai can read becomes a control.

> [!NOTE]
> The checkpoints in this part are single branches, not whole workflows. Paste one next to
> your Exercise 12 workflow. First delete the `Loudness` branch that sets `amount`, or the
> two will fight over the same uniform.

### **Exercise 13:** Keyboard

:::workflow
![Keyboard control](../workflows/02-live-demo-13.bonsai)
:::

* Add a `KeyDown` source from **Shaders** (it listens to the shader window) and set `Key` to
  `Up`. After it add a `Float` and set `Value` to 0.03.
* Add a second `KeyDown` with `Key` `Down`, and a `Float` with `Value` -0.03.
* Join the two with `Merge`, add `Accumulate`, then `UpdateUniform` for `amount`.
* Run, click the shader window, and press the arrow keys. Each press nudges the warp.

### **Exercise 14:** Mouse

:::workflow
![Mouse control](../workflows/02-live-demo-14.bonsai)
:::

* Add a `MouseMove` source from **Shaders**, then `NormalizedDeviceCoordinates` and select
  `X`.
* Add `ExpressionTransform` with `Convert.ToDouble(it)`, so `Rescale` can take the number.
* Add `Rescale` (`Min` -1, `Max` 1, `RangeMin` 0, `RangeMax` 0.3, `RescaleType` `Clamp`),
  `ExpressionTransform` with `Convert.ToSingle(it)`, and `UpdateUniform` for `amount`.
* Run and slide the mouse across the shader window.

### **Exercise 15:** Out to the world

:::workflow
![Send Motion and Loudness over OSC](../workflows/02-live-demo-15.bonsai)
:::

Bonsai does not have to draw the picture itself. It is just as happy being the sensing
layer for another tool: it reads the cameras, microphones and hardware, turns them into a
few clean numbers, and hands them to TouchDesigner, Max, Processing, a DAW, or a second
laptop. On stage we send the two numbers from this demo to a TouchDesigner patch.

* Add `CreateUdpClient` (from **Osc**). Set `Name` to `Out`, `RemoteHostName` to
  `127.0.0.1` and `RemotePort` to 8000. Use another machine's IP address to send across the
  network instead.
* Add a `SubscribeSubject` to `Motion`, then `Rescale` with `Max` set to a "lots of
  movement" value from your visualizer, `RangeMin` 0, `RangeMax` 1 and `RescaleType`
  `Clamp`. The number now always sits between 0 and 1.
* Add `ExpressionTransform` with `Convert.ToSingle(it)`, then `SendMessage` with
  `Connection` `Out` and `Address` `/br/motion`.
* Repeat for `Loudness`, sending to `/br/state/activation`.
* Run. Anything listening on port 8000 now receives two named channels. In TouchDesigner,
  an **OSC In** CHOP on port 8000 shows `br/motion` and `br/state/activation` straight away.
* **Question:** the addresses are just names. What would you call the channels if you were
  building a piece of your own?

> [!TIP]
> Only one program can listen on a port. If nothing arrives, check that no other app (or a
> second copy of Bonsai) is already using port 8000.

That is the whole demo. Next: [Fork It](03-fork-it.md).
