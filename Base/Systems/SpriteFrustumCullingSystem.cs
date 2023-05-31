using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;

namespace NSprites
{
    [BurstCompile]
    public partial struct SpriteFrustumCullingSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(SpriteRenderID))]
        [WithNone(typeof(CullSpriteTag))]
        private partial struct DisableCulledJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            public Bounds2D CameraBounds2D;

            private void Execute(Entity entity, [ChunkIndexInQuery]int chunkIndex, in LocalToWorld2D worldPosition, in Scale2D size, in Pivot pivot)
            {
                var bounds = Bounds2D.From(worldPosition, size, pivot);
                if(!CameraBounds2D.Intersects(bounds))
                    EntityCommandBuffer.AddComponent<CullSpriteTag>(chunkIndex, entity);
            }
        }
        [BurstCompile]
        [WithAll(typeof(SpriteRenderID))]
        [WithAll(typeof(CullSpriteTag))]
        private partial struct EnableUnculledJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            public Bounds2D CameraBounds2D;

            private void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, in LocalToWorld2D worldPosition, in Scale2D size, in Pivot pivot)
            {
                var bounds = Bounds2D.From(worldPosition, size, pivot);
                if (CameraBounds2D.Intersects(bounds))
                    EntityCommandBuffer.RemoveComponent<CullSpriteTag>(chunkIndex, entity);
            }
        }
        public struct CameraData : IComponentData
        {
            public float2 Position;
            public Bounds2D CullingBounds2D;
        }

#if UNITY_EDITOR
        [MenuItem("NSprites/Toggle frustum culling system")]
        public static void ToggleFrustumCullingSystem()
        {
            var systemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<SpriteFrustumCullingSystem>();

            if (systemHandle == SystemHandle.Null)
                return;

            ref var systemState = ref World.DefaultGameObjectInjectionWorld.Unmanaged.ResolveSystemStateRef(systemHandle);

            systemState.Enabled = !systemState.Enabled;

            if (!systemState.Enabled)
                systemState.EntityManager.RemoveComponent(systemState.GetEntityQuery(typeof(CullSpriteTag)), ComponentType.ReadOnly<CullSpriteTag>());
        }
#endif

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _ = state.EntityManager.AddComponentData(state.SystemHandle, new CameraData());
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var cullingBounds2D = SystemAPI.GetComponent<CameraData>(state.SystemHandle).CullingBounds2D;

            var disableCulledJob = new DisableCulledJob
            {
                EntityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                CameraBounds2D = cullingBounds2D
            };
            var disableCulledHandle = disableCulledJob.ScheduleParallelByRef(state.Dependency);
            

            var enableUnculledJob = new EnableUnculledJob
            {
                EntityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                CameraBounds2D = cullingBounds2D
            };
            var enableUnculledHandle = enableUnculledJob.ScheduleParallelByRef(state.Dependency);
            
            state.Dependency = JobHandle.CombineDependencies(disableCulledHandle, enableUnculledHandle);
        }
    }
}
