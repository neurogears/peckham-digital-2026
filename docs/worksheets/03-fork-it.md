# Fork It

You have the whole demo on your machine. Now break it. Everything below is a nudge, not a
recipe: pick one, try it, then follow whatever it makes you curious about. Shout if you get
stuck. Show a neighbour when it works.

Start from the last checkpoint you reached in [The Live Demo](02-live-demo.md), or copy the
Exercise 14 workflow for the complete thing.

## Swap a sensor

* **Motion instead of sound.** Change the `SubscribeSubject` feeding `amount` from
  `Loudness` to `Motion`. Now the picture warps when you move.
* **Position instead of the mouse.** If you did Exercise 4, feed `Position` into the
  uniform instead of `MouseMove`. A coloured object becomes the controller.
* **Two microphones, two numbers.** Add a second `AudioCapture` with a different
  `DeviceName` and a second loudness chain. Drive `time` from one and `amount` from the
  other.

The webcam and microphone are just the sensors every laptop already has. Bonsai talks to a huge range of others: Arduinos and anything you can wire to one (buttons, knobs, light, distance, pressure, temperature), game controllers, MIDI and OSC devices, depth cameras,
high-speed and machine-vision cameras, eye trackers, heart rate, breathing and brain activity sensors, and lab hardware built for timing to the millisecond. Many have a ready-made package in **Tools > Manage Packages**.

If you have a device Bonsai can't speak to, get in touch!

## Change a transformation

* **Before the GPU.** Put `Canny`, `Threshold`, `Smooth` or `ConvertColor` between `Image` and `UpdateTexture`. The shader warps whatever it is given.
* **Inside the GPU.** Open `shaders/camera.frag` from the workshop folder in a text editor. Try...
  - swapping `uv.x` and `uv.y` 
  - multiplying the colour by `vec3(1.0, 0.3, 0.3)`
  - replacing the `sin` with `abs(sin(...))`; 
  - adding a second ripple at a different frequency. 

* **Delay a stream** Put a `Delay` before `UpdateUniform` for `amount` and set `DueTime` to `00:00:00.5`. The picture reacts half a second late.
* **Smooth it out.** Add a `Buffer` to collect the last 10 values from a stream, then add an `Average`. Twitchy becomes swoopy.

## Remap input to output

* **Sound to sound.** Add `AudioReader` with a short sample and `AudioPlayback`. Trigger it
  when `Motion` crosses a value: `GreaterThan`, then `DistinctUntilChanged`, then
  `Condition`, then use the result to gate the playback.
* **Picture to picture.** Draw `Position` onto the camera image with `Circle` before it
  goes to the texture. You are painting with the tracked object.
* **Everything to the shader.** `camera.frag` reads two uniforms. Add a third, `uniform float hue;`, use it in the colour, and drive it from whichever stream you like.
* **Out to the world.** Send `Loudness` to another program with `SendMessage` from **Osc**.
  Anything that speaks OSC (a DAW, TouchDesigner, Processing, a second Bonsai) can listen.

## Bigger

* Build a second window with `CreateWindow` and put a different shader in it.
* Record the whole performance: `VideoWriter` on the camera branch and `AudioWriter` on
  the microphone, started together.
* Replace the webcam with `FileCapture` and a video file, and run the installation on
  footage instead of a live feed.

Keep the workflow. Everything you built today is a text file you can email, put on GitHub,
or open on any Windows machine with Bonsai installed. The
[Bonsai documentation](https://bonsai-rx.org/docs) and the
[community forum](https://github.com/orgs/bonsai-rx/discussions) are where to go next.
