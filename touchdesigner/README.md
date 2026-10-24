# TouchDesigner: Ember Pulse

`ember-pulse.toe` is a TouchDesigner visual with five knobs that Bonsai turns over OSC. Open it in the free version of [TouchDesigner](https://derivative.ca/download), run a Bonsai workflow that sends to any of the addresses below, and the picture follows.

| OSC address | Range | Drives |
| --- | --- | --- |
| `/pulse` | 0 to 1 | heartbeat rate, ember density, ring wobble and glow |
| `/spin` | 0 to 1 | rotation speed of the tunnel |
| `/zoom` | 0 to 1 | zoom rate and ring size |
| `/hue` | 0 to 1 | colour drift from embers toward violet |
| `/trail` | 0 to 1 | how long the trails persist |

It listens on UDP port 8000. A knob nobody sends sits at 0, so with no sender at all it idles as a slow heartbeat. Send one knob or all five; the addresses are just names, so the same patch works for camera motion, microphone loudness, a body part, a sensor or a second laptop.

Exercise 10 of the live demo sends `/pulse` from camera motion and `/hue` from microphone loudness. The MoveNet showcase sends `/spin` from where your nose is and `/hue` from how wide your arms are. Tune the look on the `ember_pulse` container's **Ember** parameter page.

To use these channels in your own project, add an **OSC In** CHOP on port 8000: each address appears as a channel as soon as Bonsai sends it.
