# Animation
Provides components / systems / authorings / data structures to simulate sprite animation.
It just shifts UVs over time which looks like different sprite rendered.
While it just UV shifting animation sprite sequence should be a solid texture sheet.

## How it works
All animation data lives in provided `ScriptableObject`s (mentioned below).
In baking phase it bakes all immutable animation data into blob (see `SpriteAnimationAuthoring`).
Then in runtime it changes `UVAtlas` component value over time to perform flipbook animation.

## [`SpriteAnimationAuthoring`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Authoring/SpriteAnimationAuthoring.cs)
Inherited from [`SpriteRenderAuthoring`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Authoring/SpriteRendererAuthoring.cs) it also bakes all needed animation data provided as [`SpriteAnimationSet`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimationSet.cs).
If you want to implement your own authoring you can still use static methods provided by this class to perform same baking.

## Prepare assets
To work with animation part you need to create [`SpriteAnimationSet`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimationSet.cs) and bunch of [`SpriteAnimation`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Animation/Data/SpriteAnimation.cs).
You can create them like most of `ScriptableObjects` by calling context menu in project and selecting `Create/NSprites/Animation Set` / `Create/Nsprites/Animation (sprite sequence)`.

### `SpriteAnimation`
A `ScriptableObject` containing sprite sequence (as solid sprite sheet) where each sprite has duration of how long it stays before switch to next frame. This class also requires additional data like frame count resolution.
### `SpriteAnimationSet`
A `ScriptableObject` containing set of `SpriteAnimation`s with string names (which used as IDs)
### `SpriteAnimationBlobData`
A `struct` blob to contain runtime immutable animation data. Contains `int` ID which corresponds to animation name with help of `Animator.StringToHash`, atlas UVs of sprite sheet, it's frame resolution, `BlobArray<float>` of frame durations and sum of all frames duration.

## Change animation during runtime
### Using `AnimatorAspect`
You can use this [aspect](https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/aspects-intro.html) to change animation by it's ID in a simple way.
```csharp
// this example is from Age-of-Sprites project https://github.com/Antoshidza/Age-of-Sprites/blob/main/Assets/Sources/Rome/Systems/MovableAnimationControlSystem.cs
[BurstCompile]
private partial struct ChangeAnimationJob : IJobEntity
{
    public int SetToAnimationID;
    public double Time;

    private void Execute(ref AnimatorAspect animator)
    {
        animator.SetAnimation(SetToAnimationID, Time);
    }
}
```

### Manually
Basically to change animation we should set `AnimationIndex` to another index and reset `AnimationTimer` and `FrameIndex` and also calculate proper `UVAtlas` of 1st frame of animation you choosed. Though the short way is to just change `AnimationIndex`, set `FrameIndex` to last frame index in choosed animation and set `AnimationTimer` to current `Time.ElapsedTime` which will trigger `SpriteUVAnimationSystem` to calculate next frame which should be the 1st frame of choosed animation.

You can inspect example of doing this by looking at `AnimatorAspect` source code.

## Components
|Type|Description|
|----|-----------|
|`AnimationSetLink`|Provides link to `BlobArray<SpriteAnimationBlobData>`|
|`AnimationIndex`|Currently playing animation from `AnimationSetLink`|
|`FrameIndex`|Currently displayed frame from sprite sheet texture|
|`AnimationTimer`|Timer which has value of global time + frame duration. When timer excided (timer value equals current global time) frame index increases|

