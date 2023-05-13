using Unity.Entities;
using UnityEngine;

namespace NSprites.Modules
{
    public static partial class BakerExtensions
    {
        public static void BakeSpriteBase<TAuthoring>(this Baker<TAuthoring> baker, in SpriteRenderData renderData)
            where TAuthoring : Component
        {
            baker.DependsOn(renderData.Material);
            baker.DependsOn(renderData.PropertiesSet);
            
            var entity = baker.GetEntity(TransformUsageFlags.None);
            // this comes from NSprites-Foundation and appears as common way to register renderers in runtime
            baker.AddComponentObject(entity, new SpriteRenderDataToRegister { data = renderData });
            // this comes from NSprites as extension method to add all required components to entity to let it be sprite rendered
            baker.AddSpriteRenderComponents(entity, renderData.ID);
        }
    }
}