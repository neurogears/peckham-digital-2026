# MoveNet: pose-driven visuals

[MoveNet](https://www.tensorflow.org/hub/tutorials/movenet) is a small neural network published by Google. It finds seventeen points on a human body in a single camera frame: nose, eyes, shoulders, hips and joints. Bonsai runs it through the `Bonsai.TensorFlow.MoveNet` package, so a body part becomes just another stream of positions, just like the tracked object in Exercise 4 of [The Live Demo](../../worksheets/02-live-demo.md).

## Setup

These workflows need a few packages the workshop environment does not have, plus the TensorFlow runtime, so they have their own environment in this folder.

1. In the unzipped workshop folder, open `docs/examples/movenet/.bonsai`.
2. Double-click `Setup.cmd`. It downloads Bonsai, the packages and the TensorFlow runtime, about 400 MB in all, and closes when finished.
3. Double-click `Bonsai.exe` in that folder. From here you can open the workflows found in `docs/examples/movenet` directly, or copy and paste them from here.

## PoseSkeleton

:::workflow
![PoseSkeleton](PoseSkeleton.bonsai)
:::

Start here to see what the network sees. Every body part it finds is drawn as a dot on the camera image and the dots are joined up into a stick figure.

* Run and open the visualizer on `DrawCanvas`. Step back until your whole body is in frame. Turn sideways, raise an arm, crouch.
* `PredictSinglePoseThunder` has a `MinimumConfidence` slider. Parts the network is unsure about come out as "not a number", and `Path` breaks the line wherever it meets one, so hiding a hand behind your back makes that dot and its line disappear. Lower the slider and the guesses come back.
* The drawing is built from Bonsai drawing operators. `CreateCanvas` and `AddImage` put the camera frame on a canvas, then six groups each add one body region and `DrawCanvas` turns the canvas back into an image. Double-click `LeftArm` to see the recipe: three `GetBodyPart` nodes, a `Zip`, an `ExpressionTransform` that lists the three positions in order, and a `Path` that draws the line through them. The dots are a second `Path`.

## PoseSkeletonScript

:::workflow
![PoseSkeletonScript](PoseSkeletonScript.bonsai)
:::

The same picture, drawn by one operator we wrote ourselves. `DrawSkeleton` is not part of Bonsai: it is a short C# file in the `Extensions` folder next to the workflow, and Bonsai compiles it when the workflow opens. When the stock operators are not enough or require more wiring than practical, this is the escape hatch.

* Run the workflow and open the visualizer on `DrawSkeleton`.
* Right click the `DrawSkeleton` operator and hit "Go to Definition" or F12. This will open `Extensions/DrawSkeleton.cs` in a text editor. The list of links at the top is the whole stick figure; the three colours sit just below it. Make changes, save and close, then back in Bonsai click "Reload Extensions" to compile and see your changes in action.
* **Question:** which version would you rather hand to a friend, and why?

## PoseToTouchDesigner

:::workflow
![PoseToTouchDesigner](PoseToTouchDesigner.bonsai)
:::

The body as a controller. This is `PoseSkeleton` with one more group, `TouchDesigner`, which turns five body measurements into numbers between 0 and 1 and sends each to one knob of the Ember Pulse patch from Exercise 15 of [The Live Demo](../../worksheets/02-live-demo.md), the one in `touchdesigner/ember-pulse.toe`. You still see the stick figure, so you can watch what the network sees while the picture responds.

| Knob | Measurement |
| --- | --- |
| `/pulse` | how high your higher wrist is |
| `/trail` | how high your lower wrist is |
| `/hue` | how far apart your wrists are |
| `/zoom` | how far apart your feet are |
| `/spin` | where your nose is from left to right |

* Open `touchdesigner/ember-pulse.toe` in TouchDesigner, then run this workflow and open the visualizer on `DrawCanvas`. Raise one hand and the heartbeat quickens. Raise both and the trails lengthen. Spread your arms for colour, your feet for zoom, and lean to spin.
* Double-click `TouchDesigner` to see how. Each measurement is one `ExpressionTransform`: `Higher wrist` is `Math.Min` of the two wrist rows, `Wrist spread` is Pythagoras on the two wrists divided by Pythagoras on the two shoulders, and so on. Dividing by the shoulder width makes a distance about your body rather than about the camera, so stepping closer does not count as spreading your arms.
* After each expression an `ExpressionCondition` drops frames where a part was not found, which the predictor marks as "not a number", then `Rescale` squeezes the range you care about into 0 to 1 and `SendMessage` names the knob.
* **Question:** swap two of the `Address` values. Then invent a sixth measurement, the angle of your head, say, and give it a knob that is already taken. Which one did you steal, and why?

## MoveVj

:::workflow
![MoveVj](MoveVj.bonsai)
:::

A VJ picture driven by your body. The camera image is stripped to moving outlines and pixelated, then drawn on the GPU in a window of its own.

* Run the workflow. How does the position of your nose change the image?
* `GetBodyPart` is set to `nose`, but the drop-down in the properties panel offers all seventeen parts, so you can use whichever body part (or parts!) you want!
* The tuning values are exposed on the top-level nodes. Select one and look at the properties panel: the detection confidence, the pixel size and the colour are all there to play with while it runs.

## MoveNetGame

:::workflow
![MoveNetGame](MoveNetGame.bonsai)
:::

A target circle appears somewhere over the camera image. Get your nose onto it before the ten seconds run out!

> [!TIP]
> If tracking lags, swap `PredictSinglePoseThunder` for `PredictSinglePoseLightning`, a smaller model that trades a little accuracy for speed on a laptop CPU.

