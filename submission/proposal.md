# Peckham Digital 2026 — Submission Proposal
## Bonsai: Wiring the World into Sound and Image

---

## Description of the Workshop, Talk, Demo or Performance (200 words)

Bonsai (https://bonsai-rx.org/) is an open-source visual reactive programming language that lets you compose streams of data — from cameras, microphones, and sensors — into real-time interactive systems. Though it was born in the neuroscience lab, its dataflow model is ideally suited to creative computing: every input is a stream of events, and every transformation is a composable, real-time operator. Bonsai has a rapidly growing ecosystem of packages for interacting with hardware, creating computer vision workflows, and running live machine learning models. The language is also highly extensible and users can easily create their own packages and plugins.

This session introduces Bonsai through the lens of an interactive installation. Starting from a blank canvas, we will build a live demo that reads from a camera and microphone, processes those signals in real time, and routes the results to visual output and audio effects — showing how a handful of connected nodes can produce expressive, reactive behaviour.

After the demonstration, attendees will have the chance to fork the demo and experiment: swapping sensors, adjusting transformations, or remapping inputs to outputs in surprising ways. No prior experience with Bonsai is required. Some familiarity with programming concepts (variables, events, loops) will help attendees get the most from the hands-on portion, but curious beginners are very welcome.

Participants will leave with a working Bonsai workflow, an understanding of its core programming model, and a template they can extend for their own creative projects.

**Audience level:** Beginner (programming curiosity sufficient; no Bonsai experience required)

---

## Facilitator Statement (250 words)

The facilitators for this demo will be members of NeuroGEARS (https://neurogears.org/), an independent research group and software company committed to the open-source software ethos. The team develops the Bonsai programming language and their work sits at the intersection of software engineering, systems neuroscience, and creative technology - building tools that let scientists and, increasingly, artists work with realtime data streams in an intuitive, visual way. 

Bonsai began as a laboratory instrument, developed by Goncalo Lopes (now director of NeuroGEARS) during his PhD as a way for neuroscientists to integrate and synchronise diverse data streams from cameras, behavioural sensors, and electrophysiology equipment without writing low-level code. Over the past decade it has grown into a mature, community-driven platform used in research worldwide. The language's reactive model has a natural home beyond the lab - in live performance, audiovisual installations, and interactive are - and anywhere that routing and transforming sensor data in real time is needed.

This demo is part of a broader effort to introduce Bonsai to the creative community and to learn from artists and makers who may push the tool in directions the scientific community never imagined. The hope is that a conversation starts here: between a language built for precision measurement and a community driven by creative curiosity.

Individual facilitators:

- Andrew Erskine (https://www.andrewerskine.uk/) has over a decade of experience working in neuroscience labs writing software to control closed-loop, interactive experiments. He has been an engineer at NeuroGEARS for the past 4 years, during which he has helped develop the Bonsai ecosystem and build the community through school outreach, conferences and workshops. 
- Nicholas Guilbeault <!-- TODO: Nic add your section -->
 

---

## How will this help your professional development? (200 words)

Bonsai was built for science, but the ideas behind it — composable data streams, real-time interaction, hardware abstraction — are not scientific ideas. They are general-purpose tools for working with the real-time world, and NeuroGEARS has long believed that their natural audience extends well beyond the research lab.

We hope that presenting at Peckham Digital will be a concrete step in reaching that audience. The creative technology community represents a set of users, workflows, and aesthetic ambitions that are distinct from those of the neuroscience labs where Bonsai grew up. Engaging with that community directly will help us understand what a creative practitioner actually needs from a tool like Bonsai: what the barriers to entry are, where the documentation falls short, and what kinds of expressive power matter most outside a scientific context.

That feedback will directly shape how we develop and communicate the platform going forward — informing new packages, tutorials, and examples aimed at artists, musicians, and interaction designers rather than researchers. More broadly, this is an opportunity for NeuroGEARS to build relationships with the London creative technology scene and to establish Bonsai as a serious option for live, sensor-driven creative work.

---

## Media Attachment (Videos & Images)

[Exploring neural data in VR](https://www.youtube.com/watch?v=mJDV07ptQFk&t=39s)
[Interactive digital performance](https://www.youtube.com/watch?v=jKB0d9vsfgA)

---

## Technical Requirements

**Provided by venue:**
- Projector and large screen (the live coding demo requires node graphs to be legible from the back of the room)
- Sound system with stereo speakers and a line-in or audio input (the demo includes live audio processing)
- A table or desk with a power socket for the presenter's laptop

**Brought by presenter:**
- Laptop running Windows (Bonsai is a Windows application) with Bonsai pre-installed
- Webcam and/or microphone for the interactive installation demo

**For attendees (guided activity):**
- Attendees wishing to participate in the hands-on portion should bring a Windows laptop
- Bonsai should be installed in advance — it is free and open source
  - Download and installation guide: https://bonsai-rx.org/
  - Installation typically takes 10–15 minutes; attendees should do this before the session
- Attendees without a Windows machine are still very welcome; they can follow along and pair with others during the hands-on section

> **Note:** Bonsai currently runs on Windows only. This is worth flagging clearly in any pre-event communications so attendees can plan accordingly.
