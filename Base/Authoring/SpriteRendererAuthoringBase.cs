using NSprites.Modules;
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
                if (!authoring.IsValid)
                    return;

                this.BakeSpriteBase(authoring.RenderData);
            }
        }

        protected abstract SpriteRenderData RenderData { get; }

        /// <summary>While returns true base baker works, otherwise does nothing</summary>
        protected virtual bool IsValid => true;
    }
}