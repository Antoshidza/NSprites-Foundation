using Unity.Collections;
using Unity.Entities;

namespace NSprites
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public partial class RegisterRenderersSystem : SystemBase
    {
        private EntityQuery _renderArchetypeToRegisterQuery;
        private EntityQuery _renderArchetypeIndexLessEntitiesQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            _renderArchetypeToRegisterQuery = GetEntityQuery
            (
                new EntityQueryDesc
                {
                    All = new []
                    {
                        ComponentType.ReadOnly<SpriteRenderDataToRegister>(),
                        ComponentType.ReadOnly<SpriteRenderID>()
                    },
                    Options = EntityQueryOptions.IncludePrefab
                }
            );
            _renderArchetypeIndexLessEntitiesQuery = GetEntityQuery
            (
                 new EntityQueryDesc
                 {
                    All = new []
                    {
                        ComponentType.ReadOnly<SpriteRenderDataToRegister>()
                    },
                    None = new []
                    {
                        ComponentType.ReadOnly<SpriteRenderID>()
                    },
                    Options = EntityQueryOptions.IncludePrefab
                 }
            );
    }
        
        protected override void OnUpdate()
        {
            EntityManager.AddComponent<SpriteRenderID>(_renderArchetypeIndexLessEntitiesQuery);

            Register(_renderArchetypeToRegisterQuery.ToEntityArray(Allocator.Temp));

            EntityManager.RemoveComponent<SpriteRenderDataToRegister>(_renderArchetypeToRegisterQuery);
            return;

            void Register(in NativeArray<Entity> entities)
            {
                if (!SystemAPI.ManagedAPI.TryGetSingleton<RenderArchetypeStorage>(out var renderArchetypeStorage))
                    return;

                for(var i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var renderData = EntityManager.GetComponentObject<SpriteRenderDataToRegister>(entity);

                    if (!renderArchetypeStorage.ContainsRender(renderData.data.ID))
                        renderArchetypeStorage.RegisterRender
                        (
                            renderData.data.ID,
                            renderData.data.Material,
                            propertyDataSet: renderData.data.PropertiesSet.PropertyData,
                            initialCapacity: 128,
                            capacityStep: 128
                        );

                    EntityManager.SetSharedComponentManaged(entity, new SpriteRenderID { id = renderData.data.ID });
                }
            }
        }
    }
}
