# Fork It

You have the whole demo on your machine. Now break it. Everything below is just a nudge, not a recipe. Follow whatever you are curious about. Shout if you get stuck or have an idea you want help with. Show a neighbour when something works.

Keep building on the workflow you made in [The Live Demo](02-live-demo.md). If you missed a step, copy that exercise's workflow onto your canvas. For bigger finished projects to pull apart, see the [Showcase](../examples/index.md): pose-driven visuals, a camera that follows you, and MilkDrop-style visualizers.

## Swap a sensor

* **Motion instead of sound.** Replace the `Loudness` node feeding `amount` with `Motion` from the toolbox. Now the picture warps when you move. Don't forget to rescale it properly.
* **Position instead of the mouse.** If you did Exercise 4, feed `Position` into the uniform instead of `MouseMove`. `Position` is a point in pixels, so right-click it to select `X` and rescale it first. A coloured object becomes the controller.
* **Two microphones, two numbers.** Add a second `AudioCapture` with a different `DeviceName` and a second loudness chain. Drive `time` from one and `amount` from the other.
* **The kit on the table.** Exercise 9 reads one sensor and lights one LED. Swap it for a distance, touch or sound sensor, send its number into `amount`, or drive a buzzer or a servo instead of the LED. Ask for a board if you have not got one.

The webcam and microphone are just the sensors every laptop already has. Bonsai talks to a huge range of others: Arduinos and anything you can wire to one (buttons, dials, light, distance, pressure, temperature), game controllers, MIDI and OSC devices, depth cameras, high-speed and machine-vision cameras, eye trackers, heart rate, breathing and brain activity sensors, and lab hardware built for timing to the millisecond. Many have a ready-made package in **Tools > Manage Packages**.

## Change a transformation

* **Before the GPU.** Put `Canny`, `Threshold`, `Smooth` or `ConvertColor` between `Image` and `UpdateTexture`. The shader warps whatever it is given.
* **Inside the GPU.** Open `shaders/camera.frag` from the workshop folder in a text editor. Try:
  - swapping `uv.x` and `uv.y`
  - multiplying the colour by `vec3(1.0, 0.3, 0.3)`
  - replacing the `sin` with `abs(sin(...))`
  - adding a second ripple at a different frequency
* **Delay a stream.** Put a `Delay` before `UpdateUniform` for `amount` and set `DueTime` to `00:00:00.5`. The picture reacts half a second late.
* **Smooth it out.** Add a `Buffer` to collect the last 10 values from a stream, then add an `Average`. Twitchy becomes swoopy.

## Remap input to output

* **Sound to sound.** Add `AudioReader` with a short sample and `AudioPlayback`. Trigger it when `Motion` crosses a value: `GreaterThan`, then `DistinctUntilChanged`, then `Condition`, then use the result to gate the playback.
* **Picture to picture.** Draw `Position` onto the camera image with `Circle` before it goes to the texture. You are painting with the tracked object.
* **Everything to the shader.** `camera.frag` reads two uniforms. Add a third, `uniform float hue;`, use it in the colour, and drive it from whichever stream you like.
* **Out to the world.** Exercise 10 sends two numbers to Ember Pulse. Send a third: the `X` of `Position`, rescaled to 0 to 1, to `/spin`, or the bass band from Exercise 14 to `/zoom`. Anything that speaks OSC (a DAW, TouchDesigner, Processing, a second Bonsai) can listen.
* **Machine to machine.** Pair up with a neighbour and drive each other's pictures. Find your IP address (type `ipconfig` in a Command Prompt) and swap. Set `RemoteHostName` on your `CreateUdpClient` to their address. On their side, a `CreateUdpClient` named `In` with `Port` 8000 listens; `ReceiveMessage` on `In`, then `Parse` with `Address` `/pulse` and `TypeTag` `f`, turns your movement back into a number they can send into their own shader. If nothing arrives, the venue wifi may block it; a phone hotspot works.

## Bigger

* Record the whole performance: `VideoWriter` on the camera branch and `AudioWriter` on the microphone, started together.
* Replace the webcam with `FileCapture` and a video file, and run the installation on footage instead of a live feed.

Keep the workflow. Everything you built today is a text file you can email, put on GitHub, or open on any Windows machine with Bonsai installed. The [Bonsai documentation](https://bonsai-rx.org/docs) and the [community forum](https://github.com/orgs/bonsai-rx/discussions) are where to go next.
