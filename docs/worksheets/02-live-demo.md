# The Live Demo

The plan:

1. **Camera**: acquire, transform, measure motion, track a coloured object.
2. **Microphone**: acquire, measure loudness, split the sound into frequencies.
3. **Combine**: let the sound change the picture.
4. **Out to the world**: read a sensor and light an LED through an Arduino, and send the room's numbers to other programs such as TouchDesigner.
5. **Shaders**: put the picture on the GPU, warp it, build a music visualizer, and bend it with the keyboard or mouse.

Every workflow you see below can be copy-pasted directly onto your canvas, or even as text in an email or message.

## Part 1: Camera

Bonsai sees the world as streams. A camera is a stream of images. Every operator we add turns one stream into another, and every stream can be watched while it runs.

### **Exercise 1:** Acquire and publish the camera image

:::workflow
![Camera capture in grayscale](../workflows/02-live-demo-01.bonsai)
:::

* Start from the workflow you built in [Getting Started](01-getting-started.md), a `CameraCapture` source feeding a `Grayscale` transform.
* Select `CameraCapture`, add a `PublishSubject` and set its `Name` to `Image`.
* Move `Grayscale` off the camera: delete it, then click the empty canvas, search the toolbox for `Image` and add it. Add `Grayscale` after it.
* Press **Start** and double-click `Grayscale` to open its visualizer.

> [!NOTE]
> A subject is a named stream. If we publish the camera image to a subject called `Image`, it appears in the toolbox, and we can subscribe to it anywhere else in the workflow as many times as we like.

### **Exercise 2:** Transform

:::workflow
![Transform the camera image](../workflows/02-live-demo-02.bonsai)
:::

* Add a `Canny` transform after `Grayscale`. Set `Threshold1` to 50 and `Threshold2` to 150. Run, and open the visualizer on `Canny`: edges only.
* There are many transforms available in the **Vision** package, searchable in the toolbox. Try some others: `Threshold`, `Smooth`, `Flip`.

### **Exercise 3:** Motion energy

:::workflow
![Motion energy](../workflows/02-live-demo-03.bonsai)
:::

We want one number that says "how much is moving right now".

* Search the toolbox for `Image` and add it, then `Grayscale`, then a `BackgroundSubtraction` transform. Set `AdaptationRate` to 0.1. Run: still parts fade to black, moving parts glow.
* Add a `Sum` transform (the one from **Dsp**). It adds up every pixel.
* Right-click `Sum` and select `Scalar` > `Val0` so the output is a plain number.
* Publish this as `Motion`. Like `Image`, any later branch can now pick it up.
* Run and open the visualizer on `Motion`. Hold still. Wave.

### **Exercise 4:** Track a coloured object

:::workflow
![Track a coloured object](../workflows/02-live-demo-04.bonsai)
:::

* Add `Image` from the toolbox, then `ConvertColor`, and set `Conversion` to `Bgr2Hsv`.
* Add `HsvThreshold`. For a blue object, set `Lower` to 100, 100, 100 and `Upper` to 130, 255, 255. Run, open its visualizer, and adjust the bounds until only your object is white. Hue is the first value.
* Add `FindContours`, then `BinaryRegionAnalysis`, then `LargestBinaryRegion`.
* Right-click `LargestBinaryRegion` and select `ConnectedComponent` > `Centroid`. This chain of threshold, contours and largest region is commonly known as blob tracking.
* Select every node from `ConvertColor` to `Centroid`, right-click and choose **Group** (Ctrl+G). Name the group `ColourTracking`. The whole recipe is now one node; double-click it to look inside.
* Inside the group, right-click `HsvThreshold` and choose **Externalize Property** for `Lower`, then again for `Upper`. Now you can retune the colour from the group's properties without opening it.
* Publish this as `Position`.
* Run: the visualizer on `Position` draws the object's position.

## Part 2: Microphone

Audio arrives as *buffers*: chunks of a few hundred samples at a time. Bonsai treats a buffer as a matrix, so the same maths operators that work on images work on sound.

### **Exercise 5:** Acquire

:::workflow
![Audio capture](../workflows/02-live-demo-05.bonsai)
:::

