using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NSprites
{
    // [UpdateAfter(typeof(UpdateCullingDataSystem))] // uncomment this if you use UpdateCullingDataSystem
    public partial struct FullScreenSpriteSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(FullScreenSpriteTag))]
        private partial struct RecalculateSpritesJob : IJobEntity
        {
            public float2 CameraPosition;
            public float2 ScreenSize;
            
            private void Execute(ref Scale2D scale, ref LocalTransform transform, ref UVTilingAndOffset uvTilingAndOffset, in NativeSpriteSize nativeSpriteSize)
            {
                scale.value = ScreenSize;
                transform.Scale = 1;
                transform.Position = new float3(CameraPosition.x, CameraPosition.y, 0f);
                uvTilingAndOffset.value = new float4(ScreenSize / nativeSpriteSize.Value, CameraPosition / nativeSpriteSize.Value - ScreenSize / nativeSpriteSize.Value / 2f);
            }
        }
        
        private struct SystemData : IComponentData
        {
            public float2 LastCameraPosition;
            public Bounds2D LastCameraBounds;
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _ = state.EntityManager.AddComponent<SystemData>(state.SystemHandle);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(!SystemAPI.TryGetSingleton<SpriteFrustumCullingSystem.CameraData>(out var cameraData))
                return;
            
            var sysData = SystemAPI.GetComponentRW<SystemData>(state.SystemHandle);

            if(cameraData.CullingBounds2D != sysData.ValueRO.LastCameraBounds)
            {
                sysData.ValueRW.LastCameraBounds = cameraData.CullingBounds2D;
                
                var recalculateSpriteJob = new RecalculateSpritesJob
                {
                    CameraPosition = cameraData.Position,
                    ScreenSize = cameraData.CullingBounds2D.Size
                };
                state.Dependency = recalculateSpriteJob.ScheduleByRef(state.Dependency);
            }
        }
    }
}