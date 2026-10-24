# Getting Started

Do this **before** the workshop if you can. Downloads over venue wifi are slow, and the sooner Bonsai is running on your laptop the more of the session you get to spend making things.

## What you need

- A laptop running Windows 10 or later, or a Linux machine. Bonsai is developed for Windows; on Linux it runs under Wine, installed through the nix package manager.
- A webcam and a microphone. The built-in ones are fine.
- About 1 GB of free disk space.

> [!TIP]
> Mac user? You are still very welcome. Follow along on the big screen and pair up with a neighbour for the hands-on part.

## Installing the workshop environment

The workshop has its own Bonsai environment with every package we use already pinned. It lives in a folder, not in your system. It is completely self-contained and you can delete it afterwards.

<details>
<summary><strong>Windows</strong></summary>

1. Download the repository as a zip from [github.com/neurogears/peckham-digital-2026](https://github.com/neurogears/peckham-digital-2026) (green **Code** button, then **Download ZIP**) and unzip it somewhere easy to find.
2. Open the `.bonsai` folder inside it. If you cannot see it, turn on **View > Hidden items** in File Explorer.
3. Double-click `Setup.cmd`. A console window appears, downloads Bonsai, its packages and the TensorFlow runtime the pose demos use, and closes when finished. This takes a few minutes.
4. Double-click `Bonsai.exe` in the same folder to open the editor.
5. Choose **File > Open** and open `playground.bonsai` from the top of the workshop folder. It is an empty workflow. Opening it tells Bonsai to work from the workshop folder, which is where the shader files the later exercises need are kept. Build and paste in here.

> [!WARNING]
> Windows may show a "protected your PC" dialog when you run `Setup.cmd`. Click **More info**, then **Run anyway**. The script only downloads files into this one folder.

> [!NOTE]
> Already have Bonsai installed from [bonsai-rx.org](https://bonsai-rx.org)? You can use it, but copied workflows will keep asking you to install missing packages, and versions may not match ours. An environment like the workshop folder avoids all of that and keeps your projects self-contained and portable. We recommend one even if you already have Bonsai on your system.

</details>

<details>
<summary><strong>Linux</strong></summary>

On Linux, Bonsai can run using either Mono or Wine. The [bonsai-nix](https://github.com/ncguilbeault/bonsai-nix) package uses Wine and takes advantage of the nix packaging language to create an all-in-one Bonsai package that works across many Linux distributions. This guide shows you how to get a working `bonsai` command without configuring any of the dependencies yourself.

1. Install nix. The full instructions are at [nixos.org/download](https://nixos.org/download/). The command below is the standard multi-user install and covers most cases, where you have sudo permissions and an x86_64 CPU. Open a terminal and run:

    ```bash
    sh <(curl --proto '=https' --tlsv1.2 -L https://nixos.org/nix/install) --daemon
    ```

    The installer asks a few questions. Answer yes to running sudo and to adding the nixbld users.

2. After installing nix, close the terminal and add a new one so it is added to the path. Install Bonsai with:

    ```bash
    nix --experimental-features "nix-command flakes" profile add github:ncguilbeault/bonsai-nix
    ```

    The first run downloads and builds Wine, winetricks and Bonsai. This takes a while, so start it early.

3. Launch Bonsai:

    ```bash
    bonsai
    ```

    The first time `bonsai` runs, it runs a setup script that initialises the Wine prefix, installs the DLLs Bonsai needs with winetricks, and then runs the Bonsai installer. Accept the licenses and complete the installation. At the end you are offered the choice to launch or exit. From then on, `bonsai` from a terminal or the Bonsai entry in your applications menu opens the editor directly.

4. Download the repository as a zip from [github.com/neurogears/peckham-digital-2026](https://github.com/neurogears/peckham-digital-2026) (green **Code** button, then **Download ZIP**) and unzip it somewhere easy to find.
5. In Bonsai, choose **File > Open** and open `playground.bonsai` from the top of the workshop folder. It is an empty workflow. Opening it tells Bonsai to work from the workshop folder, which is where the shader files the later exercises need are kept. Build and paste in here.

> [!NOTE]
> This route gives you Bonsai itself, not the pinned workshop environment, so Bonsai will prompt you to install packages the first time you open a workflow that uses one you do not have.

> [!TIP]
> If something fails part way through the first `bonsai` run, delete the Wine prefix at `~/.local/share/wineprefixes/wine-bonsai` and run `bonsai` again to start the setup from scratch.

</details>

## Finding your way around

Read the [editor guide](https://bonsai-rx.org/docs/articles/editor.html). The short version:

- The **toolbox** on the left lists every operator. Type to search it.
- The **canvas** in the middle is where you build. Operators are nodes, data flows along the connections from left to right.
- The **properties** panel on the right configures the selected node.
- The green **Start** button runs the workflow. Double-click any node while it is running to open a **visualizer** and watch the actual data flow through it.

### **Exercise 1:** A stream of numbers

* Add a `Timer` source. Set its `Period` property to `00:00:00.5`.
* Press **Start**, then double-click the `Timer` node.
* **Question:** what is coming out, and how often?

### **Exercise 2:** Your face, in a box

* Add a `CameraCapture` source. Leave `Index` at 0 for the built-in webcam.
* Press **Start** and double-click the node. Wave.
* Add a `Grayscale` transform after it. Double-click that one instead.

> [!TIP]
> If the visualizer is black, another program (Teams, Zoom, a browser tab) probably has the camera. Close it and restart your workflow.

### **Exercise 3:** Save and reopen

* **File > Save** the workflow as `hello.bonsai` in the unzipped repository folder.
* Close Bonsai, open it again, and reopen the file from **File > Recent**.
* Notice it remembers which visualizers you had open. Keep it next to the workflow.

Next: [The Live Demo](02-live-demo.md).
