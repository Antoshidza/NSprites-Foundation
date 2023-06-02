using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace NSprites
{
    [BurstCompile]
    public static class MathHelper
    {
        public static float2 rotate(float angle, float2 position)
        {
            var sin = math.sin(angle);
            var cos = math.cos(angle);
            
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
            var value = new float4 {
                z = math.sin(angle / 2),
                w = math.cos(angle / 2)
            };

            return new quaternion(value);
        }


        public static float3 euler(quaternion quaternion)
        {
            var q = quaternion.value;
            
            // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            
            // roll (x-axis rotation)
            var sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
            var cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
        
            // pitch (y-axis rotation)
            var sinp = math.sqrt(1 + 2 * (q.w * q.y - q.x * q.z));
            var cosp = math.sqrt(1 - 2 * (q.w * q.y - q.x * q.z));

            // yaw (z-axis rotation)
            var siny_cosp = 2 * (q.w * q.z + q.x * q.y);
            var cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
                
            return new
            (
                        math.atan2(sinr_cosp, cosr_cosp),
                        2 * math.atan2(sinp, cosp) - math.PI / 2,
                        math.atan2(siny_cosp, cosy_cosp)
            );
        }


        public static float eulerZ(quaternion quaternion)
        {
            var q = quaternion.value;
            
            var siny_cosp = 2 * (q.w * q.z + q.x * q.y);
            var cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
            return math.atan2(siny_cosp, cosy_cosp);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 float3(float2 vector) => math.float3(vector.x, vector.y, 0f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 float2(float3 vector) => vector.xy;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 transform(float4x4 a, float2 b) => math.transform(a, float3(b)).xy;
    }
}