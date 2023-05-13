using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace NSprites
{
    [UpdateAfter(typeof(UpdateCullingDataSystem))]
    public partial struct FullScreenSpriteSystem : ISystem
    {
        [BurstCompile]
        [WithAll(typeof(FullScreenSpriteTag))]
        private partial struct RecalculateSpritesJob : IJobEntity
        {
            public float2 CameraPosition;
            public float2 ScreenSize;
            
            private void Execute(ref Scale2D size, ref WorldPosition2D position, ref UVTilingAndOffset uvTilingAndOffset, in NativeSpriteSize nativeSpriteSize)
            {
                size.value = ScreenSize;
                position.value = CameraPosition;
                uvTilingAndOffset.value = new float4(size.value / nativeSpriteSize.Value, CameraPosition / nativeSpriteSize.Value - size.value / nativeSpriteSize.Value / 2f);
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