using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Burst.Intrinsics;

namespace NSprites
{
    [UpdateBefore(typeof(LocalToParent2DSystem))]
    [UpdateInGroup(typeof(Unity.Transforms.TransformSystemGroup))]
    public partial struct Parent2DSystem : ISystem
    {
        public struct SystemData : IComponentData
        {
            public EntityQuery MissingChildren;
            public EntityQuery LastParentLessChildren;
            public EntityQuery ReparentedChildren;
            public EntityQuery MissingParents;
            public EntityQuery LastParentWithoutParent;
            public EntityQuery StaticRelationshipsAlone;
            public EntityQuery ChildBufferAlone;

            internal ComponentTypeSet ComponentsToRemoveFromUnparentedChildren;
        }

        [BurstCompile]
        private struct GatherReparentedChildrenDataJob : IJobChunk
        {
            public NativeParallelMultiHashMap<Entity, Entity>.ParallelWriter parentChildToAdd;
            public NativeParallelMultiHashMap<Entity, Entity>.ParallelWriter parentChildToRemove;
            public NativeParallelHashSet<Entity>.ParallelWriter uniqueAffectedParents;
            [ReadOnly] public ComponentTypeHandle<Parent2D> parent_CTH;
            public ComponentTypeHandle<LastParent2D> lastParent_CTH;
            [ReadOnly] public EntityTypeHandle entityTypeHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var chunkParents = chunk.GetNativeArray(ref parent_CTH);
                var chunkLastParents = chunk.GetNativeArray(ref lastParent_CTH);
                var entities = chunk.GetNativeArray(entityTypeHandle);

                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {
                    var parentEntity = chunkParents[entityIndex].value;
                    var lastParentEntity = chunkLastParents[entityIndex].value;

                    //means there is real parent changing
                    if (parentEntity != lastParentEntity)
                    {
                        var entity = entities[entityIndex];
                        parentChildToAdd.Add(parentEntity, entity);
                        uniqueAffectedParents.Add(parentEntity);

                        if (lastParentEntity != Entity.Null)
                        {
                            parentChildToRemove.Add(lastParentEntity, entity);
                            uniqueAffectedParents.Add(lastParentEntity);
                        }
                    }

                    chunkLastParents[entityIndex] = new LastParent2D { value = parentEntity };
                }
            }
        }
        [BurstCompile]
        private struct GatherMissingChildrenDataJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LastParent2D> lastParent_CTH;
            [ReadOnly] public EntityTypeHandle entityTypeHandle;
            public NativeParallelMultiHashMap<Entity, Entity>.ParallelWriter parentChildToRemove;
            public NativeParallelHashSet<Entity>.ParallelWriter uniqueAffectedParents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var chunkEntities = chunk.GetNativeArray(entityTypeHandle);
                var chunkLastParents = chunk.GetNativeArray(ref lastParent_CTH);

                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {
                    var childEntity = chunkEntities[entityIndex];
                    var lastParent = chunkLastParents[entityIndex].value;
                    if (lastParent != Entity.Null)
                    {
                        parentChildToRemove.Add(lastParent, childEntity);
                        uniqueAffectedParents.Add(lastParent);
                    }
                }
            }
        }
        [BurstCompile]
        private struct FixupRelationsJob : IJobParallelForBatch
        {
            [ReadOnly] public NativeArray<Entity> uniqueAffectedParents;
            [ReadOnly] public NativeParallelMultiHashMap<Entity, Entity> parentChildToAdd;
            [ReadOnly] public NativeParallelMultiHashMap<Entity, Entity> parentChildToRemove;
            public EntityCommandBuffer.ParallelWriter ecb;
            [NativeDisableParallelForRestriction] public BufferLookup<Child2D> child_BL;

            public void Execute(int startIndex, int count)
            {
                var forCount = startIndex + count;
                var childList = new NativeList<Entity>(Allocator.Temp);
                for (int parentIndex = startIndex; parentIndex < forCount; parentIndex++)
                {
                    var inJobIndex = parentIndex + startIndex;
                    var parentEntity = uniqueAffectedParents[parentIndex];
                    DynamicBuffer<Child2D> childBuffer = default;

                    var parentHasNewChildren = parentChildToAdd.TryGetFirstValue(parentEntity, out var addChildEntity, out var addIterator);
                    var parentHasRemovedChildren = parentChildToRemove.TryGetFirstValue(parentEntity, out var removeChildEntity, out var removeIterator);

                    if (!parentHasNewChildren && !parentHasRemovedChildren)
                        return;

                    var parentHasChildBuffer = child_BL.HasBuffer(parentEntity);

                    //if parent has removed children and existing child buffer then we want to clear it
                    if (parentHasRemovedChildren && parentHasChildBuffer)
                    {
                        childBuffer = child_BL[parentEntity];
                        do
                            childList.Add(removeChildEntity);
                        while (parentChildToRemove.TryGetNextValue(out removeChildEntity, ref removeIterator));
                        //if remove list is the same length and there will be no new children then we can safely remove buffer
                        if (childList.Length == childBuffer.Length && !parentHasNewChildren)
                            ecb.RemoveComponent<Child2D>(inJobIndex, parentEntity);
                        //otherwise we want to carefully extract all remove children
                        else
                        {
                            for (int i = 0; i < childList.Length; i++)
                            {
                                var childInBufferIndex = GetChildIndex(childBuffer, childList[i]);
                                if (childInBufferIndex != -1)
                                    childBuffer.RemoveAtSwapBack(childInBufferIndex);
                            }
                        }
                        childList.Clear();
                    }

                    //if parent has new children then we want to allocate/access new buffer if needed and fill it with new children
                    if (parentHasNewChildren)
                    {
                        //if parent has no child buffer at all then in every case we just want to allocate new one
                        if (!parentHasChildBuffer)
                            childBuffer = ecb.AddBuffer<Child2D>(inJobIndex, parentEntity);
                        //if there is buffer and we not cache it in "Remove" section, then cache it here
                        else if (!parentHasRemovedChildren)
                            childBuffer = child_BL[parentEntity];

                        do
                        {
                            if (!Contains(childBuffer, addChildEntity))
                                childList.Add(addChildEntity);
                        }
                        while (parentChildToAdd.TryGetNextValue(out addChildEntity, ref addIterator));

                        childBuffer.AddRange(childList.AsArray().Reinterpret<Child2D>());
                        childList.Clear();
                    }
                }
            }
            private int GetChildIndex(in DynamicBuffer<Child2D> children, in Entity childEntity)
            {
                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                    if (children[childIndex].value == childEntity)
                        return childIndex;
                return -1;
            }
            private bool Contains(in DynamicBuffer<Child2D> children, in Entity childEntity)
            {
                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                    if (children[childIndex].value == childEntity)
                        return true;
                return false;
            }
        }
        [BurstCompile]
        private struct UnparentChildrenJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle entityTypeHandle;
            [ReadOnly] public BufferTypeHandle<Child2D> child_BTH;
            [ReadOnly] public ComponentLookup<Parent2D> parent_CL;
            public EntityCommandBuffer.ParallelWriter ecb;
            public ComponentTypeSet componentsToRemoveFromChildren;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var chunkEntities = chunk.GetNativeArray(entityTypeHandle);
                var chunkChildren = chunk.GetBufferAccessor(ref child_BTH);

                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {
                    var parentEntity = chunkEntities[entityIndex];
                    var childBuffer = chunkChildren[entityIndex];

                    for (int childIndex = 0; childIndex < childBuffer.Length; childIndex++)
                    {
                        var childEntity = childBuffer[childIndex].value;
                        if (!parent_CL.HasComponent(childEntity) || parent_CL[childEntity].value == parentEntity)
                            ecb.RemoveComponent(unfilteredChunkIndex, childEntity, componentsToRemoveFromChildren);
                    }
                }
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var systemData = new SystemData();
            
            var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
            queryBuilder
                .WithAll<LastParent2D>()
                .WithNone<Parent2D>()
                .WithNone<StaticRelationshipsTag>();

            systemData.MissingChildren = state.GetEntityQuery(queryBuilder);
            queryBuilder.Reset();

            queryBuilder
                .WithAll<Parent2D>()
                .WithNone<LastParent2D>()
                //no need to attach LastParent2D to entities which won't be handled by that system
                .WithNone<StaticRelationshipsTag>();
            systemData.LastParentLessChildren = state.GetEntityQuery(queryBuilder);
            queryBuilder.Reset();

            queryBuilder
                .WithAll<Parent2D>()
                .WithAll<LastParent2D>()
                .WithNone<StaticRelationshipsTag>()
                //we want to be sure we deal with transform entities, because LocalToParent2DSystem will access this components
                .WithAll<LocalPosition2D>()
                .WithAll<WorldPosition2D>();
            var reparentChildrenQuery = state.GetEntityQuery(queryBuilder);
            reparentChildrenQuery.SetChangedVersionFilter(ComponentType.ReadOnly<Parent2D>());
            systemData.ReparentedChildren = reparentChildrenQuery;
            queryBuilder.Reset();

            queryBuilder
                .WithAll<Child2D>()
                .WithNone<WorldPosition2D>()
                .WithNone<StaticRelationshipsTag>();
            systemData.MissingParents = state.GetEntityQuery(queryBuilder);
            queryBuilder.Reset();

            queryBuilder
                .WithAll<LastParent2D>()
                .WithNone<Parent2D>();
            systemData.LastParentWithoutParent = state.GetEntityQuery(queryBuilder);
            queryBuilder.Reset();

            queryBuilder
                .WithAll<StaticRelationshipsTag>()
                .WithNone<Child2D>()
                .WithNone<Parent2D>();
            systemData.StaticRelationshipsAlone = state.GetEntityQuery(queryBuilder);
            queryBuilder.Reset();

            queryBuilder
                .WithAll<Child2D>()
                .WithNone<WorldPosition2D>();
            systemData.ChildBufferAlone = state.GetEntityQuery(queryBuilder);

            queryBuilder.Dispose();
            
            systemData.ComponentsToRemoveFromUnparentedChildren = new
            (
                ComponentType.ReadOnly<Parent2D>(),
                ComponentType.ReadOnly<LastParent2D>(),
                ComponentType.ReadOnly<LocalPosition2D>()
            );

            state.EntityManager.AddComponentData(state.SystemHandle, systemData);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var systemData = SystemAPI.GetComponent<SystemData>(state.SystemHandle);
            
            //children without LastParent2D must have one
            if (!systemData.LastParentLessChildren.IsEmptyIgnoreFilter)
                state.EntityManager.AddComponent(systemData.LastParentLessChildren, ComponentType.ReadOnly<Parent2D>());

            if (!systemData.MissingParents.IsEmptyIgnoreFilter)
            {
                var unparentECB = new EntityCommandBuffer(Allocator.TempJob);
                state.Dependency = new UnparentChildrenJob
                {
                    entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
                    child_BTH = SystemAPI.GetBufferTypeHandle<Child2D>(true),
                    parent_CL = SystemAPI.GetComponentLookup<Parent2D>(true),
                    ecb = unparentECB.AsParallelWriter(),
                    componentsToRemoveFromChildren = systemData.ComponentsToRemoveFromUnparentedChildren
                }.ScheduleParallel(systemData.MissingParents, state.Dependency);
                state.Dependency.Complete();
                unparentECB.Playback(state.EntityManager);
                state.EntityManager.RemoveComponent<Child2D>(systemData.MissingParents);
                unparentECB.Dispose();
            }

            var missingChildrenIsEmpty = systemData.MissingChildren.IsEmptyIgnoreFilter;
            var reparentedChildrenIsEmpty = systemData.ReparentedChildren.IsEmpty;

            if (!missingChildrenIsEmpty || !reparentedChildrenIsEmpty)
            {
                var potentialRemoveCount = 0;
                var potentialAddCount = 0;
                if (!missingChildrenIsEmpty)
                    potentialRemoveCount += systemData.MissingChildren.CalculateEntityCount();
                if (!reparentedChildrenIsEmpty)
                {
                    var reparentedCount = systemData.ReparentedChildren.CalculateEntityCount();
                    potentialAddCount += reparentedCount;
                    potentialRemoveCount += reparentedCount;
                }
                //remove count is always bigger or equal to add count, so we can use it like max potential size. * 2 because there can be N parent + N last parent and all unique
                var uniqueParents = new NativeParallelHashSet<Entity>(potentialRemoveCount * 2, Allocator.TempJob);
                var parentChildToRemove = new NativeParallelMultiHashMap<Entity, Entity>(potentialRemoveCount, Allocator.TempJob);
                var parentChildToAdd = new NativeParallelMultiHashMap<Entity, Entity>(potentialAddCount, Allocator.TempJob);

                var uniqueParents_PW = uniqueParents.AsParallelWriter();
                var parentChildToRemove_PW = parentChildToRemove.AsParallelWriter();

                state.Dependency = new GatherMissingChildrenDataJob
                {
                    entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
                    lastParent_CTH = SystemAPI.GetComponentTypeHandle<LastParent2D>(true),
                    uniqueAffectedParents = uniqueParents_PW,
                    parentChildToRemove = parentChildToRemove_PW
                }.ScheduleParallel(systemData.MissingChildren, state.Dependency);

                state.Dependency = new GatherReparentedChildrenDataJob
                {
                    parentChildToAdd = parentChildToAdd.AsParallelWriter(),
                    parentChildToRemove = parentChildToRemove_PW,
                    uniqueAffectedParents = uniqueParents_PW,
                    entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
                    parent_CTH = SystemAPI.GetComponentTypeHandle<Parent2D>(true),
                    lastParent_CTH = SystemAPI.GetComponentTypeHandle<LastParent2D>(false)
                }.ScheduleParallel(systemData.ReparentedChildren, state.Dependency);

                state.Dependency.Complete();

                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var uniqueParentArray = uniqueParents.ToNativeArray(Allocator.TempJob);
                state.Dependency = new FixupRelationsJob
                {
                    parentChildToAdd = parentChildToAdd,
                    parentChildToRemove = parentChildToRemove,
                    uniqueAffectedParents = uniqueParentArray,
                    child_BL = SystemAPI.GetBufferLookup<Child2D>(false),
                    ecb = ecb.AsParallelWriter(),
                }.ScheduleBatch(uniqueParentArray.Length, 32, default);

                uniqueParents.Dispose();

                state.Dependency.Complete();
                ecb.Playback(state.EntityManager);

                ecb.Dispose();
                uniqueParentArray.Dispose();
                parentChildToAdd.Dispose();
                parentChildToRemove.Dispose();
            }

            if (!systemData.LastParentWithoutParent.IsEmptyIgnoreFilter)
                state.EntityManager.RemoveComponent<LastParent2D>(systemData.LastParentWithoutParent);

            if (!systemData.StaticRelationshipsAlone.IsEmptyIgnoreFilter)
                state.EntityManager.RemoveComponent<StaticRelationshipsTag>(systemData.StaticRelationshipsAlone);

            if (!systemData.ChildBufferAlone.IsEmptyIgnoreFilter)
                state.EntityManager.RemoveComponent<Child2D>(systemData.ChildBufferAlone);
        }
    }
}