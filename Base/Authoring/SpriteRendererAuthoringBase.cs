using System;
using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    /// <summary>
    /// Gets <see cref="SpriteRenderData"/> through virtual <see cref="RenderData"/> property then adds <see cref="SpriteRenderDataToRegister"/>.
    /// Lately baking system will catch those entities and add needed components for rendering. 
    /// </summary>
    public abstract class SpriteRendererAuthoringBase : MonoBehaviour
    {
        [BakeDerivedTypes]
        private class Baker : Baker<SpriteRendererAuthoringBase>
        {
            public override void Bake(SpriteRendererAuthoringBase authoring)
            {
                var renderData = authoring.RenderData;

                if (renderData.Material == null)
                {
                    Debug.LogException(new ArgumentException($"{nameof(SpriteRenderData.Material)} is null"), authoring.gameObject);
                    return;
                }
                if (renderData.PropertiesSet == null)
                {
                    Debug.LogException(new ArgumentException($"{nameof(SpriteRenderData.PropertiesSet)} is null"), authoring.gameObject);
                    return;
                }

                DependsOn(renderData.PropertiesSet);
                AddComponentObject(new SpriteRenderDataToRegister { data = renderData });
                this.AddSpriteRenderComponents(renderData.ID);
            }
        }

        protected abstract SpriteRenderData RenderData { get; }
    }
}