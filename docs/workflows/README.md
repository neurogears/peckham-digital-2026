# Workflows

Completed Bonsai workflows that accompany the worksheets. A worksheet embeds one with the
`:::workflow` container:

```markdown
:::workflow
![Alt text](../workflows/02-live-demo-03.bonsai)
:::
```

At build time `docs/export-images.ps1` renders an SVG for every `.bonsai` file here using
the environment in `/.bonsai/`, and the site swaps the image in and adds a copy button that
puts the workflow XML on the clipboard. So nothing but the `.bonsai` files needs committing:
`.svg` and `.layout` files in this folder are ignored by git.

- Build and save every workflow from the environment in `/.bonsai/` so the package versions
  in the XML match what participants install.
- Name files after the worksheet and exercise they belong to: `02-live-demo-03.bonsai` is
  worksheet 2, exercise 3.
- The GLSL files the shader exercises load live in `shaders/` at the repository root, not
  here. Material paths are relative to Bonsai's working folder, which is the folder of
  the open workflow. Participants open the empty `playground.bonsai` at the root, so the
  root is always the working folder.

## Status

| File | Worksheet | Exercise | Status |
| --- | --- | --- | --- |
| `01-getting-started-01.bonsai` | [Getting Started](../worksheets/01-getting-started.md) | 2 Acquire the camera (camera) | renders |
| `01-getting-started-02.bonsai` | | 2 Acquire the camera (camera and grayscale) | renders |
| `02-live-demo-01.bonsai` | [The Live Demo](../worksheets/02-live-demo.md) | 1 Acquire and publish the camera image | renders |
| `02-live-demo-02.bonsai` | | 2 Transform | renders |
| `02-live-demo-03.bonsai` | | 3 Motion energy | renders |
| `02-live-demo-04.bonsai` | | 4 Track a coloured object | renders |
| `02-live-demo-05.bonsai` | | 5 Acquire audio (publishes `Audio`) | renders |
| `02-live-demo-06.bonsai` | | 6 Loudness | renders |
| `02-live-demo-07.bonsai` | | 7 Spectrum | renders |
| `02-live-demo-08.bonsai` | | 8 Volume decides the thresholds | renders |
| `02-live-demo-09.bonsai` | | 9 A sensor and an LED (Arduino, standalone) | renders |
| `02-live-demo-10.bonsai` | | 10 Send to other programs (OSC to TouchDesigner) | renders |
| `02-live-demo-11.bonsai` | | 11 Warp | renders |
| `02-live-demo-12.bonsai` | | 12 Keyboard (branch to paste beside 11) | renders |
| `02-live-demo-13.bonsai` | | 13 Mouse (branch to paste beside 11) | renders |
| `02-live-demo-14.bonsai` | | 14 Winamp Visualizer (Nick; subscribes to `Audio`, clashes with 11) | renders |
| `../../shaders/quad.vert` | | 11 | |
| `../../shaders/camera.frag` | | 11 | |
| `../../shaders/warp.frag`, `copy.frag`, `screen.frag` | | 14 | |
| `../../shaders/waveform.vert`, `waveform.frag` | | 14 | |
