# GPU Particles

A particle system using compute shaders in Unity. Built using 2017.2

### Usage
Add the GPUParticles component to a Game Object and press play.   
Properties with two values imply a random selection between them.

### To Do
* Rotation over lifetime + by speed
* Velocity over lifetime
* Size over lifetime
* Emission types:
    - Sphere
    - Disk
    - Line
    - Mesh
    - Skinned mesh renderer
* Rendering modes:
    - Sprite
    - Trails
    - Instanced
* Optimization:
    - Precompute random fields
    - Precompute noise if not animated (and at lower resolution?)
* Depth buffer collision
* Pick noise dimension
* Dynamic kernel size