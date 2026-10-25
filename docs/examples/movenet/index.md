# MoveNet: pose-driven visuals

[MoveNet](https://www.tensorflow.org/hub/tutorials/movenet) is a small neural network published by Google. It finds seventeen points on a human body in a single camera frame: nose, eyes, shoulders, hips and joints. Bonsai runs it through the `Bonsai.TensorFlow.MoveNet` package, so a body part becomes just another stream of positions, like the tracked object in Exercise 4 of [The Live Demo](../../worksheets/02-live-demo.md).

## Setup

Nothing extra. The workshop environment from [Getting Started](../../worksheets/01-getting-started.md) already has the MoveNet package, and its `Setup.cmd` put the TensorFlow runtime in the `lib` folder at the top of the workshop folder. Open any workflow below from the workshop `Bonsai.exe`, straight from `docs/examples/movenet`, or copy and paste it from this page.

## PoseSkeleton

:::workflow
![PoseSkeleton](PoseSkeleton.bonsai)
:::

Start here to see what the network sees. Every body part it finds is drawn as a dot on the camera image and the dots are joined up into a stick figure.

* Run and open the visualizer on `DrawCanvas`. Step back until your whole body is in frame.
* `PredictSinglePoseThunder` has a `MinimumConfidence` slider. Parts the network is unsure about come out as "not a number", so nothing is drawn.
* The drawing is built from Bonsai drawing operators. `CreateCanvas` and `AddImage` put the camera frame on a canvas, then six groups each add one body region and `DrawCanvas` turns the canvas back into an image. Double-click `LeftArm` to see the recipe: three `GetBodyPart` nodes, a `Zip`, an `ExpressionTransform` that lists the three positions in order, and a `Path` that draws the line through them.

> [!NOTE]
> `Gap` is a point whose coordinates are both "not a number", published once per frame. Each group zips it in and puts it in its list of points wherever the line should break, because `Path` stops drawing at a "not a number" point and starts again after it.

## PoseSkeletonScript

:::workflow
![PoseSkeletonScript](PoseSkeletonScript.bonsai)
:::

The same picture, drawn by one operator we wrote ourselves. `DrawSkeleton` is not part of Bonsai: it is a short C# file in the `Extensions` folder next to the workflow, and Bonsai compiles it when the workflow opens. When the stock operators are not enough or require more wiring than practical, add a `CSharpTransform` and create your own operator from a template.

* Run the workflow and open the visualizer on `DrawSkeleton`.
* Right click the `DrawSkeleton` operator and hit "Go to Definition" or F12. This will open `Extensions/DrawSkeleton.cs` in a text editor. The list of links at the top is the whole stick figure; the three colours sit just below it. Make changes, save and close, then back in Bonsai click "Reload Extensions" to compile and see your changes in action.
* **Question:** which version would you rather hand to a friend, and why?

> [!TIP]
> You can write in Python too. The `Bonsai.Scripting.Python` package runs Python code and whole Python programs inside a workflow, with whichever Python packages you have installed.

## PoseToTouchDesigner

:::workflow
![PoseToTouchDesigner](PoseToTouchDesigner.bonsai)
:::

The body as a controller. This is `PoseSkeleton`, with its drawing folded into one `Skeleton` group, plus a `TouchDesigner` group that turns five body measurements into numbers between 0 and 1 and sends each to one dial of the Ember Pulse patch from Exercise 10 of [The Live Demo](../../worksheets/02-live-demo.md), the one in `touchdesigner/ember-pulse.toe`. You still see the stick figure, so you can watch what the network sees while the picture responds.

| Dial | Measurement |
| --- | --- |
| `/pulse` | how high your higher wrist is |
| `/trail` | how high your lower wrist is |
| `/hue` | how far apart your wrists are |
| `/zoom` | how far apart your feet are |
| `/spin` | where your nose is from left to right |

* Open `touchdesigner/ember-pulse.toe` in TouchDesigner, then run this workflow and open the visualizer on `DrawCanvas`. Raise one hand and the heartbeat quickens. Raise both and the trails lengthen. Spread your arms for colour, your feet for zoom, and lean to spin.
* Double-click `TouchDesigner` to see how. Each measurement is one `ExpressionTransform`: `Higher wrist` is `Math.Min` of the two wrist rows, `Wrist spread` is Pythagoras on the two wrists divided by Pythagoras on the two shoulders, and so on. Dividing by the shoulder width makes a distance about your body rather than about the camera, so stepping closer does not count as spreading your arms.
* After each expression an `ExpressionCondition` drops frames where a part was not found, which the predictor marks as "not a number", then `Rescale` squeezes the range you care about into 0 to 1 and `SendMessage` names the dial.
* **Question:** swap two of the `Address` values. Then invent a sixth measurement, the angle of your head, say, and give it a dial that is already taken. Which one did you steal, and why?

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

## ClosedLoop

:::workflow
![ClosedLoop](ClosedLoop.bonsai)
:::

A window into a 3D room that moves as you do. Five shapes sit at different depths and the viewpoint follows your head, so leaning to one side lets you peek round the near cube and stepping closer brings the whole scene nearer. This is the closed-loop demo from the [BonVision examples](https://github.com/bonvision/examples/tree/master/Demos/ClosedLoop), which followed a torch held up to the camera. Here MoveNet follows your nose instead.

* Run the workflow. A grey window opens with the shapes in it. Stand in front of the camera, sway from side to side and watch the shapes slide past each other. Then step forward and back.
* `HeadTracking` runs the network and picks out the nose and both eyes. The `ExpressionCondition` after it drops any frame where one of the three was not found.
* `Rescale` turns pixels into scene units. The nose column becomes the `X` of both `Eye` and `Target` in `PerspectiveView`, flipped so that moving to your right moves the viewpoint right. The spacing between your eyes stands in for how close you are, because everyone's eyes are about the same distance apart however tall they are: 40 pixels apart counts as far away and puts the viewpoint at `Z` 3, 160 pixels counts as close and brings it in to 1. On a 640 by 480 webcam that is roughly 2.5 m to 0.6 m, and sitting at a laptop lands in the middle. Open the visualizer on `Eye spacing` to see your own number, then tune `Min` and `Max` on that third `Rescale` for your camera and room.
* The scene is five `DrawModel` nodes, each with its own mesh, position, scale and colour, all drawn on the `Draw` frame that `PerspectiveView` publishes. The icosphere spins because `RangeAnimation` counts from 0 to 359 every three seconds, `Repeat` starts it again, and `DegreeToRadian` feeds its `RotationY`. The meshes are plain OBJ files in the `Models` folder, loaded once by `MeshResources` at the top. `suzanne`, `sphere` and `cylinder` are loaded but not drawn, so they are yours to add.
* **Question:** the nose row is rescaled too, but nothing reads it. Map it onto `Y` of `Eye` as well. Which way round does it need to go so that crouching lowers the view?

