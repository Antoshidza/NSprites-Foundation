# 2D Transform
Provides similar to `Unity.Transforms` 2D implementation of `LocalTransform2D` / `LocalToWorld2D` and parent / child relationship.
Contains components / systems / authoring.

The only difference between `Unity.Transforms` and this is that in `LocalTransform2D` we use `float2` fields for Position / Rotation / Scale instead of `float3`.
This is really the only difference. In the end shader obtain `float4x4` matrix.

> There are plans to implement 2x3 matrix which should be minimal matrix for 2D sprites

Also provides a way to remove default 3D transform components from baking.

> **Existing of this part doesn't mean that NSprites only renders 2D, but for 2D needs we don't want to have unnecessary data.**

> **Note**: if you want to use full 3D sprites then you can easily use `Unity.Tramsforms` by registering `[assembly: InstancedPropertyComponent(typeof(LocalToWorld), "_positionBuffer")]`.
> In this case you would also replace all using of `LocalTransform2D` / `LocalToWorld2D` with `LocalTransform` and `LocalToWorld`