* Add an `AudioCapture` source. Leave `DeviceName` empty for the default microphone.
* Publish this as `Audio`, the microphone's twin of `Image`.
* Run and open the visualizer on `AudioCapture`. Talk. Clap.
* **Optional:** add an `AudioWriter` sink and set `FileName` to a name ending in `.wav` to save the sample, run for a few seconds, stop, and play the file.

### **Exercise 6:** Loudness

:::workflow
![Loudness](../workflows/02-live-demo-06.bonsai)
:::

We want one number for "how loud is it right now", the audio twin of `Motion`.

* Add `Audio` from the toolbox, then `ConvertScale`, and set `Depth` to `F32`. The microphone records 16-bit whole numbers, which top out at 32767, and squaring a loud sample in the next step would overflow them. 32-bit decimals have room; without this step the loudness never climbs past about 180.
* Add `Pow` (from **Dsp**) and set `Power` to 2. Negative and positive swings now both count.
* Add `RunningAverage` with `Alpha` 0.2. This blends each buffer with the ones before it, so the number moves smoothly instead of jittering. A smaller `Alpha` is smoother but slower to react.
* Add `Average`. One number per buffer: the mean power.
* Right-click `Average` and select `Scalar` > `Val0`.
* Add another `Pow` and set `Power` to 0.5. This is the square root, so the number is back in the units of the waveform. This chain is called the root-mean-square, or RMS.
* Select every node from `ConvertScale` to the second `Pow`, right-click and choose **Group** (Ctrl+G). Name the group `RMS`. We will not need to open it again.
* Publish this as `Loudness`.
* Run and open the visualizer on `Loudness`. Whisper. Shout.

> [!NOTE]
> The raw value depends on your microphone and its gain. Make a note of the range you see when quiet and when loud, you will map it in the next part.

### **Exercise 7:** Spectrum

:::workflow
![Spectrum](../workflows/02-live-demo-07.bonsai)
:::

A Fourier transform splits each buffer of sound into its frequencies, from low to high.

* Add `Audio` from the toolbox, then `DiscreteFourierTransform`, then `Magnitude`.
* Run and open the visualizer on `Magnitude`. Notice the peaks sit at both edges. The transform outputs every frequency twice, mirrored, with the low notes at each end.
* Add a `Submatrix` after `Magnitude` and set `EndCol` to 60. Each column is 100 Hz wide, so this keeps 0 to 6000 Hz, which covers voices and whistles, with low notes on the left.
* Open the visualizer on `Submatrix`. Whistle and watch the peak slide as the pitch changes.

## Part 3: Combine

We can change an operator's properties in the property editor while the workflow is running, but they can also be driven by another sequence in the workflow.

### **Exercise 8:** Volume decides the thresholds

:::workflow
![Volume decides the thresholds](../workflows/02-live-demo-08.bonsai)
:::

The `Canny` edge detection operator we used in Exercise 2 has two thresholds, which you can modify by hand in the property editor, even while the workflow is running. These properties can also be externalized and driven from any other stream in the workflow in real time.

* Right-click the `Canny` node from Exercise 2 and choose **Externalize Property** > `Threshold1`. A property node appears, feeding `Canny`. Every number that flows into it now overwrites `Threshold1`.
* Our `Loudness` subject carries numbers far higher than we would want to set on `Canny`, whose thresholds do their useful work between about 10 and 400. So we need to rescale the data to match the range it expects. Search the toolbox for `Loudness` and add it, then add `Rescale` after it with `Min` and `Max` set to the quiet and loud values you noted, `RangeMin` 200, `RangeMax` 10 and `RescaleType` `Clamp`. Because `RangeMin` is bigger than `RangeMax`, `Rescale` also flips the direction, so loud means low thresholds and more lines. Connect `Rescale` into the `Threshold1` node.
* From the same `Loudness` node, add a second `Rescale` with `RangeMin` 400 and `RangeMax` 30. Externalize `Threshold2` from `Canny` the same way and connect this `Rescale` into it.
* Run and open the visualizer on `Canny`. Quiet: a few bold outlines. Loud: the picture fills with lines.
* **Question:** can you drive the thresholds from the `Motion` number you made in Exercise 3 instead? What does moving do to the picture?

## Part 4: Out to the world

So far every sensor has been on your laptop, and every number has remained inside Bonsai. However, Bonsai can talk to a wide variety of hardware and devices: cameras, microcontrollers, game controllers. If Bonsai cannot speak to your device, please get in touch!

