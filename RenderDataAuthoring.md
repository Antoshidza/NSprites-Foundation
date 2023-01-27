# Render data authoring
Contains several authoring classes which can be used to rapidly create sprite entities.

## abstract [SpriteRenderAuthoringBase](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Authoring/SpriteRendererAuthoringBase.cs)
Abstract base class which uses [render data registration]() part to bake sprite data. It just creates [`SpriteRenderDataToRegister`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Authoring/SpriteRenderDataToRegister.cs) to setup rendering system. If you want use registration part then you can inherit your authoring classes from this class.
> Note: all other authoring classes inherited from this one, so if you want to implement your own render register approach then avoid using this part.

## [SpriteRenderAuthoring](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Authoring/SpriteRendererAuthoring.cs)
Bakes default render components. Optionally adds [2D transform](2DTransform.md) components / removes unity default 3D transforms / adds sorting components. Provides static functions bake logic to use in custom authorings. Use this, when

## Prepare assets
There is [`PropertiesSet`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Data/PropertiesSet.cs)
which contains properties components names with update strategy type (read more about [properties](https://github.com/Antoshidza/NSprites/wiki/Register-components-as-properties) and [update modes](https://github.com/Antoshidza/NSprites/wiki/Property-update-modes)).
This `ScriptableObject` used by authoring to bake registration data. You can create it by call context menu in project `Create/NSprites/Properties Set`.