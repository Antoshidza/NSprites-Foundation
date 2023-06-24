using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace NSprites
{
    /// <summary>
    /// This system computes a <see cref="LocalToWorld2D"/> matrix for each entity
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(ParentSystem2D))]
    public partial struct LocalToWorldSystem2D : ISystem
    {
        // Compute the LocalToWorld of all root-level entities
        [BurstCompile]
        private unsafe struct ComputeRootLocalToWorldJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LocalTransform2D> LocalTransformTypeHandleRO;
            [ReadOnly] public ComponentTypeHandle<PostTransformMatrix2D> PostTransformMatrixTypeHandleRO;
            public ComponentTypeHandle<LocalToWorld2D> LocalToWorldTypeHandleRW;
            public uint LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Assert.IsFalse(useEnabledMask);

                LocalTransform2D* chunkLocalTransforms = (LocalTransform2D*)chunk.GetRequiredComponentDataPtrRO(ref LocalTransformTypeHandleRO);
                if (chunk.DidChange(ref LocalTransformTypeHandleRO, LastSystemVersion) ||
                    chunk.DidChange(ref PostTransformMatrixTypeHandleRO, LastSystemVersion))
                {
                    LocalToWorld2D* chunkLocalToWorlds = (LocalToWorld2D*)chunk.GetRequiredComponentDataPtrRW(ref LocalToWorldTypeHandleRW);
                    PostTransformMatrix2D* chunkPostTransformMatrices = (PostTransformMatrix2D*)chunk.GetComponentDataPtrRO(ref PostTransformMatrixTypeHandleRO);
                    if (chunkPostTransformMatrices != null)
                    {
                        for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; ++i) {
                            chunkLocalToWorlds[i].Value = mul(
                                chunkLocalTransforms[i].ToMatrix(),
                                chunkPostTransformMatrices[i].Value
                            );
                        }
                    }
                    else
                    {
                        for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; ++i)
                        {
                            chunkLocalToWorlds[i].Value = chunkLocalTransforms[i].ToMatrix();
                        }
                    }
                }
            }
        }

        [BurstCompile]
        private unsafe struct ComputeChildLocalToWorldJob : IJobChunk
        {
            [ReadOnly] public EntityQueryMask LocalToWorldWriteGroupMask;

            [ReadOnly] public BufferTypeHandle<Child2D> ChildTypeHandleRO;
            [ReadOnly] public BufferLookup<Child2D> ChildLookupRO;
            public ComponentTypeHandle<LocalToWorld2D> LocalToWorldTypeHandleRW;

            [ReadOnly] public ComponentLookup<LocalTransform2D> LocalTransformLookupRO;
            [ReadOnly] public ComponentLookup<PostTransformMatrix2D> PostTransformMatrixLookupRO;
            [NativeDisableContainerSafetyRestriction] public ComponentLookup<LocalToWorld2D> LocalToWorldLookupRW;
            public uint LastSystemVersion;


            private void ChildLocalToWorldFromTransformMatrix(in float4x4 parentLocalToWorld, Entity childEntity, bool updateChildrenTransform)
            {
                updateChildrenTransform = updateChildrenTransform
                                          || PostTransformMatrixLookupRO.DidChange(childEntity, LastSystemVersion)
                                          || LocalTransformLookupRO.DidChange(childEntity, LastSystemVersion);

                float4x4 localToWorld;

                if (updateChildrenTransform && LocalToWorldWriteGroupMask.MatchesIgnoreFilter(childEntity))
                {
                    LocalTransform2D localTransform = LocalTransformLookupRO[childEntity];
                    localToWorld = mul(parentLocalToWorld, localTransform.ToMatrix());
                    if (PostTransformMatrixLookupRO.HasComponent(childEntity))
                    {
                        localToWorld = mul(localToWorld, PostTransformMatrixLookupRO[childEntity].Value);
                    }
                    LocalToWorldLookupRW[childEntity] = new LocalToWorld2D{Value = localToWorld};
                }
                else
                {
                    localToWorld = LocalToWorldLookupRW[childEntity].Value;
                    updateChildrenTransform = LocalToWorldLookupRW.DidChange(childEntity, LastSystemVersion);
                }

                if (ChildLookupRO.TryGetBuffer(childEntity, out DynamicBuffer<Child2D> children))
                {
                    for (int i = 0, childCount = children.Length; i < childCount; i++)
                    {
                        ChildLocalToWorldFromTransformMatrix(localToWorld, children[i].Value, updateChildrenTransform);
                    }
                }
            }

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Assert.IsFalse(useEnabledMask);

                bool updateChildrenTransform = chunk.DidChange(ref ChildTypeHandleRO, LastSystemVersion);
                BufferAccessor<Child2D> chunkChildBuffers = chunk.GetBufferAccessor(ref ChildTypeHandleRO);
                updateChildrenTransform = updateChildrenTransform || chunk.DidChange(ref LocalToWorldTypeHandleRW, LastSystemVersion);
                LocalToWorld2D* chunkLocalToWorlds = (LocalToWorld2D*)chunk.GetRequiredComponentDataPtrRO(ref LocalToWorldTypeHandleRW);
                for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++)
                {
                    float4x4             localToWorld = chunkLocalToWorlds[i].Value;
                    DynamicBuffer<Child2D> children     = chunkChildBuffers[i];
                    for (int j = 0, childCount = children.Length; j < childCount; j++)
                    {
                        ChildLocalToWorldFromTransformMatrix(localToWorld, children[j].Value, updateChildrenTransform);
                    }
                }
            }
        }



        private EntityQuery     rootsQuery;
        private EntityQuery     parentsQuery;
        private EntityQueryMask localToWorldWriteGroupMask;

        private ComponentTypeHandle<LocalTransform2D>      localTransformTypeHandleRO;
        private ComponentTypeHandle<PostTransformMatrix2D> postTransformMatrixTypeHandleRO;
        private ComponentTypeHandle<LocalTransform2D>      localTransformTypeHandleRW;
        private ComponentTypeHandle<LocalToWorld2D>        localToWorldTypeHandleRW;

        private BufferTypeHandle<Child2D> childTypeHandleRO;
        private BufferLookup<Child2D>     childLookupRO;

        private ComponentLookup<LocalTransform2D>      localTransformLookupRO;
        private ComponentLookup<PostTransformMatrix2D> postTransformMatrixLookupRO;
        private ComponentLookup<LocalToWorld2D>        localToWorldLookupRW;

        /// <inheritdoc cref="ISystem.OnCreate"/>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp)
                                        .WithAll<LocalTransform2D>()
                                        .WithAllRW<LocalToWorld2D>()
                                        .WithNone<Parent2D>()
                                        .WithOptions(EntityQueryOptions.FilterWriteGroup);
            rootsQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform2D, Child2D>()
                .WithAllRW<LocalToWorld2D>()
                .WithNone<Parent2D>();
            parentsQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform2D, Parent2D>()
                .WithAllRW<LocalToWorld2D>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup);
            localToWorldWriteGroupMask = state.GetEntityQuery(builder).GetEntityQueryMask();

            localTransformTypeHandleRO = state.GetComponentTypeHandle<LocalTransform2D>(true);
            postTransformMatrixTypeHandleRO = state.GetComponentTypeHandle<PostTransformMatrix2D>(true);
            localTransformTypeHandleRW = state.GetComponentTypeHandle<LocalTransform2D>(false);
            localToWorldTypeHandleRW = state.GetComponentTypeHandle<LocalToWorld2D>(false);

            childTypeHandleRO = state.GetBufferTypeHandle<Child2D>(true);
            childLookupRO = state.GetBufferLookup<Child2D>(true);

            localTransformLookupRO = state.GetComponentLookup<LocalTransform2D>(true);
            postTransformMatrixLookupRO = state.GetComponentLookup<PostTransformMatrix2D>(true);
            localToWorldLookupRW = state.GetComponentLookup<LocalToWorld2D>(false);
        }

        /// <inheritdoc cref="ISystem.OnDestroy"/>
        /// <inheritdoc cref="ISystem.OnUpdate"/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localTransformTypeHandleRO.Update(ref state);
            postTransformMatrixTypeHandleRO.Update(ref state);
            localTransformTypeHandleRW.Update(ref state);
            localToWorldTypeHandleRW.Update(ref state);

            childTypeHandleRO.Update(ref state);
            childLookupRO.Update(ref state);

            localTransformLookupRO.Update(ref state);
            postTransformMatrixLookupRO.Update(ref state);
            localToWorldLookupRW.Update(ref state);

            // Compute LocalToWorld for all root-level entities
            ComputeRootLocalToWorldJob rootJob = new ComputeRootLocalToWorldJob
            {
                LocalTransformTypeHandleRO = localTransformTypeHandleRO,
                PostTransformMatrixTypeHandleRO = postTransformMatrixTypeHandleRO,
                LocalToWorldTypeHandleRW = localToWorldTypeHandleRW,
                LastSystemVersion = state.LastSystemVersion,
            };
            state.Dependency = rootJob.ScheduleParallelByRef(rootsQuery, state.Dependency);

            // Compute LocalToWorld for all child entities
            ComputeChildLocalToWorldJob childJob = new ComputeChildLocalToWorldJob
            {
                LocalToWorldWriteGroupMask = localToWorldWriteGroupMask,
                ChildTypeHandleRO = childTypeHandleRO,
                ChildLookupRO = childLookupRO,
                LocalToWorldTypeHandleRW = localToWorldTypeHandleRW,
                LocalTransformLookupRO = localTransformLookupRO,
                PostTransformMatrixLookupRO = postTransformMatrixLookupRO,
                LocalToWorldLookupRW = localToWorldLookupRW,
                LastSystemVersion = state.LastSystemVersion,
            };
            state.Dependency = childJob.ScheduleParallelByRef(parentsQuery, state.Dependency);
        }
    }
}
