using NSprites;

[assembly: InstancedPropertyComponent(typeof(MainTexST), "_mainTexSTBuffer")]
[assembly: InstancedPropertyComponent(typeof(WorldPosition2D), "_positionBuffer")]
[assembly: InstancedPropertyComponent(typeof(Scale2D), "_heightWidthBuffer")]
[assembly: InstancedPropertyComponent(typeof(Pivot), "_pivotBuffer")]
[assembly: InstancedPropertyComponent(typeof(SortingValue), "_sortingValueBuffer")]
[assembly: InstancedPropertyComponent(typeof(Flip), "_flipBuffer")]

[assembly: DisableRenderingComponent(typeof(CullSpriteTag))]