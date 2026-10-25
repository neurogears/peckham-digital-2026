# Getting Started

Do this **before** the workshop if you can.

## What you need

- A laptop running Windows 10 or later. Bonsai runs on Linux too, but the editor and visualizers are most reliable on Windows. We are working on this!
- A webcam and a microphone. The built-in ones are fine.
- About 2 GB of free disk space. Setup downloads several hundred megabytes, so please set it up at home if you can; the venue wifi will be slow with everyone downloading at once.

> [!TIP]
> No Windows machine? You are still very welcome. Follow along on the big screen and pair up with a neighbour for the hands-on part.

## Installing the workshop environment

The workshop has its own Bonsai environment with every package we use already pinned. It lives in a folder, not in your system. It is completely self-contained and you can delete it afterwards.

1. Download the repository as a zip from [github.com/neurogears/peckham-digital-2026](https://github.com/neurogears/peckham-digital-2026) (green **Code** button, then **Download ZIP**) and unzip it somewhere easy to find. This is your workshop folder.
2. Open the `.bonsai` folder inside it. If you cannot see it, turn on **View > Hidden items** in File Explorer.
3. Double-click `Setup.cmd`. A console window appears, downloads Bonsai, its packages and everything else you need to run and edit all the workflows in this repository. This can take a while on a slow connection.
4. Double-click `Bonsai.exe` in the same folder to open the editor.
5. Choose **File > Open** and open `playground.bonsai` from the top of the workshop folder. It is a blank canvas, running in the environment built by the script you just ran. Here you can build, edit and code as you like.

> [!WARNING]
> Windows may show a "protected your PC" dialog when you run `Setup.cmd`. Click **More info**, then **Run anyway**. We promise there's nothing malicious here, and downloads will only go into this folder, so you can completely delete them later if you wish. 

> [!NOTE]
> Already have Bonsai installed from [bonsai-rx.org](https://bonsai-rx.org)? You can use it, but copied workflows will keep asking you to install missing packages and versions may not match ours or conflict with other packages you might have installed. An environment like the workshop folder avoids all of that and keeps your projects self-contained and portable. We recommend doing this for all your future projects.

## Finding your way around

Read the [editor guide](https://bonsai-rx.org/docs/articles/editor.html). The short version:

- The **toolbox** on the left lists every operator. Type to search it.
- The **canvas** in the middle is where you build. Operators are selected from the toolbox and dropped into the canvas. These are nodes. Each node has a documentation page online, which you can get at by pressing F1 with any node selected. 
- Data flows through nodes along the connections from left to right (and not top to bottom!). Note that nodes that line up from left to right are not neccessarily happening at the same time.
- The **properties** panel on the right configures the selected node. Each operator and each property has a description explaining what it does.
- The green **Start** button runs the workflow. Double-click any node while it is running to open a **visualizer** and watch the actual data flow through it.

### **Exercise 1:** A stream of numbers

* Add a `Timer` source. Set its `Period` property to `00:00:00.5`.
* Press **Start**, then double-click the `Timer` node.
* **Question:** what is coming out, and how often?

### **Exercise 2:** Acquire the camera

:::workflow
![Camera capture](../workflows/01-getting-started-01.bonsai)
:::

* Add a `CameraCapture` source. In the property editor (left side) with `CameraCapture` node selected, leave `Index` at 0 for the built-in webcam.
* Press **Start** and double-click the node. Wave.

:::workflow
![Camera capture in grayscale](../workflows/01-getting-started-02.bonsai)
:::

* Add a `Grayscale` node after it. Double-click that one instead.
* Keep this workflow. The Live Demo starts from it.

> [!TIP]
> If the visualizer is black, another program (Teams, Zoom, a browser tab) probably has the camera. Close it and restart your workflow.

### **Exercise 3:** Save and reopen

* **File > Save** the workflow as `hello.bonsai` in the workshop folder (the top of the unzipped download, next to the `.bonsai` folder).
* Close Bonsai, open it again, and reopen the file from **File > Recent**.
* Notice it remembers which visualizers you had open and their settings.

Next: [The Live Demo](02-live-demo.md).