### **Exercise 9:** A sensor and an LED

:::workflow
![A sensor lights an LED](../workflows/02-live-demo-09.bonsai)
:::

One of the most common microcontrollers is the Arduino, and with the Firmata sketch on it every pin becomes a stream Bonsai can read or write. The kit on the table has dials, light, sound, distance and touch sensors, and LEDs, buzzers and motors to drive. This exercise reads one sensor and lights one LED.

* Plug the Arduino into USB and a sensor (the rotation dial, the light sensor or the sound sensor) into the `A0` socket on the shield. Pin 13 is the built-in LED on the Arduino itself, but you can drive any output pin on the board.
* Add `CreateArduino`. Set `Name` to `Board` and `PortName` to the board's port.
* Add `AnalogInput`. Set `PortName` to `Board` and `Pin` to 0. Run and open its visualizer: a number from 0 to 1023. Activate your sensor and watch how far the number moves. Note where it sits at rest and where it goes when you activate it.
* Add `GreaterThan` after it and set `Value` to a number between those two readings, so that activating the sensor crosses it. The number becomes true or false.
* Add `DistinctUntilChanged`. The sensor reports many times a second, so without it the board is sent `true` over and over; now it only hears about it when the answer flips.
* Add `DigitalOutput`. Set `PortName` to `Board` and `Pin` to 13, the LED built into the board.
* Run the workflow, and flash your LED using your sensor.
* **Into the demo:** try connecting your sensor to the `Canny` thresholds from Exercise 8 in place of `Loudness`. Set `Min` and `Max` on both `Rescale` nodes to your sensor's rest and active readings.

> [!TIP]
> `AnalogInput` gives whole numbers, but `Rescale` expects a `double`. Add an `ExpressionTransform` with `Convert.ToDouble(it)` in between to convert the number to the type it expects.

### **Exercise 10:** Send to other programs

:::workflow
![Send Motion and Loudness over OSC](../workflows/02-live-demo-10.bonsai)
:::

