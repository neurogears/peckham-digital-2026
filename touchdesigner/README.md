# TouchDesigner: Ember Pulse

`ember-pulse.toe` is a TouchDesigner visual driven live by Bonsai. Open it in the free
version of [TouchDesigner](https://derivative.ca/download), run the live demo's Exercise 15
workflow in Bonsai, and move and make noise in front of the laptop.

| OSC address | Range | Drives |
| --- | --- | --- |
| `/motion` | 0 to 1 | heartbeat speed, ember density, spin and zoom |
| `/loudness` | 0 to 1 | trail length, ring size, colour drift |

It listens on UDP port 8000. With nothing sending, both inputs sit at 0 and it idles as a
slow heartbeat. Tune the look on the `ember_pulse` container's **Ember** parameter page.

To use these channels in your own project, add an **OSC In** CHOP on port 8000: `motion`
and `loudness` appear as channels as soon as Bonsai starts sending.
