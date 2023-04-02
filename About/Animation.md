# Animation
Provides components / systems / authorings / data structures to simulate sprite animation.
It just shifts UVs over time which looks like different sprite rendered.
While it just UV shifting animation sprite sequence should be a solid texture sheet.

## [`SpriteAnimationAuthoring`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Authoring/SpriteAnimationAuthoring.cs)
Inherited from [`SpriteRenderAuthoring`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Authoring/SpriteRendererAuthoring.cs) it also bakes all needed animation data provided as [`SpriteAnimationSet`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimationSet.cs).
If you want to implement your own authoring you can still use static methods provided by this class to perform same baking.

## Prepare assets
To work with animation part you will also need to create [`SpriteAnimationSet`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimationSet.cs) and bunch of [`SpriteAnimation`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimation.cs).
You can create them like most of `ScriptableObjects` by calling context menu in project and selecting `Create/NSprites/Animation Set` or `Create/Nsprites/Animation (sprite sequence)`.

### `SpriteAnimation`
A `ScriptableObject` containing sprite sequence (as solid sprite sheet) where each sprite has duration of how long it stays before switch to next frame. This class also requires additional data like frame count resolution.
### `SpriteAnimationSet`
A `ScriptableObject` containing set of `SpriteAnimation`s with string names (which used as IDs)
### `SpriteAnimationBlobData`
A `struct` blob to contain runtime immutable animation data. Contains `int` ID which corresponds to animation name with help of `Animator.StringToHash`, atlas UVs of sprite sheet, it's frame resolution, `BlobArray<float>` of frame durations and sum of all frames duration.

## Components
|Type|Description|
|----|-----------|
|`AnimationSetLink`|Provides link to `BlobArray<SpriteAnimationBlobData>`|
|`AnimationIndex`|Currently playing animation from `AnimationSetLink`|
|`FrameIndex`|Currently displayed frame from sprite sheet texture|
|`AnimationTimer`|Timer which has value of global time + frame duration. When timer excided (timer value equals current global time) frame index increases|

## How to change animations during runtime
Basically to change animation we should set `AnimationIndex` to another index and reset `AnimationTimer` and `FrameIndex` and also calculate proper `UVAtlas` of 1st frame of animation you choosed. Though the short way is to just change `AnimationIndex`, set `FrameIndex` to last frame index in choosed animation and set `AnimationTimer` to current `Time.ElapsedTime` which will trigger `SpriteUVAnimationSystem` to calculate next frame which should be the 1st frame of choosed animation.
