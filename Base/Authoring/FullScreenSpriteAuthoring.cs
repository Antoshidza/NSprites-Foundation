using Unity.Entities;
using UnityEngine;

namespace NSprites
{
    public class FullScreenSpriteAuthoring : MonoBehaviour
    {
        [SerializeField] private SpriteRendererAuthoring _spriteAuthoring;
        
        private partial class Baker : Baker<FullScreenSpriteAuthoring>
        {
            public override void Bake(FullScreenSpriteAuthoring authoring)
            {
                if(authoring._spriteAuthoring == null)
                    return;
                
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<FullScreenSpriteTag>(entity);
                AddComponent(entity, new NativeSpriteSize{ Value = authoring._spriteAuthoring.Sprite.GetSize() });

                DependsOn(authoring._spriteAuthoring);
            }
        }
    }
}