using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace NSprites
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(SpriteFrustumCullingSystem))]
    public partial struct UpdateCullingDataSystem : ISystem
    {
        private class SystemData : IComponentData
        {
            private Camera _camera;

            public Camera Camera
            {
                get
                {
                    if(_camera == null)
                        _camera = Camera.main;
                    return _camera;
                }
            }
        }

        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.AddComponentObject(state.SystemHandle, new SystemData());
        }

        public void OnUpdate(ref SystemState state)
        {
            var camera = state.EntityManager.GetComponentObject<SystemData>(state.SystemHandle).Camera;
            var cameraPos = camera.transform.position;
            var leftBottomPoint = camera.ScreenToWorldPoint(new Vector3(0f, 0f, 0f));
            var rightUpPoint = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
            var cameraViewBounds2D = new Bounds2D(new float2x2(new float2(leftBottomPoint.x, leftBottomPoint.y), new float2(rightUpPoint.x, rightUpPoint.y)));
            SystemAPI.SetSingleton(new SpriteFrustumCullingSystem.CameraData
            {
                Position = new float2(cameraPos.x, cameraPos.y),
                CullingBounds2D = cameraViewBounds2D
            });
        }
    }
}