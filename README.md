# NSprites Foundation
This package provides various solutions for dots sprite rendering based on [NSprites](https://github.com/Antoshidza/NSprites) package. While NSprites trying to be flexible solution for any needs this project is more like set of samples and useful tools which can be not perfect for your personal needs.

## How to use
I would recommend download this as embedded package and adapt for your needs. You can use parts separately except those who depends on each other (details below). Though you can implement your own tools on top of those or from scratch looking here as to example.

Also before working with this package please read [NSprites documentation](https://github.com/Antoshidza/NSprites/wiki) to understand what is going on (or use without editing :D)

## Content
| **name**                                                                                  | **running** | description                                                            |
|:------------------------------------------------------------------------------------------|:------------|:-----------------------------------------------------------------------|
| [Components](https://github.com/Antoshidza/NSprites-Foundation/tree/main/Base/Components) | runtime     | Contains components to be used with NSprites                           |
| [Render data authoring](About/RenderDataAuthoring.md)                                     | editor      | Bakes basic render data, uses all other parts                          |
| Render data registration                                                                  | runtime     | Register render data for NSprites render system                        |
| [Sorting](About/Sorting.md)                                                               | runtime     | Calculate SortingValue depending on 2D position to use in shader       |
| Culling                                                                                   | runtime     | Disables / Enables rendering based on 2D position relatively to camera |
| [Animation](About/Animation.md)                                                           | runtime     | Shifts UVs values to simulate sprite animation                         |
| [2D Transform](About/2DTransform.md)                                                      | both        | Provides systems / components to simulate 2D transforms                |
| Graphics                                                                                  | runtime     | Provides shaders / materials / other render assets                     |

Diagram below illustrates dependencies between parts
<img src="About/NSprites-Foundation-Map.drawio.svg" width="800"/>

## Limitations
* You can use included shaders only with URP, so before anything rendered you should [import, create and assign URP asset in project settings](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/InstallingAndConfiguringURP.html).
If you want use another render pipeline then you need to implement your own [shader and create material](https://github.com/Antoshidza/NSprites/wiki/Prepare-compatible-material) with it.
* Some components registered as properties in this package, so by default you will have component properties (you can see them [this window](https://github.com/Antoshidza/NSprites/wiki/Debug-NSprites-data)).
This means that some shader properties names already registered with some component and you can't use similar names for different formats in another shaders you want to use with NSprites.
If you don't want to have default registered components you can exclude manifest file from asset (use *.unitypackage installation for that).

## Installation
### Requirements
* [NSprites v3.0.0+](https://github.com/Antoshidza/NSprites)
* Unity v2022.2.3+
* Universal RP v14.0.6+
* Entities v1.0.0-pre.65+

### [Install via Package Manager](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-ui-giturl.html)
* Window -> Package Manager -> + button -> Add package from git url
* Paste `https://github.com/Antoshidza/NSprites-Foundation.git`
### Install via git submodule
* `cd` to your project's `/Packages` folder
* git submodule https://github.com/Antoshidza/NSprites-Foundation.git
### Install via .unitypackage (recommended)
* Open release you need in this repository
* Expand Assets foldout and download *.unitypackage file
* Import it to your project by opening file during unity editor opened or Assets -> Import Package
> Note: You can select what files to import, use this to filter unwanted assets. Package will be installed
as local to /Packages folder as usual, but in this case package will be already cached so you can freely modify it.
