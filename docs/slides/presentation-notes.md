# Bonsai: Wiring the World into Sound and Image

Welcome.

I'm Andrew and I'm one of the facilitators of this workshop along with Tom and Nic. I'm going to give you a quick introduction to this piece of software called Bonsai that we'll be working with today.

A bit about myself, I come from a background in neuroscience research - same as Tom and Nic. I did my PhD here in London and then did my postdoc research in the US, and then came back to London to join this company NeuroGEARS where I work now along with Tom and Nic. The founder of NeuroGEARS - Goncalo Lopes - created Bonsai and is still the primary developer, and one of the goals of the company is to maintain and develop this bit of software.

# What is Bonsai?

So what is Bonsai exactly? It's a programming language, ecosystem and editor / development environment.

Bonsai is an open-source, reactive programming language that's designed to work well with asynchronous streams of data; it has an ecosystem of packages for things like hardware integration, graphics pipelines and much more; and it has a visual editor for designing workflows / programs in the language.  

# What is Bonsai? cont.

The language itself was born and initially developed in the world of neuroscience research where it aimed to solve some of the problems in that field that I would argue are also very relevant outside of research. 

1. Neuroscience labs often rely on closed-source, proprietary and/or paid software to do their work. 
2. Neuroscience research often involves coordinating lots of different pieces of hardware in real-time: cameras, electrophysiology, screens etc. - and doing interesting stuff with hardware in real-time is just difficult, and it gets exponentially more difficult the more hardware you have.

# Problem 1 - Own your own tools

The response to problem 1, and the general philosophy of Bonsai and NeuroGEARS is that researchers, artists, makers and everyone in between should have freedom and ownership of their tools. They should be able to modify, inspect, repair and understand those tools without restriction. This is why Bonsai and almost everything we do with Bonsai at the company is open-source and accessible.

# Problem 2

Doing stuff with hardware is exponentially difficult because if you have a project that involves, say, cameras, audio, projection, MIDI, machine learning etc. all together you have to deal with a set of devices that all talk a different language in terms of their transmission protocols, data formats, sampling rates and so on. Bonsai aims to be a language that can unify all these devices - so that when you're creating an interactive, real-time system you worry about the actual interactions between hardware rather than wrangling with data formats and synchronisation. Bonsai can handle all these devices and allow you to interact with them with a unified model composed of different data streams, interactions and transformations.

A feature of Bonsai that falls out of this, and that I hope you'll see during the demo, is that its a brilliant tool for rapid prototyping and iteration. It's just very quick to get different data sources like cameras and microphones without much overhead or boilerplate code and get them to do interesting things together.

# The Bonsai model
At the high level Bonsai is made up of 'operators' which are nodes of processing that do one of a few different things:
1. Sources - these produce streams of data: images from a camera, audio from a microphone, user-input
2. Transforms - one-to-one transformations of a stream of data, e.g. modifying the color of an image, filtering an audio buffer
3. Combinators - combinatorial and conditional operations, usually requiring two or more streams. For example, sample a data stream every time an event (e.g. key press) happens; combine 2 streams of data for synchronisation.
4. Sinks - side effect operations that don't modify the stream, for example writing a stream's data to disk.

# Under the hood
As a technical aside, Bonsai is a compiled language based on the .NET reactive extensions library. Bonsai uses rx to make all of these data streams parallel and asynchronous, but abstracts away setting up all the asynchronous stuff. So as a user, you don't need to worry about writing stuff to manage all your multithreading, you just drop in camera, microphone, keyboard input, whatever and it all runs in parallel out-of-the-box. 

# Thinking in Bonsai
Because of this paralellism, Bonsai lends itself to a visual programming approach and programs are constructed as branching observable sequences. If you've ever done any programming in functional languages Bonsai might feel similar, if you've never done any programming at all Bonsai is quite intiuitive as you're connecting these nodes together to make 'pipes' of data flow, but generally one thinks of a Bonsai program in terms of asynchronous data flow, rather than sequential instructions.

# Live demo intro
Timed talk leaves 8:30 for intro demo.

Start from a fresh bonsai environment.

