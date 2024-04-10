# NSprites Foundation

This package provides various solutions for dots sprite rendering based on the [NSprites](https://github.com/Antoshidza/NSprites) package. While NSprites tries to be a flexible solution for any needs this project is more like a set of samples and useful tools which may not be perfect for your personal needs.

## How to use

I would recommend downloading this as an embedded package and adapting it to your needs. You can use most parts separately except those which depend on each other (details below). You can also implement your own tools on top of those or from scratch looking here for an example.

Also, before working with this package please read the [NSprites documentation](https://github.com/Antoshidza/NSprites/wiki) to understand what is going on (or use without editing :D)

## Content

| **name**                                                                                  | **running** | description                                                            |
| :---------------------------------------------------------------------------------------- | :---------- | :--------------------------------------------------------------------- |
| [Components](https://github.com/Antoshidza/NSprites-Foundation/tree/main/Base/Components) | runtime     | Contains components to be used with NSprites                           |
| [Authoring](About/AuthoringWorkflow.md)                                                   | editor      | Bakes basic render data, uses all other parts                          |
| Render data registration                                                                  | runtime     | Register render data for NSprites render system                        |
| [Sorting](About/Sorting.md)                                                               | runtime     | Calculate SortingValue depending on 2D position to use in shader       |
| Culling                                                                                   | runtime     | Disables / Enables rendering based on 2D position relatively to camera |
| [Animation](About/Animation.md)                                                           | runtime     | Shifts UVs values to simulate sprite animation                         |
| Graphics                                                                                  | runtime     | Provides shaders / materials / other render assets                     |

The diagram below illustrates the dependencies between parts:
<img src="About/NSprites-Foundation-Map.drawio.svg" width="800"/>

## Good to Know

- You can use the included shaders only with URP, so before rendering anything you should [import, create and assign the URP asset in the project settings](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/InstallingAndConfiguringURP.html).
  If you want to use another render pipeline, then you need to implement your own [shader and create a material](https://github.com/Antoshidza/NSprites/wiki/Prepare-compatible-material) to use with it.
- Some components are registered as properties in this package, so by default you will have component properties (you can see them [this window](https://github.com/Antoshidza/NSprites/wiki/Debug-NSprites-data)).
  This means that some shader property names are already registered with some components and you can't use similar names for different formats in other shaders you want to use with NSprites.
  If you don't want to have default registered components you can exclude the manifest file from the asset (use \*.unitypackage installation for that).

## Installation

### Requirements

- [NSprites v4.0.0+](https://github.com/Antoshidza/NSprites)
- Unity v2022.2.3+
- Universal RP v14.0.6+
- Entities v1.0.0-pre.65+

### [Install via Package Manager](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-ui-giturl.html)

- Window -> Package Manager -> + button -> Add package from git url
- Paste `https://github.com/Antoshidza/NSprites-Foundation.git`

### Install via git submodule

- `cd` to your project's `/Packages` folder
- Run `git submodule https://github.com/Antoshidza/NSprites-Foundation.git`

### Install via .unitypackage (recommended)

- Open the release you want on the Releases page in this repository
- Expand the Assets foldout and download the \*.unitypackage file
- Import it to your project by opening the file while the unity editor is open, or use Assets -> Import Package
  > Note: You can select which files to import, and can use this to filter out unwanted assets. The Package will be installed as local to the /Packages folder as usual, but in this case the package will be already cached so you can freely modify it.
