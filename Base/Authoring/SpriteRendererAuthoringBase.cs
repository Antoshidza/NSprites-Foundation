using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Gets <see cref="SpriteRenderData"/> through virtual <see cref="RenderData"/> property then adds <see cref="SpriteRenderDataToRegister"/>.
    /// Lately <see cref="SpriteRenderBakingSystem"/> will catch those entities and add needed components for rendering. 
    /// </summary>
    public abstract class SpriteRendererAuthoringBase : MonoBehaviour
    {
        [BakeDerivedTypes]
        private class Baker : Baker<SpriteRendererAuthoringBase>
        {
            public override void Bake(SpriteRendererAuthoringBase authoring)
            {
                var renderData = authoring.RenderData;
                
                if(renderData.Material == null || renderData.PropertiesSet == null)
                    return;

                DependsOn(renderData.PropertiesSet);
                AddComponentObject(new SpriteRenderDataToRegister { data = renderData });
                this.AddSpriteRenderComponents(renderData.ID);
            }
        }

        protected abstract SpriteRenderData RenderData { get; }
    }
}