# Getting Started

Do this **before** the workshop if you can. Downloads over venue wifi are slow, and the
sooner Bonsai is running on your laptop the more of the session you get to spend making
things.

## What you need

- A laptop running Windows 10 or later. Bonsai runs on Linux too, but the editor and
  visualizers are most reliable on Windows.
- A webcam and a microphone. The built-in ones are fine.
- About 500 MB of free disk space.

> [!TIP]
> No Windows machine? You are still very welcome. Follow along on the big screen and pair
> up with a neighbour for the hands-on part.

## Installing the workshop environment

The workshop has its own Bonsai environment with every package we use already pinned. It
lives in a folder, not in your system. It's completely self-contained and you
can delete it afterwards.

1. Download the repository as a zip from
   [github.com/neurogears/peckham-digital-2026](https://github.com/neurogears/peckham-digital-2026)
   (green **Code** button, then **Download ZIP**) and unzip it somewhere easy to find.
2. Open the `.bonsai` folder inside it. If you cannot see it, turn on **View > Hidden items**
   in File Explorer.
3. Double-click `Setup.cmd`. A console window appears, downloads Bonsai and its packages, and
   closes when finished. This takes a few minutes.
4. Double-click `Bonsai.exe` in the same folder to open the editor.
5. Choose **File > Open** and open `playground.bonsai` from the top of the workshop folder.
   It is an empty workflow. Opening it tells Bonsai to work from the workshop folder, which
   is where the shader files the later exercises need are kept. Build and paste in here.

> [!WARNING]
> Windows may show a "protected your PC" dialog when you run `Setup.cmd`. Click **More
> info**, then **Run anyway**. The script only downloads files into this one folder.

> [!NOTE]
> Already have Bonsai installed from [bonsai-rx.org](https://bonsai-rx.org)? You can use it,
> but copied workflows will keep asking you to install missing packages, and versions may
> not match ours. Having a Bonsai environment like the workshop folder avoids all of that, keeps your projects self-contained and mobile. We recommend using environments in this even if you have Bonsai on your system already.

## Finding your way around

Read the [editor guide](https://bonsai-rx.org/docs/articles/editor.html) The short version:

- The **toolbox** on the left lists every operator. Type to search it.
- The **canvas** in the middle is where you build. Operators are nodes, data flows along the connections from left to right.
- The **properties** panel on the right configures the selected node.
- The green **Start** button runs the workflow. Double-click any node while it is running
  to open a **visualizer** and watch the actual data flow through it.

### **Exercise 1:** A stream of numbers

* Add a `Timer` source. Set its `Period` property to `00:00:00.5`.
* Press **Start**, then double-click the `Timer` node.
* **Question:** what is coming out, and how often?

### **Exercise 2:** Your face, in a box

* Add a `CameraCapture` source. Leave `Index` at 0 for the built-in webcam.
* Press **Start** and double-click the node. Wave.
* Add a `Grayscale` transform after it. Double-click that one instead.

> [!TIP]
> If the visualizer is black, another program (Teams, Zoom, a browser tab) probably has
> the camera. Close it and restart your workflow.

### **Exercise 3:** Save and reopen

* **File > Save** the workflow as `hello.bonsai` in the unzipped repository folder.
* Close Bonsai, open it again, and reopen the file from **File > Recent**.
* Notice it remembers which visualizers you had open. Keep it next to the workflow.

Next: [The Live Demo](02-live-demo.md).
