#if UNITY_EDITOR
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial class Transform2DConversionSystem : SystemBase
    {
        private bool TryGetPrimaryEntity(in EntityManager.EntityManagerDebug entityManagerDebug, Component comp, out Entity entity)
        {
            var childEntities = new NativeList<Entity>(1, Allocator.Temp);
            entityManagerDebug.GetEntitiesForAuthoringObject(comp, childEntities);
            var findAny = childEntities.Length != 0;
            entity = findAny
                ? childEntities[0]
                : Entity.Null;
            return findAny;
        }
        private Entity GetPrimaryEntity(in EntityManager.EntityManagerDebug entityManagerDebug, Component comp)
        {
            return !TryGetPrimaryEntity(entityManagerDebug, comp, out var result)
                ? throw new System.Exception($"There is no converted entities from {comp.gameObject.name} gameobjects")
                : result;
        }
        private void Convert(Transform transform, in Entity entity, in float2 parentWorldPosition, in Entity parentEntity, EntityCommandBuffer ecb, in EntityManager.EntityManagerDebug entityManagerDebug)
        {
            var pos = transform.position;
            var worldPosition = new float2(pos.x, pos.y);
            ecb.AddComponent(entity, new WorldPosition2D { value = worldPosition });

            if (parentEntity != Entity.Null)
            {
                ecb.AddComponent(entity, new LocalPosition2D { value = worldPosition - parentWorldPosition });
                ecb.AddComponent(entity, new Parent2D { value = parentEntity });
                _ = EntityManager.HasComponent<Child2D>(parentEntity)
                        ? SystemAPI.GetBuffer<Child2D>(parentEntity)
                        : ecb.AddBuffer<Child2D>(parentEntity);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var childEntity = GetPrimaryEntity(entityManagerDebug, child);
                if (childEntity == Entity.Null || EntityManager.HasComponent<ExcludeFrom2DConversion>(childEntity)/*child.TryGetComponent<ExcludeFrom2DConversion>(out _)*/)
                    continue;

                Convert(child, childEntity, worldPosition, entity, ecb, entityManagerDebug);
            }
        }
        bool NestedEntity(Transform parentTransform, in EntityManager.EntityManagerDebug entityManagerDebug)
        {
            // there is no parent at all, so entity isn't nested
            if (parentTransform == null)
                return false;

            // if there is no conversion for parent, so entity isn't nested
            if(!TryGetPrimaryEntity(entityManagerDebug, parentTransform, out var parentEntity))
                return false;

            // parent has 2D transform which means this entity for sure is nested
            if (EntityManager.HasComponent<Transform2DRequest>(parentEntity))
                return true;

            // parent has no 2D transform, but it can have grandparents with 2D transform still, so check recursively
            return NestedEntity(parentTransform.parent, entityManagerDebug);
        }
        
        protected override void OnUpdate()
        {
            // need to retrieve entity from authoring gameobject
            var entityManagerDebug = EntityManager.Debug;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities
                .WithEntityQueryOptions(EntityQueryOptions.IncludePrefab)
                .WithNone<ExcludeFrom2DConversion>()
                .ForEach((Entity entity, Transform2DRequest transform2D) =>
                {
                    
                    var transform = transform2D.sourceGameObject.transform;
                    if (!NestedEntity(transform.parent, entityManagerDebug))
                        Convert(transform, entity, default, Entity.Null, ecb, entityManagerDebug);
                })
                .WithoutBurst()
                .Run();

            ecb.Playback(EntityManager);
        }
    }
}
#endif