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
Because of this paralellism, Bonsai lends itself to a visual programming approach and programs are constructed as branching observable sequences. If you've ever done any programming in functional languages Bonsai might feel similar, but generally one thinks of a Bonsai program in terms of asynchronous data flow, rather than sequential instructions.