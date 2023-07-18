# UOClient
__Proof of concept of UOClient renderer in 3D__

This is a "simple" proof of concept of an Ultima Online client clone that renders map in 3D and statics in 2D by using textures from original UO EC Client.

The client currently can render only the Tokuno map with statics; it is based on Monogame and is a first attempt to use the ECS pattern to organize code execution.

Due to custom data format used by client, in order to try to use this client you have to convert original UO files to custom data format: inside the solution there is a project called FileConverter with hard-coded paths 
that read original UO files and convert them into custom data format without any user intervention: just change the UOEC and UOCC paths in order run it.

By default converted files are written inside UO CC client folder that will be read by the UOClient project by default: you can change the path in both projects (they are hard coded) if you want to move them in a different location.

Paths are hard coded into:
- FileConverter: Program.cs
- UOClient: Settings.cs

You can switch between classic and KR statics by a boolean flag into Settings.cs ("UseEnhancedTextures").

Currently the development is halted due to no dev time and no interest from the community, but is usable to view the map.

## Classic statics
![](https://github.com/stefanomerotta/UOClient/blob/master/Images/Classic.png)

## KR statics
![](https://github.com/stefanomerotta/UOClient/blob/master/Images/KR.png)

## Water effect
![](https://github.com/stefanomerotta/UOClient/blob/master/Images/water.gif)

## Lava effect
![](https://github.com/stefanomerotta/UOClient/blob/master/Images/lava.gif)
