# MIDI: conduct a fugue with your body

Bonsai plays Bach's Contrapunctus IX from *The Art of Fugue* as MIDI notes into a software synthesizer, and you are the conductor. Tilt your head to change the tempo, and raise or lower your arms to turn two of the synth's dials. Bonsai does not make any sound itself here: it is the sequencer and the controller, and whatever synth is listening plays the notes.

## What you need

* A webcam. The body tracking is the same MoveNet network as the [MoveNet](../movenet/index.md) demos.
* [loopMIDI](https://www.tobias-erichsen.de/software/loopmidi.html), a free virtual MIDI cable for Windows. Bonsai sends to it and the synth listens on it.
* A software synth or DAW that takes MIDI input. The four voices of the fugue arrive on MIDI channels 1 to 4, and the two arm dials arrive as control changes 20 and 21 on channel 1.

## Setup

The `Bonsai.MidiSequencing` package is in the workshop environment from [Getting Started](../../worksheets/01-getting-started.md). If you set up before this demo was added, run `Setup.cmd` again to install it.

1. Open loopMIDI and add a port with the **+** button.
2. In your synth, set the MIDI input to the loopMIDI port. Give channels 1 to 4 an instrument each, and map control changes 20 and 21 to any two dials you like.
3. Open the workshop `Bonsai.exe`, then open `midi-conductor.bonsai` from `docs/examples/midi`.
4. Select `OutputDevice` and set `Index` to the loopMIDI port's number among your MIDI outputs. It is saved as 1.

## midi-conductor

:::workflow
![MIDI conductor](midi-conductor.bonsai)
:::

* `BodyAngles` runs the camera through MoveNet and publishes a set of body measurements, each in degrees. `HeadAngle` is the tilt of the line between your eyes, 0 when level. `LeftUpperArmAngle` and `RightUpperArmAngle` follow each shoulder to elbow line: 0 with the arm straight out to the side, +90 straight up, -90 straight down. Double-click it to see how: each measurement is one `ExpressionTransform`, after an `ExpressionCondition` that drops frames where a body part was not found.
* **The tempo.** `SequencerClock` ticks every millisecond into `Transport`, which counts the beats. `HeadAngle` goes through `Rescale`, which turns -30 to 30 degrees into 60 to 180 beats per minute, and sets the `Tempo` of `Transport`. Hold your head level for 120; tilt it one way to speed up and the other to slow down.
* **The score.** `ReadMidiFile` loads `contrapunctusIX.mid` and `PlaySequence` plays it against the transport, looping, and starting on the next four-beat boundary. Four `FilterTrack` and `SetChannel` pairs put each voice of the fugue on its own channel, so the synth can give each one a different instrument. Bonsai counts channels from 0, so `Channel` 0 to 3 here are channels 1 to 4 in the synth.
* **The dials.** Each upper arm angle goes into a `CreateControlChange`, which turns -60 to 60 degrees into the full range of control change 20 (left arm) or 21 (right arm).
* `Merge` gathers the notes and the dials into one stream and `OutputDevice` sends it to the loopMIDI port.
* Run the workflow, start your synth, and conduct.
* **Question:** `BodyAngles` also publishes `LeftHandHeight`, `RightHandHeight`, `LeftLowerArmAngle` and `RightLowerArmAngle`, and nothing reads them yet. Add a `CreateControlChange` for one of them and give the synth a third dial. Which part of your body makes the best fader?
