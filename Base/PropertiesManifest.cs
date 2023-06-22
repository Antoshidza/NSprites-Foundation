using NSprites;

[assembly: InstancedPropertyComponent(typeof(UVTilingAndOffset), "_uvTilingAndOffsetBuffer")]
[assembly: InstancedPropertyComponent(typeof(UVAtlas), "_uvAtlasBuffer")]
[assembly: InstancedPropertyComponent(typeof(LocalToWorld2D), "_positionBuffer")]
[assembly: InstancedPropertyComponent(typeof(Scale2D), "_heightWidthBuffer")]
[assembly: InstancedPropertyComponent(typeof(Pivot), "_pivotBuffer")]
[assembly: InstancedPropertyComponent(typeof(SortingValue), "_sortingValueBuffer")]
[assembly: InstancedPropertyComponent(typeof(Flip), "_flipBuffer")]

[assembly: DisableRenderingComponent(typeof(CullSpriteTag))]