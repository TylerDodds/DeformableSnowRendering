# Deformable Snow Rendering

This project implements and explains the process of creating real-time deformable snow using HDRP. It covers shading, deformation, and simulation of snow.

## Project Setup

This project was built using Unity 2020.3.6f1 with HDRP 10.4.0 (upgraded from Unity 2019.3.9f1 with HDRP 7.3.1). It relies on HDRP's CustomPass to perform updates for deformation and simulation. All content (scripts, shaders, materials, textures, etc.) can be found under the Assets/Snow folder.

A slightly modified version of the HDRP package is needed for this version of HDRP, since HDRP has not yet [exposed the override camera rendering API](https://github.com/Unity-Technologies/Graphics/pull/5016). See the [_Version Update_ section of the Project Overview](Assets/Snow/Notes/Project%20Overview.md#hdrp-package-modification-instructions) for information on the required modifications, which will be needed for the project to compile.
    
## Documentation

[Project Overview.md](Assets/Snow/Notes/Project%20Overview.md) is a ground-up description of the project, describing the entire process of creating snow with appropriate shading, deformation, and simulation.

## Sample Scene

SampleScene.unity in the Scenes folder has examples from all stages of the project, including the most fully-featured snow simulation.