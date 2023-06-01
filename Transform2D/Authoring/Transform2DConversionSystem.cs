#if UNITY_EDITOR
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static NSprites.MathHelper;



namespace NSprites
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial class Transform2DConversionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // need to retrieve entity from authoring gameobject
            var entityDebugManager = EntityManager.Debug;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities
                .WithEntityQueryOptions(EntityQueryOptions.IncludePrefab)
                .WithNone<ExcludeFrom2DConversion>()
                .ForEach((Entity entity, Transform2DRequest transform2D) =>
                {
                    bool TryGetPrimaryEntity(Component comp, out Entity entity)
                    {
                        NativeList<Entity> childEntities = new (1, Allocator.Temp);
                        entityDebugManager.GetEntitiesForAuthoringObject(comp, childEntities);
                        bool findAny = childEntities.Length != 0;
                        entity = findAny
                            ? childEntities[0]
                            : Entity.Null;
                        return findAny;
                    }
                    
                    Entity GetPrimaryEntity(Component comp)
                    {
                        return !TryGetPrimaryEntity(comp, out Entity result)
                            ? throw new System.Exception($"There is no converted entities from {comp.gameObject.name} gameobjects")
                            : result;
                    }
                    
                    void Convert(Transform transform, in Entity entity, in Entity parentEntity)
                    {
                        ecb.AddComponent(entity, new LocalToWorld2D { Value = transform.localToWorldMatrix});
                        ecb.AddComponent(entity, new LocalTransform2D
                        {
                            Position = float2(transform.localPosition),
                            Rotation = transform.localRotation,
                            Scale = float2(transform.localScale)
                        });

                        if (parentEntity != Entity.Null)
                        {
                            ecb.AddComponent(entity, new Parent2D { Value = parentEntity });
                            _ = EntityManager.HasComponent<Child2D>(parentEntity)
                                    ? SystemAPI.GetBuffer<Child2D>(parentEntity)
                                    : ecb.AddBuffer<Child2D>(parentEntity);
                        }

                        for (int i = 0; i < transform.childCount; i++)
                        {
                            Transform child = transform.GetChild(i);
                            Entity childEntity = GetPrimaryEntity(child);
                            if (childEntity == Entity.Null || EntityManager.HasComponent<ExcludeFrom2DConversion>(childEntity)/*child.TryGetComponent<ExcludeFrom2DConversion>(out _)*/)
                                continue;

                            Convert(child, childEntity, entity);
                        }
                    }
                    
                    bool NestedEntity(Transform parentTransform)
                    {
                        // there is no parent at all, so entity isn't nested
                        if (parentTransform == null)
                            return false;

                        // if there is no conversion for parent, so entity isn't nested
                        if(!TryGetPrimaryEntity(parentTransform, out Entity parentEntity))
                            return false;

                        // parent has 2D transform which means this entity for sure is nested
                        if (EntityManager.HasComponent<Transform2DRequest>(parentEntity))
                            return true;

                        // parent has no 2D transform, but it can have grandparents with 2D transform still, so check recursively
                        return NestedEntity(parentTransform.parent);
                    }
                    
                    Transform transform = transform2D.Source.transform;
                    if (!NestedEntity(transform.parent))
                        Convert(transform, entity, Entity.Null);
                })
                .WithoutBurst()
                .Run();

            ecb.Playback(EntityManager);
        }
    }
}
#endif