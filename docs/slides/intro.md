# Bonsai: Wiring the World into Sound and Image

## What is Bonsai?
Bonsai is an open-source programming language, ecosystem and development environment. The language itself was actually born and initially developed in the world of neuroscience research where it aimed to solve some problems that are also relevant outside academic research:

1. Neuroscience labs often rely on closed-source proprietary software to do their work, the philosophy of Bonsai (and NeuroGEARS) is that researchers, artists etc. should be able to own their own tools, take them apart and understand them internally.
2. Doing interesting things with hardware in real-time is difficult, and gets exponentially more difficult the more hardware you have. The reason for that is that if you want to do something that involves cameras, audio, projection, MIDI, machine learning etc. you have to deal with a set of devices that all talk a different language in terms of their transmission protocols, data formats, sampling rates and so on. Bonsai aims to be a language that can unify all these devices, so that when you're creating an interactive, real-time system you worry about the actual interactions between hardware rather than wrangling with data formats and synchronisation.

A feature of Bonsai that falls out of this aim is that it's a brilliant tool for rapid prototyping and iteration. If you want to, for example, get a stream of images from a camera; track an object; record some audio and have a visualizer react to those things you can get this up and running within minutes.

## The Bonsai model
At the high level Bonsai is made up of 'operators' which are nodes of processing that do one of a few different things:
1. Sources - these produce streams of data: images from a camera, audio from a microphone, user-input
2. Transforms - one-to-one transformations of a stream of data, e.g. modifying the color of an image, filtering an audio buffer
3. Combinators - combinatorial and conditional operations, usually requiring two or more streams. For example, sample a data stream every time an event (e.g. key press) happens; combine 2 streams of data for synchronisation.
4. Sinks - side effect operations that don't modify the stream, for example writing a stream's data to disk.

As a technical aside, all of this is based on the reactive extensions (rx) library from Microsoft. Bonsai uses rx to make all of these data streams parallel and asynchronous, but abstracts away setting up all the asynchronous stuff. So as a user, you don't need to worry about writing stuff to manage all your multithreading, you just drop in camera, microphone, keyboard input, whatever and it all runs in parallel out-of-the-box. 

Because of this paralellism, Bonsai lends itself to a visual programming approach and programs are constructed as branching observable sequences. If you've ever done any programming in functional languages Bonsai might feel similar, but generally one thinks of a Bonsai program in terms of asynchronous data flow, rather than sequential instructions.

## The Bonsai editor

Editor demo

## Extending Bonsai