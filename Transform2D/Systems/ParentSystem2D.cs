using System;
using System.Diagnostics;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;
using Unity.Transforms;

namespace NSprites
{
    /// <summary>
    /// This system maintains parent/child relationships between entities within a transform hierarchy.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct ParentSystem2D : ISystem
    {
        private EntityQuery mNewParentsQuery;
        private EntityQuery mRemovedParentsQuery;
        private EntityQuery mExistingParentsQuery;
        private EntityQuery mDeletedParentsQuery;

        private static readonly ProfilerMarker kProfileDeletedParents = new ProfilerMarker("ParentSystem.DeletedParents");
        private static readonly ProfilerMarker kProfileRemoveParents  = new ProfilerMarker("ParentSystem.RemoveParents");
        private static readonly ProfilerMarker kProfileChangeParents  = new ProfilerMarker("ParentSystem.ChangeParents");
        private static readonly ProfilerMarker kProfileNewParents     = new ProfilerMarker("ParentSystem.NewParents");

        private BufferLookup<Child2D> childLookupRo;
        private BufferLookup<Child2D> childLookupRw;
        private ComponentLookup<Parent2D> parentFromEntityRO;
        private ComponentTypeHandle<PreviousParent2D> previousParentTypeHandleRW;
        private EntityTypeHandle entityTypeHandle;
        private ComponentTypeHandle<Parent2D> parentTypeHandleRO;


        private int FindChildIndex(DynamicBuffer<Child2D> children, Entity entity)
        {
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].Value == entity)
                    return i;
            }

            throw new InvalidOperationException("Child entity not in parent");
        }


        private void RemoveChildFromParent(ref SystemState state, Entity childEntity, Entity parentEntity)
        {
            if (!state.EntityManager.HasComponent<Child2D>(parentEntity))
                return;

            DynamicBuffer<Child2D> children   = state.EntityManager.GetBuffer<Child2D>(parentEntity);
            int             childIndex = FindChildIndex(children, childEntity);
            children.RemoveAt(childIndex);
            if (children.Length == 0)
            {
                state.EntityManager.RemoveComponent(parentEntity, ComponentType.FromTypeIndex(
                    TypeManager.GetTypeIndex<Child2D>()));
            }
        }

        [BurstCompile]
        private struct GatherChangedParents : IJobChunk
        {
            public NativeParallelMultiHashMap<Entity, Entity>.ParallelWriter ParentChildrenToAdd;
            public NativeParallelMultiHashMap<Entity, Entity>.ParallelWriter ParentChildrenToRemove;
            public NativeParallelHashSet<Entity>.ParallelWriter ChildParentToRemove;   // Children that have a Parent component, but that parent does not exist (deleted before ParentSystem runs)
            public NativeParallelHashMap<Entity, int>.ParallelWriter UniqueParents;
            public ComponentTypeHandle<PreviousParent2D> PreviousParentTypeHandle;
            public EntityStorageInfoLookup EntityStorageInfoLookup;

            [ReadOnly] public BufferLookup<Child2D> ChildLookup;

            [ReadOnly] public ComponentTypeHandle<Parent2D> ParentTypeHandle;
            [ReadOnly] public EntityTypeHandle EntityTypeHandle;
            public uint LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Assert.IsFalse(useEnabledMask);

                if (chunk.DidChange(ref ParentTypeHandle, LastSystemVersion) ||
                    chunk.DidChange(ref PreviousParentTypeHandle, LastSystemVersion))
                {
                    NativeArray<PreviousParent2D> chunkPreviousParents = chunk.GetNativeArray(ref PreviousParentTypeHandle);
                    NativeArray<Parent2D>         chunkParents         = chunk.GetNativeArray(ref ParentTypeHandle);
                    NativeArray<Entity>           chunkEntities        = chunk.GetNativeArray(EntityTypeHandle);

                    for (int j = 0, chunkEntityCount = chunk.Count; j < chunkEntityCount; j++) {
                        if (chunkParents[j].Value == chunkPreviousParents[j].Value) continue;
                        
                        Entity childEntity          = chunkEntities[j];
                        Entity parentEntity         = chunkParents[j].Value;
                        Entity previousParentEntity = chunkPreviousParents[j].Value;

                        if (!EntityStorageInfoLookup.Exists(parentEntity))
                        {
                            // If we get here, the Parent component is pointing to an invalid entity
                            // This can happen, for example, if a parent has been deleted before ParentSystem has had a chance to add a PreviousParent component
                            ChildParentToRemove.Add(childEntity);
                            continue;
                        }

                        ParentChildrenToAdd.Add(parentEntity, childEntity);
                        UniqueParents.TryAdd(parentEntity, 0);

                        if (ChildLookup.HasBuffer(previousParentEntity))
                        {
                            ParentChildrenToRemove.Add(previousParentEntity, childEntity);
                            UniqueParents.TryAdd(previousParentEntity, 0);
                        }

                        chunkPreviousParents[j] = new PreviousParent2D
                        {
                            Value = parentEntity
                        };
                    }
                }
            }
        }

        [BurstCompile]
        private struct FindMissingChild : IJob
        {
            [ReadOnly] public NativeParallelHashMap<Entity, int> UniqueParents;
            [ReadOnly] public BufferLookup<Child2D> ChildLookup;
            public NativeList<Entity> ParentsMissingChild;

            public void Execute()
            {
                NativeArray<Entity> parents = UniqueParents.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < parents.Length; i++)
                {
                    Entity parent = parents[i];
                    if (!ChildLookup.HasBuffer(parent))
                    {
                        ParentsMissingChild.Add(parent);
                    }
                }
            }
        }

        [BurstCompile]
        private struct FixupChangedChildren : IJob
        {
            [ReadOnly] public NativeParallelMultiHashMap<Entity, Entity> ParentChildrenToAdd;
            [ReadOnly] public NativeParallelMultiHashMap<Entity, Entity> ParentChildrenToRemove;
            [ReadOnly] public NativeParallelHashMap<Entity, int> UniqueParents;

            public BufferLookup<Child2D> ChildLookup;

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private static void ThrowChildEntityNotInParent()
            {
                throw new InvalidOperationException("Child entity not in parent");
            }


            private int FindChildIndex(DynamicBuffer<Child2D> children, Entity entity)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].Value == entity)
                        return i;
                }

                ThrowChildEntityNotInParent();
                return -1;
            }


            private void RemoveChildrenFromParent(Entity parent, DynamicBuffer<Child2D> children)
            {
                if (ParentChildrenToRemove.TryGetFirstValue(parent, out Entity child, out NativeParallelMultiHashMapIterator<Entity> it))
                {
                    do
                    {
                        int childIndex = FindChildIndex(children, child);
                        children.RemoveAt(childIndex);
                    }
                    while (ParentChildrenToRemove.TryGetNextValue(out child, ref it));
                }
            }


            private void AddChildrenToParent(Entity parent, DynamicBuffer<Child2D> children)
            {
                if (ParentChildrenToAdd.TryGetFirstValue(parent, out Entity child, out NativeParallelMultiHashMapIterator<Entity> it))
                {
                    do
                    {
                        children.Add(new Child2D() { Value = child });
                    }
                    while (ParentChildrenToAdd.TryGetNextValue(out child, ref it));
                }
            }

            public void Execute()
            {
                NativeArray<Entity> parents = UniqueParents.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < parents.Length; i++)
                {
                    Entity parent = parents[i];

                    if (ChildLookup.TryGetBuffer(parent, out DynamicBuffer<Child2D> children))
                    {
                        RemoveChildrenFromParent(parent, children);
                        AddChildrenToParent(parent, children);
                    }
                }
            }
        }

        /// <inheritdoc cref="ISystem.OnCreate"/>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            childLookupRo = state.GetBufferLookup<Child2D>(true);
            childLookupRw = state.GetBufferLookup<Child2D>();
            parentFromEntityRO = state.GetComponentLookup<Parent2D>(true);
            previousParentTypeHandleRW = state.GetComponentTypeHandle<PreviousParent2D>(false);
            parentTypeHandleRO = state.GetComponentTypeHandle<Parent2D>(true);
            entityTypeHandle = state.GetEntityTypeHandle();

            EntityQueryBuilder builder0 = new EntityQueryBuilder(Allocator.Temp)
                                         .WithAll<Parent2D>()
                                         .WithNone<PreviousParent2D>()
                                         .WithOptions(EntityQueryOptions.FilterWriteGroup);
            mNewParentsQuery = state.GetEntityQuery(builder0);

            EntityQueryBuilder builder1 = new EntityQueryBuilder(Allocator.Temp)
                                         .WithAllRW<PreviousParent2D>()
                                         .WithNone<Parent2D>()
                                         .WithOptions(EntityQueryOptions.FilterWriteGroup);
            mRemovedParentsQuery = state.GetEntityQuery(builder1);

            EntityQueryBuilder builder2 = new EntityQueryBuilder(Allocator.Temp)
                                         .WithAll<Parent2D>()
                                         .WithAllRW<PreviousParent2D>()
                                         .WithOptions(EntityQueryOptions.FilterWriteGroup);
            mExistingParentsQuery = state.GetEntityQuery(builder2);
            mExistingParentsQuery.ResetFilter();
            mExistingParentsQuery.AddChangedVersionFilter(ComponentType.ReadWrite<Parent2D>());
            mExistingParentsQuery.AddChangedVersionFilter(ComponentType.ReadWrite<PreviousParent2D>());

            EntityQueryBuilder builder3 = new EntityQueryBuilder(Allocator.Temp)
                                         .WithAllRW<Child2D>()
                                         .WithNone<LocalToWorld2D>()
                                         .WithOptions(EntityQueryOptions.FilterWriteGroup);
            mDeletedParentsQuery = state.GetEntityQuery(builder3);
        }

        /// <inheritdoc cref="ISystem.OnDestroy"/>
        private void UpdateNewParents(ref SystemState state)
        {
            if (mNewParentsQuery.IsEmptyIgnoreFilter)
                return;

            state.EntityManager.AddComponent(mNewParentsQuery, ComponentType.FromTypeIndex(
                TypeManager.GetTypeIndex<PreviousParent2D>()));
        }


        private void UpdateRemoveParents(ref SystemState state)
        {
            if (mRemovedParentsQuery.IsEmptyIgnoreFilter)
                return;

            NativeArray<Entity>         childEntities   = mRemovedParentsQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.ToAllocator);
            NativeArray<PreviousParent2D> previousParents = mRemovedParentsQuery.ToComponentDataArray<PreviousParent2D>(state.WorldUnmanaged.UpdateAllocator.ToAllocator);

            for (int i = 0; i < childEntities.Length; i++)
            {
                Entity childEntity = childEntities[i];
                Entity previousParentEntity = previousParents[i].Value;

                RemoveChildFromParent(ref state, childEntity, previousParentEntity);
            }

            state.EntityManager.RemoveComponent(mRemovedParentsQuery, ComponentType.FromTypeIndex(
                TypeManager.GetTypeIndex<PreviousParent2D>()));
        }


        private void UpdateChangeParents(ref SystemState state)
        {
            if (mExistingParentsQuery.IsEmptyIgnoreFilter)
                return;

            int count = mExistingParentsQuery.CalculateEntityCount() * 2; // Potentially 2x changed: current and previous
            if (count == 0)
                return;

            // 1. Get (Parent,Child) to remove
            // 2. Get (Parent,Child) to add
            // 3. Get unique Parent change list
            // 4. Set PreviousParent to new Parent
            NativeParallelMultiHashMap<Entity, Entity> parentChildrenToAdd    = new NativeParallelMultiHashMap<Entity, Entity>(count, state.WorldUnmanaged.UpdateAllocator.ToAllocator);
            NativeParallelMultiHashMap<Entity, Entity> parentChildrenToRemove = new NativeParallelMultiHashMap<Entity, Entity>(count, state.WorldUnmanaged.UpdateAllocator.ToAllocator);
            NativeParallelHashSet<Entity>              childParentToRemove    = new NativeParallelHashSet<Entity>(count, state.WorldUnmanaged.UpdateAllocator.ToAllocator);
            NativeParallelHashMap<Entity, int>         uniqueParents          = new NativeParallelHashMap<Entity, int>(count, state.WorldUnmanaged.UpdateAllocator.ToAllocator);

            parentTypeHandleRO.Update(ref state);
            previousParentTypeHandleRW.Update(ref state);
            entityTypeHandle.Update(ref state);
            childLookupRw.Update(ref state);
            GatherChangedParents gatherChangedParentsJob = new GatherChangedParents
            {
                ParentChildrenToAdd = parentChildrenToAdd.AsParallelWriter(),
                ParentChildrenToRemove = parentChildrenToRemove.AsParallelWriter(),
                ChildParentToRemove = childParentToRemove.AsParallelWriter(),
                UniqueParents = uniqueParents.AsParallelWriter(),
                PreviousParentTypeHandle = previousParentTypeHandleRW,
                ChildLookup = childLookupRw,
                EntityStorageInfoLookup = state.GetEntityStorageInfoLookup(),
                ParentTypeHandle = parentTypeHandleRO,
                EntityTypeHandle = entityTypeHandle,
                LastSystemVersion = state.LastSystemVersion
            };
            JobHandle gatherChangedParentsJobHandle = gatherChangedParentsJob.ScheduleParallel(mExistingParentsQuery, state.Dependency);
            gatherChangedParentsJobHandle.Complete();

            // Remove Parent components that are not valid
            NativeArray<Entity> arrayToRemove = childParentToRemove.ToNativeArray(state.WorldUnmanaged.UpdateAllocator.ToAllocator);
            state.EntityManager.RemoveComponent(arrayToRemove, ComponentType.ReadWrite<Parent2D>());

            // 5. (Structural change) Add any missing Child to Parents
            NativeList<Entity> parentsMissingChild = new NativeList<Entity>(state.WorldUnmanaged.UpdateAllocator.ToAllocator);
            childLookupRo.Update(ref state);
            FindMissingChild findMissingChildJob = new FindMissingChild
            {
                UniqueParents = uniqueParents,
                ChildLookup = childLookupRo,
                ParentsMissingChild = parentsMissingChild
            };
            JobHandle findMissingChildJobHandle = findMissingChildJob.Schedule();
            findMissingChildJobHandle.Complete();

            ComponentTypeSet componentsToAdd = new ComponentTypeSet(ComponentType.ReadWrite<Child2D>());
            state.EntityManager.AddComponent(parentsMissingChild.AsArray(), componentsToAdd);

            // 6. Get Child[] for each unique Parent
            // 7. Update Child[] for each unique Parent
            childLookupRw.Update(ref state);
            FixupChangedChildren fixupChangedChildrenJob = new FixupChangedChildren
            {
                ParentChildrenToAdd = parentChildrenToAdd,
                ParentChildrenToRemove = parentChildrenToRemove,
                UniqueParents = uniqueParents,
                ChildLookup = childLookupRw
            };

            JobHandle fixupChangedChildrenJobHandle = fixupChangedChildrenJob.Schedule();
            fixupChangedChildrenJobHandle.Complete();

            // 8. Remove empty Child[] buffer from now-childless parents
            NativeArray<Entity> parents = uniqueParents.GetKeyArray(Allocator.Temp);
            foreach (Entity parentEntity in parents)
            {
                DynamicBuffer<Child2D> children = state.EntityManager.GetBuffer<Child2D>(parentEntity);
                if (children.Length == 0)
                {
                    ComponentTypeSet componentsToRemove = new ComponentTypeSet(ComponentType.ReadWrite<Child2D>());
                    state.EntityManager.RemoveComponent(parentEntity, componentsToRemove);
                }
            }
        }

        [BurstCompile]
        private struct GatherChildEntities : IJob
        {
            [ReadOnly] public NativeArray<Entity> Parents;
            public NativeList<Entity> Children;
            [ReadOnly] public BufferLookup<Child2D> ChildLookup;
            [ReadOnly] public ComponentLookup<Parent2D> ParentFromEntity;

            public void Execute()
            {
                for (int i = 0; i < Parents.Length; i++)
                {
                    Entity             parentEntity        = Parents[i];
                    NativeArray<Child2D> childEntitiesSource = ChildLookup[parentEntity].AsNativeArray();
                    for (int j = 0; j < childEntitiesSource.Length; j++)
                    {
                        Entity childEntity = childEntitiesSource[j].Value;
                        if (ParentFromEntity.TryGetComponent(childEntity, out Parent2D parent) && parent.Value == parentEntity)
                        {
                            Children.Add(childEntitiesSource[j].Value);
                        }
                    }
                }
            }
        }



        private void UpdateDeletedParents(ref SystemState state)
        {
            if (mDeletedParentsQuery.IsEmptyIgnoreFilter)
                return;

            NativeArray<Entity> previousParents = mDeletedParentsQuery.ToEntityArray(state.WorldUnmanaged.UpdateAllocator.ToAllocator);
            NativeList<Entity>  childEntities   = new NativeList<Entity>(state.WorldUnmanaged.UpdateAllocator.ToAllocator);

            childLookupRo.Update(ref state);
            parentFromEntityRO.Update(ref state);
            GatherChildEntities gatherChildEntitiesJob = new GatherChildEntities
            {
                Parents = previousParents,
                Children = childEntities,
                ChildLookup = childLookupRo,
                ParentFromEntity = parentFromEntityRO,
            };
            JobHandle gatherChildEntitiesJobHandle = gatherChildEntitiesJob.Schedule();
            gatherChildEntitiesJobHandle.Complete();

            state.EntityManager.RemoveComponent(
                childEntities.AsArray(),
                new ComponentTypeSet(
                    ComponentType.FromTypeIndex(TypeManager.GetTypeIndex<Parent2D>()),
                    ComponentType.FromTypeIndex(TypeManager.GetTypeIndex<PreviousParent2D>())
                ));
            state.EntityManager.RemoveComponent(mDeletedParentsQuery, ComponentType.FromTypeIndex(
                TypeManager.GetTypeIndex<Child2D>()));
        }

        /// <inheritdoc cref="ISystem.OnUpdate"/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            // TODO: these dotsruntime ifdefs are a workaround for a crash - BUR-1767
#if !UNITY_DOTSRUNTIME
            kProfileDeletedParents.Begin();
#endif
            UpdateDeletedParents(ref state);
#if !UNITY_DOTSRUNTIME
            kProfileDeletedParents.End();
#endif

#if !UNITY_DOTSRUNTIME
            kProfileRemoveParents.Begin();
#endif
            UpdateRemoveParents(ref state);
#if !UNITY_DOTSRUNTIME
            kProfileRemoveParents.End();
#endif

#if !UNITY_DOTSRUNTIME
            kProfileNewParents.Begin();
#endif
            UpdateNewParents(ref state);
#if !UNITY_DOTSRUNTIME
            kProfileNewParents.End();
#endif

#if !UNITY_DOTSRUNTIME
            kProfileChangeParents.Begin();
#endif
            UpdateChangeParents(ref state);
#if !UNITY_DOTSRUNTIME
            kProfileChangeParents.End();
#endif
        }
    }
}
