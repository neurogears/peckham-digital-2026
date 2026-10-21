# Pan-tilt: a camera that follows

A webcam sits on two small servos, one that turns it left and right (pan) and one that nods it up and down (tilt). Bonsai finds something in the picture, works out how far it is from the centre, and nudges the servos until it is back in the middle. The object moves, the camera follows. The tracking half is the same recipe as Exercise 4 of [The Live Demo](../../worksheets/02-live-demo.md); the new half is the servos, which Bonsai talks to through an Arduino.

There are two workflows. `PanTilt` follows an object found by thresholding the image. `PanTiltML` follows an object in the image that a neural network can identify. The only difference between the 2 workflows is the tracking pipeline - the part of the workflow that transforms a camera image into a point to track.

## Hardware

* A pan-tilt bracket with two hobby servos. The demo uses the DFRobot FIT0004 kit with DSS-P05 servos, with a small USB camera board mounted on the bracket.
* An Arduino running the `StandardFirmata` sketch from the Arduino IDE examples. Bonsai sends servo commands over USB using the Firmata protocol. The Arduino has a DFRobot Gravity IO Expansion Shield. 
* The tilt servo is connected to pin 4 and the pan servo is connected to pin 5.
* A USB camera on the bracket, or the laptop camera if you only want to watch the angles change.

## Setup

These workflows need `Bonsai.Arduino` and, for `PanTiltML`, the Torch runtime, so they have their own environment in this folder.

1. In the unzipped workshop folder, open `docs/examples/pan-tilt/.bonsai`.
2. Double-click `Setup.cmd`. It downloads Bonsai and the packages, about 500 MB in all, and closes when finished.
3. Double-click `Bonsai.exe` in that folder, then open either workflow from `docs/examples/pan-tilt`.
4. Plug in the Arduino and find its port in Device Manager under Ports (COM & LPT). Double-click `ArduinoResources`, select `CreateArduino` and set `PortName` to that port. It is saved as `COM5`.

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

The same rig, but the thing being followed is now a selectable category of objects. [YOLO](https://docs.ultralytics.com) is an object detection network that draws a labelled box around each of eighty different kinds of more commonly found objects, such as `Person`, `Bottle`, `Cell phone`, etc. Bonsai runs it through `Bonsai.ML.Torch`, and the centre of the box takes the place of the centroid that represented the thresholded object in the previous example. Everything downstream of `Position` is unchanged.

* `Model` sets up the device and loads `models/yolo26n.torchscript`, the smallest YOLO model exported for TorchSharp. `InitializeTorchDevice` is set to `CUDA`. On a laptop without an NVIDIA card change `DeviceType` to `CPU`, which still manages a few frames a second.
* `ImageProcessing` replaces `CameraTracking`. `Preprocess` resizes the frame and moves it to the device, `Forward` runs the network, and `DecodeObjectDetectionData` turns the raw output into boxes, dropping anything under half confidence and merging boxes that overlap. `Concat` then unpacks the list so each box is its own event.
* `Condition` keeps only boxes whose class matches `Class`, exposed on the group as a drop-down. `ExpressionTransform` takes the middle of the box, and that is `Position`.
* `ArduinoResources` and `PanTiltUpdate` are the same as in `PanTilt`, with the two starting angles exposed as `PanAngle` and `TiltAngle` on the group so you can set them from the properties panel without opening it. The tilt branch uses the full gain here.
* Run and open the window. Every box the network finds is drawn on the camera image, and the camera turns to follow whichever one matches `Class`. Step out of frame and it stays where it was until someone else walks in.
* Set `Class` to `Bottle`, `Book`, `Laptop` or `CellPhone` and hold up the real thing. Which does it find most reliably?
* **Question:** two people in frame produce two boxes, and `Concat` hands both to the controller every frame. What does the camera do? Which box wins, and what would you add before `Position` to pick the one you want, the biggest, say, or the one nearest the centre?

> [!TIP]
> If the camera jitters, the detections are jittering. Lower the gain by narrowing `RangeMin` and `RangeMax` on the two `Rescale` nodes, or put a `Buffer` and `Average` on `Position` as in [Fork It](../../worksheets/03-fork-it.md).
