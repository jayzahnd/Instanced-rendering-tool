
# Unity instanced geometry converion tool.

![Screenshot  CUTOUT small](https://github.com/jayzahnd/Instanced-rendering-tool/assets/54983110/cf220fc7-82f3-4902-a41f-184060f8fa05)

This tool serves as a demonstration of how to use `Graphics.DrawMeshInstanced` for objects made up of potentially multiple sub-meshes and materials. I did not use `Graphics.RenderMeshInstanced` due to a bug on some older versions of Unity (such as 2021.3.19f1) that caused significant garbage generation.  
The `InstantiableProp` component is also provided to quickly convert scene objects into instanced geometry. 

## Setting up objects that you want instanced.

* Select the game objects you wish to convert and add a Convertible Prop component to them. You can select multiple objects at a time when doing this.
* Make sure the “Batching Static” tag MUST be disabled on those objects.
* On the shared material, make sure GPU instancing is enabled (This parameter is in the “Advanced Options” of the material inspector). This will allow for instancing variants to be generated.

## Setting up the controller.

* Add an **Environment Instancer Controller** component to a GameObject in the scene. Simple :)

## Using the menu commands.

* Now that you have your objects and your controller prepared, you can now complete their setup via the editor tools. The commands can be found under **Level Editor -> Mesh Instancing**.
* You will see 3 options:  
    1. ***Build prop mesh groups from convertible props in the scene***: Will detect the convertible props in the scene, irrespective of their position in the hierarchy, and populate the fields of your controller. This is mostly to visualize and test in editor, as well as allow the use of the two other commands, as stored instanced prefab references will be lost in scene load/unload. Mesh groups will be rebuilt at runtime anyway.
    2. ***Convert meshed objects to points (Disable Renderer)***: Detects all the Convertible Props in the scene, sets up their variables (shared Mesh reference + mesh name string + shared material), and disables their renderer. Make sure to always disable props once you’ve finished working on a level to avoid overlapping geometry.
    3. ***Convert points back to meshed objects (Enable Renderer)***: Re-activates all the Convertible Props’ Mesh renderer. Useful to make later adjustments. The tool is non-destructive in its current state.
 
## Special situation: baking lightmaps.

Baked light behaves a bit strangely with instanced geometry, but there is a workaround if you need one:

1)  Make a duplicate of your main scene Directional Light;
2)	Set its mode to Baked (Mixed will produce weaker shadows, so try that if baked is too strong); 
3)  Set its culling mask to Everything;
4)	Set shadows to Soft Shadows (baked mode force sets them to soft anyway)
5)	On your convertible props, try using these Mesh Renderer settings: Cast Shadows on, Static Shadow caster, Contribute GI, receive Global Illumination from Lightmaps
6)	In Window -> Rendering -> Lighting, try using Baked Global Illumination + Shadowmask lighting mode
7)	Clear any previous baked data, and regenerate lighting.
8)	Once you have the lightmap you like, disable or delete the directional light you’ve used for baking.
9)	Make sure that your main Directional Light’s culling mask includes the layer number you set in the Environment Instancer Controller (Rendering Layer Number Global). Or your instanced meshes will remain dark.
10)	If your baked textures are too bright, they need to be on a layer that is excluded from the main Directional Light’s culling mask.


