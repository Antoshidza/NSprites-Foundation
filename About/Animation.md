# Animation
Provides components / systems / authorings / data structures to simulate sprite animation.
It just shifts UVs over time which looks like different sprite rendered.
While it just UV shifting animation sprite sequence should be a solid texture sheet.

## [SpriteAnimationAuthoring](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Authoring/SpriteAnimationAuthoring.cs)
Inherited from [`SpriteRenderAuthoring`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Authoring/SpriteRendererAuthoring.cs) it also bakes all needed animation data provided as [`SpriteAnimationSet`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimationSet.cs).
If you want to implement your own authoring you can still use static methods provided by this class to perform same baking.

## Prepare assets
To work with animation part you will also need to create [`SpriteAnimationSet`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimationSet.cs) and bunch of [`SpriteAnimation`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimation.cs).
You can create them like most of `ScriptableObjects` by calling context menu in project and selecting `Create/NSprites/Animation Set` or `Create/Nsprites/Animation (sprite sequence)`.