Tom is going to do a proper demo of Bonsai shortly but I want to just start with a quick walkthrough of the Bonsai editor to show concretely some of the stuff I've been introducing here. You should have been sent a link already detailing how to get Bonsai on your machine, but just give either me or Nic a shout if you're missing this and we can help you out with getting started.

>> Open the Bonsai editor
Once you open the editor, click New File to open a new workflow editor.

The most important panels here are the toolbox on the left; the workflow editor in the centre; and the property editor on the right.

Bonsai workflows are built by adding operators to the workflow and then chaining them together. For example, I can search in the toolbox here for a Timer, and hit enter or double-click to add it to the workflow. 

Once it's in the workflow, I can click on it to inspect its properties in the property editor. Timer has two editable properties, DueTime and Period which specify when the Timer starts and its tick period after it starts. I'll edit the period to be 1 second...

... And then start the workflow by clicking Start at the top.

The workflow is now running and live. For each operator, I can inspect what its doing by double-clicking...

... which will open a visualizer that lets me inspect the data flow of the operator in real-time.

I'll now stop the workflow, and we'll add some video so we have a slightly more interesting data source. We don't have camera streaming by default so we'll need to add the capability via a package. 

I'll go to Tools >> Manage Packages... Save the workflow before proceeding ... Jump into Browse and search for vision ... Install both the Vision and Vision Design packages. Hit Close to return to the workflow.

I should now be able to search for CameraCapture, and add it to the workflow in the same way.

Running the workflow now I can inspect both of these operators simultaneously, and I can see my Timer again and some camera streaming data in another visualizer.

Now we'll add some transformation to the data stream. First I'm going to grayscale the image. I'll add the Grayscale operator, and then have it apply to the camera data by dragging from the CameraCapture to the grayscale to create a transformation. Running the workflow again I can confirm with the visualizer for grayscale that the transformation is happening. 

We can keep chaining these transformations together, for example I can now add a BackgroundSubtraction operator in the same way. Running the workflow again I can inspect the transformation at each stage. I can also adjust the parameters of the background subtraction in real time using the property editor

>> reduce adaptation rate to start smearing image.

I can also make these streams interact with each other. For example I can add a sample operator after the background subtraction, and then use the Timer as the second input. Running the workflow again I get the same thing, but if I open the visualizer for Sample you can see that these two asynchronous streams are now interacting. Every time the timer produces an event, it sample from the image stream. 

This sampling is generic, I can swap out the Timer event for something else, for example some user input. I'll go back to the package manager and install the Windows Input package. Now I can add a KeyDown operator and set the Filter to something like 'A'.

I'll remove the existing Timer connection with right-click >> Remove connection >> click on Sample to remove the connection to Sample. Now drag from KeyDown to Sample. Now running again I sample the image feed in response to a key press.

Note the colors of the operators at this stage: we have green for data sources, operators that produce data; blue for transformations the transform the data sample by sample; and yellow for combinations such as Sample, in this case we make the image stream dependent on the input from a second stream.

The last operator type is a Sink. This is an operator that doesn't transform or combine, but produces some side effect on the data. The usual example of this is saving data to disk. I'll add a VideoWriter (also from the vision package) and give it a placeholder filename (test.avi). Run the workflow again and produce a load of samples, when I stop the workflow I will have saved some video data to disk, consisting of the samples I just triggered.

That's the very basic structure of designing and running a Bonsai workflow.

We've made a very simple program here but you can imagine that as workflows grow larger they become less coherent visually, so we can start grouping operations together.

I'll drag select the image transformation part, right-click and create a GroupWorkflow (or Ctrl+G). I can now give this a name like ImageTransformation, and add a description of its function if I want. 

We can also save these groups if we feel like we might want to repeat their functionality later. This allows us to produce Bonsai's equivalent of reusable functions. If I right-click, Save as Workflow, Save as ImageTransformation.bonsai. I can now add this grouped operation again by searching in the toolbox. 

>> Example here is to add ImageTransformation back in as a second branch from CameraCapture. Redo the sample logic with the hanging Timer to demonstrate the streams are parallel.

We can also edit these 'functions' to have arguments.

>> Ungroup ImageTransformation and externalise some properties. Resave, demonstrate that the two branches can have separate configurable properties now.