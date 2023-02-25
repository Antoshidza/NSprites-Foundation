using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Burst.Intrinsics;

namespace NSprites
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct LocalToParent2DSystem : ISystem
    {
        private struct SystemData : IComponentData
        {
            public EntityQuery RootQuery;
        }
        
        [BurstCompile]
        private struct UpdateHierarchy : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<WorldPosition2D> worldPosition_CTH;
            [NativeDisableContainerSafetyRestriction] public ComponentLookup<WorldPosition2D> worldPosition_CL;
            [ReadOnly] public ComponentLookup<LocalPosition2D> localPosition_CL;
            [ReadOnly] public BufferTypeHandle<Child2D> child_BTH;
            [ReadOnly] public BufferLookup<Child2D> child_BL;
            public uint lastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                //if position or child set was changed then we need update children hierarchically
                var needUpdate = chunk.DidChange(ref worldPosition_CTH, lastSystemVersion) && chunk.DidChange(ref child_BTH, lastSystemVersion);

                var chunkWorldPosition = chunk.GetNativeArray(ref worldPosition_CTH);
                var chunkChild = chunk.GetBufferAccessor(ref child_BTH);

                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {
                    var worldPosition = chunkWorldPosition[entityIndex];
                    var children = chunkChild[entityIndex];

                    for (int childIndex = 0; childIndex < children.Length; childIndex++)
                        UpdateChild(worldPosition.value, children[childIndex].value, needUpdate);
                }
            }

            private void UpdateChild(in float2 parentPosition, in Entity childEntity, bool needUpdate)
            {
                var position = parentPosition + localPosition_CL[childEntity].value;
                worldPosition_CL[childEntity] = new WorldPosition2D { value = position };

                //if this child also is a parent update its children
                if (!child_BL.HasBuffer(childEntity))
                    return;

                needUpdate = needUpdate || localPosition_CL.DidChange(childEntity, lastSystemVersion) || child_BL.DidChange(childEntity, lastSystemVersion);
                var children = child_BL[childEntity];

                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                    UpdateChild(position, children[childIndex].value, needUpdate);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
            queryBuilder
                .WithAll<WorldPosition2D>()
                .WithAll<Child2D>()
                .WithNone<Parent2D>();
            
            state.EntityManager.AddComponentData(state.SystemHandle, new SystemData { RootQuery = state.GetEntityQuery(queryBuilder) });
            
            queryBuilder.Dispose();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var systemData = SystemAPI.GetComponent<SystemData>(state.SystemHandle);
            
            state.Dependency = new UpdateHierarchy
            {
                worldPosition_CL = SystemAPI.GetComponentLookup<WorldPosition2D>(false),
                localPosition_CL = SystemAPI.GetComponentLookup<LocalPosition2D>(true),
                child_BL = SystemAPI.GetBufferLookup<Child2D>(true),
                child_BTH = SystemAPI.GetBufferTypeHandle<Child2D>(true),
                worldPosition_CTH = SystemAPI.GetComponentTypeHandle<WorldPosition2D>(true),
                lastSystemVersion = state.LastSystemVersion
            }.ScheduleParallel(systemData.RootQuery, state.Dependency);
        }
    }
}
