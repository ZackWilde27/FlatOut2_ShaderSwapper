# FlatOut 2 Shader Swapper
This mod recompiles shaders on the fly when they change, giving you a live-preview in game

<br>

# Building
I used Visual Studio 2022 on Windows, it requires my fork of Sewer's FlatOut 2 SDK

<br>

# Using the mod
You'll need to put the ```dxStuff.dll``` file in the game's directory, where ```FlatOut2.exe``` is

In the same place, you'll need to create a ```data``` folder, and in that create a ```shader``` folder, it should be set up just like in the BFS (If you are running the game unpacked, those folders should already be there)

Any shaders in the ```shader``` folder should now be recompiled when they change

### Note
There is a strange issue where shaders using model 3 will show an error the first time, but after that it should work just fine.
