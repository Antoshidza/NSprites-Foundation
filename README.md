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
| [Sorting](About/Sorting.md)                                                                     | runtime     | Calculate SortingValue depending on 2D position to use in shader       |
| Culling                                                                                   | runtime     | Disables / Enables rendering based on 2D position relatively to camera |
| [Animation](About/Animation.md)                                                                 | runtime     | Shifts UVs values to simulate sprite animation                         |
| [2D Transform](About/2DTransform.md)                                                            | both        | Provides systems / components to simulate 2D transforms                |
Diagram below illustrates dependencies between parts

// TODO: place diagram through GitHub 

## Installation
### Requirements
* [NSprites v2.0.0](https://github.com/Antoshidza/NSprites)
* Unity v2022.2+
* Entities v1.0.0-pre.15

### [Install via Package Manager](https://docs.unity3d.com/2021.3/Documentation/Manual/upm-ui-giturl.html)
* Window -> Package Manager -> + button -> Add package from git url
* Paste `https://github.com/Antoshidza/NSprites-Foundation.git`
### Install via git submodule
* `cd` to your project's `/Packages` folder
* git submodule https://github.com/Antoshidza/NSprites-Foundation.git