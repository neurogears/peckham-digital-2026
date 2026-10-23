# Pan-tilt: a camera that follows

A webcam sits on two small servos, one that turns it left and right (pan) and one that nods it up and down (tilt). Bonsai finds something in the picture, works out how far it is from the centre, and nudges the servos until it is back in the middle. The object moves, the camera follows. The tracking half is the same recipe as Exercise 4 of [The Live Demo](../../worksheets/02-live-demo.md); the new half is the servos, which Bonsai talks to through an Arduino.

There are two workflows. `PanTilt` follows an object found by thresholding the image. `PanTiltML` follows an object in the image that a neural network can identify. What differs between the two is the tracking pipeline, the part of the workflow that transforms a camera image into a point to track; everything that drives the servos from that point is shared.

## Hardware

* A pan-tilt bracket with two hobby servos. The demo uses the DFRobot FIT0004 kit with DSS-P05 servos, with a small USB camera board mounted on the bracket.
* An Arduino running the `StandardFirmata` sketch from the Arduino IDE examples. Bonsai sends servo commands over USB using the Firmata protocol. The Arduino has a DFRobot Gravity IO Expansion Shield. 
* The tilt servo is connected to pin 4 and the pan servo is connected to pin 5.
* A USB camera on the bracket, or the laptop camera if you only want to watch the angles change.

## Setup

These workflows need `Bonsai.Arduino` and, for `PanTiltML`, the ONNX Runtime, so they have their own environment in this folder.

1. In the unzipped workshop folder, open `docs/examples/pan-tilt/.bonsai`.
2. Double-click `Setup.cmd`. It downloads Bonsai and the packages, and closes when finished.
3. Double-click `Bonsai.exe` in that folder, then open either workflow from `docs/examples/pan-tilt`.
4. Plug in the Arduino and find its port in Device Manager under Ports (COM & LPT). Double-click `ArduinoResources`, select `CreateArduino` and set `PortName` to that port. It is saved as `COM5`.

`PanTiltML` also needs a detection model, which is not stored in the repository. First, you should install [uv](https://docs.astral.sh/uv/), and then run the `export_onnx.py` to download the YOLO model weights and write them to `models/yolo26n.onnx` beside the workflow:

```
uv run export_onnx.py
```

You can use the `--model` argument to export a different model, for example `uv run export_onnx.py --model yolo26s`. [uv](https://docs.astral.sh/uv/) reads the dependencies declared at the top of the script, so there is nothing to install first.

## PanTilt

:::workflow
![PanTilt](PanTilt.bonsai)
:::

Four groups, left to right: find the object, connect the servos, turn a position into a nudge, and a window that shows what the camera sees.

* `CameraTracking` is Exercise 4 with two additions. The image is flipped vertically to match the way the camera sits on the bracket, and a `MorphologicalOperator` closes holes in the thresholded blob so a shiny object still counts as one region.
* `ArduinoResources` opens the board and holds the two angles. Each angle starts life as a slider from 0 to 1 multiplied by 180, stored in a `BehaviorSubject` so there is always a current value. A `ServoOutput` on each subject sends the angle to its pin. `SubscribeWhen` waits for the `Arduino` subject before subscribing, so nothing is sent until the board has answered.
* `PanTiltUpdate` is the controller. `Rescale` turns the X of `Position` into a number from -1 at the left edge to 1 at the right, with the image width mapped onto its `Max` so a different camera resolution needs no change. `Scan` adds that number to the current `PanAngle` every frame, and `MulticastSubject` writes the new angle back into the subject the servo is listening to. The further the object is from the centre, the bigger the step, so the camera slows down as it arrives. The Y branch does the same for `TiltAngle` with half the gain.
* Double-click `Scan` to see the limits. `Max` and `Min` keep the pan between 20 and 160 degrees and the tilt between 0 and 180. A step that would leave the range is thrown away and the old angle kept, which protects the bracket from trying to turn through itself.
* Run. Open `ArduinoResources` and drag the two sliders to point the camera somewhere sensible, then hold up the object. The window shows the camera image with the tracked point drawn on top. Walk slowly across the room and the camera turns to keep you in frame.
* `HsvThreshold` is set for a bright, strongly coloured object. Open its visualizer and drag `Lower` and `Upper` until only your object is white, as in Exercise 4.
* **Question:** the step is proportional to the error and nothing else. Hold the object still just off centre and watch the angle. Does it settle, hunt back and forth, or creep? Halve `RangeMin` and `RangeMax` on the pan `Rescale` and try again.

## PanTiltML

:::workflow
![PanTiltML](PanTiltML.bonsai)
:::

The same rig, but the thing being followed is now a selectable category of objects. [YOLO](https://docs.ultralytics.com) is an object detection network that draws a labelled box around each of eighty different kinds of more commonly found objects, such as `Person`, `Bottle`, `Cell phone`, etc. The model runs in the ONNX Runtime through `YoloDetect`, an operator defined in the `Extensions` folder next to the workflow and compiled by Bonsai when the editor opens. The centre of the box takes the place of the centroid that represented the thresholded object in the previous example. Everything downstream of `Position` is unchanged.

Three groups this time rather than four. Each detection carries the image it was found in, so the separate display group is no longer needed.

* `ImageProcessing` replaces `CameraTracking`. The camera image is flipped vertically and published as `Image`, and `YoloDetect` runs the model on every frame and emits each object it finds. `ModelPath` is `models/yolo26n.onnx`, the file written by `export_onnx.py`. `ConfidenceThreshold` drops candidates scoring below 0.5 and `IouThreshold` suppresses a box that overlaps a more confident one by more than 0.5, which is what stops one object collecting a stack of boxes. `ExecutionProvider` is `Cpu` and manages a few frames a second; it can be set to `Cuda` or `TensorRT` with the GPU build of the runtime, where `DeviceId` picks the card.
* `Concat` unpacks the collection so each box arrives as its own event. `Condition` keeps only the boxes whose `ObjectClass` matches `Class`, exposed on the group as a drop-down of the eighty COCO categories. `BoundingBox` and then `ExpressionTransform` take the middle of the box, and that is `Position`.
* `ArduinoResources` and `PanTiltUpdate` are the same as in `PanTilt`, with the two starting angles exposed as `PanAngle` and `TiltAngle` on the group so you can set them from the properties panel without opening it. The tilt branch uses the full gain here.
* Run, then double-click `ObjectDetections` to open its visualizer. Every box the network finds is drawn on the camera image with its class and confidence, and the camera turns to follow whichever one matches `Class`. Right-click the window to toggle the boxes and the labels. Step out of frame and it stays where it was until someone else walks in.
* Set `Class` to `Bottle`, `Book`, `Laptop` or `CellPhone` and hold up the real thing. Which does it find most reliably?
* Raise `ConfidenceThreshold` until the detection flickers, then lower it until things that are not there start appearing. Where you leave it decides whether the camera loses the object or chases a ghost.
* **Question:** two people in frame produce two boxes, and `Concat` hands both to the controller every frame. What does the camera do? Which box wins, and what would you add before `Position` to pick the one you want, the biggest, say, or the one nearest the centre?

> [!TIP]
> If the camera jitters, the detections are jittering. Lower the gain by narrowing `RangeMin` and `RangeMax` on the two `Rescale` nodes, or put a `Buffer` and `Average` on `Position` as in [Fork It](../../worksheets/03-fork-it.md).