Bonsai does not have to draw the picture itself. It is just as happy being the sensing layer for another tool: it reads the cameras, microphones and hardware, turns them into a few clean numbers, and hands them to TouchDesigner, Max, Processing, a DAW, or a second laptop. Here we send the two numbers from this demo to a TouchDesigner patch called Ember Pulse, and you can take it home: it is `touchdesigner/ember-pulse.toe` in the workshop folder, and it opens in the free version of [TouchDesigner](https://derivative.ca/download).

Ember Pulse has five dials, each a number from 0 to 1 with a name: `/pulse` for the heartbeat, `/spin` for the rotation, `/zoom`, `/hue` for the colour and `/trail` for how long the trails last. A dial nobody sends stays at rest. We will turn two of them.

* Add `CreateUdpClient` (from **Osc**). Set `Name` to `Out`, `RemoteHostName` to `127.0.0.1` and `RemotePort` to 8000. Use another machine's IP address to send across the network instead.
* Add `Motion` from the toolbox, then `Rescale`, `ExpressionTransform` and `SendMessage` with `Connection` `Out` and `Address` `/pulse`. The example TouchDesigner sketch expects normalised float values between 0 and 1, so we rescale and convert the type to match. Set the `Rescale` `Max` to a "lots of movement" value from your `Motion` visualizer and `RescaleType` to `Clamp`, and the `ExpressionTransform` to `Convert.ToSingle(it)`; the workflow above has the rest of the values.
* Repeat for `Loudness`, with the loud value you noted as `Max`, and send it to `/hue`.
* Run. Anything listening on port 8000 now receives two named channels.
* If you have the free version of TouchDesigner installed, you can immediately open `touchdesigner/ember-pulse.toe`. Move and the heartbeat quickens and the embers thicken. Make noise and the colour drifts.
* **Optional:** the addresses are just names. Try sending `Motion` to `/hue`, and `Loudness` to `/pulse`. Or `Loudness` to `/spin`.

> [!TIP]
> Only one program can listen on a port. If nothing arrives, check that no other app (or a second copy of Bonsai) is already using port 8000.

## Part 5: Shaders

TouchDesigner turns our numbers into pictures with shaders. Bonsai has a shaders package of its own, so we can build the picture right here too.

So far the CPU has done all the work. A **shader** is a small program that runs on the GPU, which runs it for every pixel of the window at the same time. We hand it the camera image as a *texture*, it decides the colour of each pixel, and the window is painted.

A shader is two text files, both in the `shaders` folder at the top of the workshop folder. `quad.vert` stretches a rectangle over the whole window and `camera.frag` decides the colour of every pixel in that rectangle. Before it can draw, Bonsai needs to know three things: which shader to use, which image to hand it, and what shape to draw it on.

Keep working in `playground.bonsai`, or another workflow saved in the workshop folder, because the shader files are found relative to it.

### **Exercise 11:** Warp

:::workflow
![Warp](../workflows/02-live-demo-11.bonsai)
:::

A shader cannot see the rest of the workflow. Its way in is a **uniform**: a named number the shader declares and the workflow sets with `UpdateUniform`. `camera.frag` has two. `amount` drives both the ripple and the colour shift, so a whisper barely moves the picture and a shout bends it into bands of colour. `time` makes the ripple flow.

* Copy the workflow above onto your canvas and run it. Speak and the picture bends. Shout and the colours bleed into rainbow.
* **The setup.** The first three branches set up the shader, draw each frame, and upload `Image` as the texture.
* **The `amount` branch.** `Loudness` goes through `Rescale` (`Min` 60, `Max` 600, `RangeMin` 0, `RangeMax` 0.3, `RescaleType` `Clamp`), then an `ExpressionTransform` with `Convert.ToSingle(it)`, because uniforms take floats, then `UpdateUniform` with `ShaderName` `Camera` and `UniformName` `amount`. Setting `Min` to 60 instead of 0 means background chatter leaves the picture still; only real noise moves it.
* **The `time` branch.** From `RenderFrame`, `TimeStep` > `ElapsedTime` gives the time since the last frame. `Accumulate` adds these up into a running clock, which goes through `Convert.ToSingle(it)` into `UpdateUniform` with `UniformName` `time`. This is what makes the ripple flow down the picture; without it the bend grows and shrinks but does not move.
* **Optional:** make it flow only while people move: in the `time` branch, replace `ElapsedTime` with `Motion` from the toolbox and a `Rescale` (`Min` 0, `Max` your "lots of movement" value, `RangeMin` 0, `RangeMax` 0.1, `RescaleType` `Clamp`), and keep `Accumulate` after it. Every bit of movement nudges the wave along; hold still and it freezes.
* **Optional:** open `camera.frag` in a text editor, change the `20.0` in the `sin` line to `5.0`, save, and restart the workflow. Shaders reload on start, so restart after every edit.

Any stream can drive a uniform, so anything Bonsai can read becomes a control.

### **Exercise 12:** Keyboard

:::workflow
![Keyboard control](../workflows/02-live-demo-12.bonsai)
:::

* Delete the `Loudness` branch that sets `amount` first, or the two will fight over the same uniform.
* Add a `KeyDown` source from **Shaders** and set `Key` to `Up`. It only hears keys while the shader window has focus, so click that window before pressing anything. After it add a `Float` and set `Value` to 0.03.
* Add a second `KeyDown` with `Key` `Down`, and a `Float` with `Value` -0.03.
* Join the two with `Merge`, add `Accumulate`, then `UpdateUniform` for `amount`.
* Run, click the shader window, and press the arrow keys. Each press nudges the warp.

### **Exercise 13:** Mouse

:::workflow
![Mouse control](../workflows/02-live-demo-13.bonsai)
:::

* Replace the keyboard with the mouse cursor: delete the two `KeyDown` branches from Exercise 12 first.
* Add a `MouseMove` source from **Shaders**, then `NormalizedDeviceCoordinates` and select `X`.
* Add `ExpressionTransform` with `Convert.ToDouble(it)`, so `Rescale` can take the number.
* Add `Rescale` (`Min` -1, `Max` 1, `RangeMin` 0, `RangeMax` 0.3, `RescaleType` `Clamp`), `ExpressionTransform` with `Convert.ToSingle(it)`, and `UpdateUniform` for `amount`.
* Run and slide the mouse across the shader window.

### **Exercise 14:** Winamp Visualizer

:::workflow
![Winamp Visualizer](../workflows/02-live-demo-14.bonsai)
:::

[Winamp's MilkDrop](https://en.wikipedia.org/wiki/MilkDrop) visualizer works on one main trick. Every frame, the GPU reads back the picture it drew last frame, zooms and rotates it, dims it a little, and paints fresh colour on top. Sound decides how much zoom, how much spin and how much colour. A MilkDrop preset is a list of equations that map bass, mid and treble onto those dials. In this exercise, we build such a preset by hand to demonstrate how this would work in Bonsai. The shaders in `shaders/warp.frag`, `shaders/copy.frag` and `shaders/screen.frag` are the equations, and the workflow provides the plumbing between audio and visualization.

The workflow has three parts: a feedback loop on the GPU, an audio analysis pipeline that extracts bass, mid, and treble from the microphone, and then uniforms that connect these streams together.

It brings its own window and shaders, which clash with those of Exercise 11. Select the Exercise 11 branches, right-click and choose **Disable**, then paste this workflow beside the rest of your demo. It listens to the `Audio` subject from Exercise 5, so keep that running.

* **The loop.** The `Textures` operator defines `DefaultFeedback` and `DefaultWork` textures to hold the last frame on the GPU (feedback) and the current frame being drawn (work). Each `RenderFrame` runs four `DrawMesh` calls in order. `DefaultWarp` samples `DefaultFeedback`, zooms, rotates and fades it, adds a wandering blob of colour, and draws into `DefaultWork`. `DefaultWaveform` draws the raw audio signal as a line on top, which is fed into the waveform shader directly as vertex buffer data. `DefaultCopy` copies `DefaultWork` back into `DefaultFeedback`, ready for the next frame. `DefaultScreen` draws `DefaultWork` to the window with a mirrored echo, a hue drift and a vignette, none of which feed back. Each shader's target texture is set under `FramebufferAttachments` in `ShaderResources`. With this pipeline, we get one shader that draws to the screen and one shader that remembers and warps what was previously drawn.
* **Audio in.** The `Audio` subject from Exercise 5 delivers samples at 44100 Hz. They are rescaled to -2 to 2, and `Buffer` with `Count` 1024 and `Skip` 100 cuts the stream into overlapping windows of 1024 samples, a new one every 100 samples. One branch sends the buffered audio data straight to `UpdateVertexBuffer` as the line `DefaultWaveform` draws.
* **Spectrum.** As in Exercise 7, the audio is transformed into the frequency domain with `DiscreteFourierTransform` then `Magnitude`, keeping the first 512 columns as `Power`. With 1024 samples at 44.1 kHz, each column is about 43 Hz wide.
* **Slow average.** A second branch from `Magnitude` goes through `IirFilter` with coefficients `0,0.01` and `1,-0.99`. Every column now creeps one percent of the way toward its new value each buffer, so it remembers what the room has sounded like for the last few seconds. This is `Attenuated`.
* **Three bands.** `Submatrix` cuts columns 0 to 6 (bass, below 250 Hz), 6 to 93 (mid, below 4 kHz) and 93 to 465 (treble, below 20 kHz) from both `Power` and `Attenuated`. `Sum`, then `Val0`, then `single(it)` turn each slice into one float, and `UpdateUniform` hands it to the shaders as `bass`, `mid`, `treble` and their `_att` twins.
* **Louder than baseline.** The shader never uses a band on its own. It divides each one by its slow average: `bass / bass_att` is 1 when the room sounds as it usually does, 3 on a kick drum. That ratio sets the zoom, `mid / mid_att` sets the spin, and `treble / treble_att` sets how bright the blob paints. Because it is a ratio, the picture reacts the same whatever the microphone gain, which is why there is no `Rescale` to tune this time.
* Run. Silence: a slow spiral fading to black. Music: the picture breathes with the bass and sparkles with the hi-hats.
* **Optional:** `warp.frag` declares its dials as uniforms with defaults, such as `prev_scale` for the fade and `zoom_scale` for how hard the bass pushes. How would you modify the Bonsai workflow to allow changing these dials dynamically? Hint: take a look at how `bass`, `bass_att` are fed in as uniforms with `UpdateUniform`.
* **Question:** swap the `UniformName` on the mid and bass branches so the drums spin the picture and the voice zooms it. Which feels more musical?

Next: [Fork It](03-fork-it.md).
