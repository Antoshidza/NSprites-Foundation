using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

// ReSharper disable InconsistentNaming



namespace NSprites
{
    [BurstCompile]
    public static class math2D
    {
        public static float2 rotate(float angle, float2 position)
        {
            float sin = math.sin(angle);
            float cos = math.cos(angle);
            
            return new float2(
                position.x * cos - position.y * sin,
                position.x * sin + position.y * cos
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 rotate(quaternion quaternion, float2 position)
        {
            return math.rotate(quaternion, float3(position)).xy;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion quaternion(float angle)
        {
            float4 value = new (){
                z = math.sin(angle / 2),
                w = math.cos(angle / 2)
            };

            return new quaternion(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 float3(float2 vector) => math.float3(vector.x, vector.y, 0f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 float2(float3 vector) => vector.xy;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 transform(float4x4 a, float2 b) => math.transform(a, float3(b)).xy;
    }
}