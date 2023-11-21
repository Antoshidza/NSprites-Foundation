using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites.Authoring
{
    public static partial class BakerExtensions
    {
        /// <summary>
        /// Bakes basic sprite data. Use this method only if you using registration from this package.
        /// If you want implement your own registration process please implement your own baking method.
        /// </summary>
        public static void BakeSpriteBase<TAuthoring>(this Baker<TAuthoring> baker, in SpriteRenderData renderData)
            where TAuthoring : Component
        {
            //baker.DependsOn(renderData.Material);
            baker.DependsOn(renderData.PropertiesSet);
            
            var entity = baker.GetEntity(TransformUsageFlags.None);
            // this comes from NSprites-Foundation and appears as common way to register renderers in runtime
            baker.AddComponentObject(entity, new SpriteRenderDataToRegister { data = renderData });
            // this comes from NSprites as extension method to add all required components to entity to let it be sprite rendered
            baker.AddSpriteRenderComponents(entity, renderData.ID);
        }

        /// <summary>
        /// Bakes all passed data to make entity be able to rendered though shader from this package.
        /// If you use another shader and you need some another data, please implement your own baking method.
        /// </summary>
        public static void BakeSpriteRender<TAuthoring>(this Baker<TAuthoring> baker, in Entity entity, TAuthoring authoring, in float4 uvAtlas, in float4 uvTilingAndOffset, in float2 pivot, in float2 scale, bool flipX = false, bool flipY = false)
            where TAuthoring : Component
        {
            if (baker == null)
            {
                Debug.LogError(new NSpritesException($"Passed baker is null"), authoring.gameObject);
                return;
            }
            if (authoring == null)
            {
                Debug.LogError(new NSpritesException($"Passed authoring object is null"), authoring.gameObject);
                return;
            }

            baker.AddComponent(entity, new UVAtlas { value = uvAtlas });
            baker.AddComponent(entity, new UVTilingAndOffset { value = uvTilingAndOffset });
            baker.AddComponent(entity, new Pivot { value = pivot });
            baker.AddComponent(entity, new Scale2D { value = scale });
            baker.AddComponent(entity, new Flip { Value = new(flipX ? -1 : 0, flipY ? -1 : 0) });
        }
    }
}