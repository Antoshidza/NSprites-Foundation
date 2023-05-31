using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;
using float2 = Unity.Mathematics.float2;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;
using static NSprites.math2D;
using static Unity.Mathematics.math;



namespace NSprites
{
    [BurstCompile]
    public struct LocalTransform2D : IComponentData
    {
        /// <summary>
        /// The position of this transform.
        /// </summary>
        [CreateProperty]
        public float2 Position;

        /// <summary>
        /// The uniform scale of this transform.
        /// </summary>
        [CreateProperty]
        public float2 Scale;

        /// <summary>
        /// The rotation of this transform.
        /// </summary>
        [CreateProperty]
        public quaternion Rotation;

        public float3 Position3D => float3(Position.x, Position.y, 0);
        
        /// <summary>
        /// The identity transform.
        /// </summary>
        public static readonly LocalTransform2D Identity = new LocalTransform2D { Scale = 1.0f, Rotation = quaternion.identity };
        
        /// <summary>
        /// Returns the Transform equivalent of a float4x4 matrix.
        /// </summary>
        /// <param name="matrix">The orthogonal matrix to convert.</param>
        /// <remarks>
        /// If the input matrix contains non-uniform scale, the largest value will be used.
        /// Any shear in the input matrix will be ignored.
        /// </remarks>
        /// <seealso cref="FromMatrixSafe"/>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromMatrix(float4x4 matrix)
        {
            float2 position = matrix.c3.xy;
            float scaleX = length(matrix.c0.xy);
            float scaleY = length(matrix.c1.xy);

            float scale = max(scaleX, scaleY);

            float3x3 normalizedRotationMatrix = orthonormalize(float3x3(matrix));
            quaternion rotation = new quaternion(normalizedRotationMatrix);
            
            return new LocalTransform2D {Position = position, Scale = scale, Rotation = rotation};
        }

        /// <summary>
        /// Returns the Transform equivalent of a float4x4 matrix. Throws and exception if the matrix contains
        /// nonuniform scale or shear.
        /// </summary>
        /// <param name="matrix">The orthogonal matrix to convert.</param>
        /// <remarks>
        /// If the input matrix contains non-uniform scale, this will throw an exception.
        /// If the input matrix contains shear, this will throw an exception.
        /// </remarks>
        /// <seealso cref="FromMatrix"/>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromMatrixSafe(float4x4 matrix)
        {
            const float TOLERANCE = .001f;
            const float TOLERANCESQ = TOLERANCE * TOLERANCE;

            // Test for uniform scale
            float scaleX = lengthsq(matrix.c0.xy);
            float scaleY = lengthsq(matrix.c1.xy);

            if (abs(scaleX - scaleY) > TOLERANCESQ)
                throw new ArgumentException("Trying to convert a float4x4 to a LocalTransform, but the scale is not uniform");

            float3x3 matrix3X3 = new float3x3(matrix);
            float3x3 transpose3X3 = transpose(matrix3X3);
            float3x3 combined3X3 = mul(matrix3X3, transpose3X3);

            // If the matrix is orthogonal, the combined result should be identity
            if (lengthsq(combined3X3.c0 - right()) > TOLERANCESQ ||
                lengthsq(combined3X3.c1 - up()) > TOLERANCESQ ||
                lengthsq(combined3X3.c2 - right()) > TOLERANCESQ)
            {
                throw new ArgumentException("Trying to convert a float4x4 to a LocalTransform, but the rotation 3x3 is not orthogonal");
            }

            float3x3 normalizedRotationMatrix = orthonormalize(new float3x3(matrix));
            quaternion rotation = new quaternion(normalizedRotationMatrix);

            float2 position = matrix.c3.xy;

            LocalTransform2D transform = new LocalTransform2D {Position = position, Scale = scaleX, Rotation = rotation};

            return transform;
        }


        /// <summary>
        /// Returns a Transform initialized with the given position and rotation. Scale will be 1.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromPositionRotation(float2 position, quaternion rotation) => new() {Position = position, Scale = 1f, Rotation = rotation};

        /// <summary>
        /// Returns a Transform initialized with the given position, rotation and scale.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromPositionRotationScale(float2 position, quaternion rotation, float2 scale) => new() {Position = position, Scale = scale, Rotation = rotation};

        /// <summary>
        /// Returns a Transform initialized with the given position. Rotation will be identity, and scale will be 1.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromPosition(float2 position) => new() {Position = position, Scale = 1f, Rotation = quaternion.identity};

        /// <summary>
        /// Returns a Transform initialized with the given position. Rotation will be identity, and scale will be 1.
        /// </summary>
        /// <param name="x">The x coordinate of the position.</param>
        /// <param name="y">The y coordinate of the position.</param>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromPosition(float x, float y) => new() {Position = new float2(x,y), Scale = 1f, Rotation = quaternion.identity};

        /// <summary>
        /// Returns a Transform initialized with the given rotation. Position will be 0,0,0, and scale will be 1.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromRotation(quaternion rotation) => new() {Position = float2.zero, Scale = 1f, Rotation = rotation};

        /// <summary>
        /// Returns a Transform initialized with the given scale. Position will be 0,0,0, and rotation will be identity.
        /// </summary>
        /// <param name="scale">The scale.</param>
        /// <returns>The Transform.</returns>
        public static LocalTransform2D FromScale(float2 scale) => new() {Position = float2.zero, Scale = scale, Rotation = quaternion.identity};

        /// <summary>
        /// Convert transformation data to a human-readable string
        /// </summary>
        /// <returns>The transform value as a human-readable string</returns>
        public override string ToString()
        {
            return $"Position={Position.ToString()} Rotation={Rotation.ToString()} Scale={Scale.ToString()}";
        }

        /// <summary>
        /// Gets the right vector of unit length.
        /// </summary>
        /// <returns>The right vector.</returns>
        public float2 Right() => TransformDirection(float2(1,0));

        /// <summary>
        /// Gets the up vector of unit length.
        /// </summary>
        /// <returns>The up vector.</returns>
        public float2 Up() => TransformDirection(float2(0,1));

        /// <summary>
        /// Transforms a point by this transform.
        /// </summary>
        /// <param name="point">The point to be transformed.</param>
        /// <returns>The point after transformation.</returns>
        public float2 TransformPoint(float2 point) => Position + rotate(Rotation, point) * Scale;

        /// <summary>
        /// Transforms a point by the inverse of this transform.
        /// </summary>
        /// <remarks>
        /// Throws if the <see cref="Scale"/> field is zero.
        /// </remarks>
        /// <param name="point">The point to be transformed.</param>
        /// <returns>The point after transformation.</returns>
        public float2 InverseTransformPoint(float2 point) => rotate(conjugate(Rotation), point - Position) / Scale;

        /// <summary>
        /// Transforms a direction by this transform.
        /// </summary>
        /// <param name="direction">The direction to be transformed.</param>
        /// <returns>The direction after transformation.</returns>
        public float2 TransformDirection(float2 direction) => rotate(Rotation, direction);

        /// <summary>
        /// Transforms a direction by the inverse of this transform.
        /// </summary>
        /// <param name="direction">The direction to be transformed.</param>
        /// <returns>The direction after transformation.</returns>
        public float2 InverseTransformDirection(float2 direction) => rotate(conjugate(Rotation), direction);

        /// <summary>
        /// Transforms a rotation by this transform.
        /// </summary>
        /// <param name="rotation">The rotation to be transformed.</param>
        /// <returns>The rotation after transformation.</returns>
        public quaternion TransformRotation(quaternion rotation) => mul(Rotation, rotation);

        /// <summary>
        /// Transforms a rotation by the inverse of this transform.
        /// </summary>
        /// <param name="rotation">The rotation to be transformed.</param>
        /// <returns>The rotation after transformation.</returns>
        public quaternion InverseTransformRotation(quaternion rotation) => mul(conjugate(Rotation), rotation);

        /// <summary>
        /// Transforms a scale by this transform.
        /// </summary>
        /// <param name="scale">The scale to be transformed.</param>
        /// <returns>The scale after transformation.</returns>
        public float2 TransformScale(float2 scale) => scale * this.Scale;

        /// <summary>
        /// Transforms a scale by the inverse of this transform.
        /// </summary>
        /// <remarks>
        /// Throws if the <see cref="Scale"/> field is zero.
        /// </remarks>
        /// <param name="scale">The scale to be transformed.</param>
        /// <returns>The scale after transformation.</returns>
        public float2 InverseTransformScale(float2 scale) => scale / this.Scale;

        /// <summary>
        /// Transforms a Transform by this transform.
        /// </summary>
        /// <param name="transformData">The Transform to be transformed.</param>
        /// <returns>The Transform after transformation.</returns>
        public LocalTransform2D TransformTransform(in LocalTransform2D transformData) => new()
        {
            Position = TransformPoint(transformData.Position),
            Scale = TransformScale(transformData.Scale),
            Rotation = TransformRotation(transformData.Rotation),
        };

        /// <summary>
        /// Transforms a <see cref="LocalTransform"/> by the inverse of this transform.
        /// </summary>
        /// <param name="transformData">The <see cref="LocalTransform"/> to be transformed.</param>
        /// <returns>The <see cref="LocalTransform"/> after transformation.</returns>
        public LocalTransform2D InverseTransformTransform(in LocalTransform2D transformData) => new()
        {
            Position = InverseTransformPoint(transformData.Position),
            Scale = InverseTransformScale(transformData.Scale),
            Rotation = InverseTransformRotation(transformData.Rotation),
        };

        /// <summary>
        /// Gets the inverse of this transform.
        /// </summary>
        /// <remarks>
        /// This method will throw if the <see cref="Scale"/> field is zero.
        /// </remarks>
        /// <returns>The inverse of the transform.</returns>
        public LocalTransform2D Inverse()
        {
            quaternion inverseRotation = conjugate(Rotation);
            float2 inverseScale = 1.0f / Scale;
            return new()
            {
                Position = -rotate(0f, Position) * inverseScale,
                Scale = inverseScale,
                Rotation = inverseRotation,
            };
        }

        /// <summary>
        /// Gets the float4x4 equivalent of this transform.
        /// </summary>
        /// <returns>The float4x4 matrix.</returns>
        public float4x4 ToMatrix() => float4x4.TRS(float3(Position), Rotation, float3(Scale));

        /// <summary>
        /// Gets the float4x4 equivalent of the inverse of this transform.
        /// </summary>
        /// <returns>The inverse float4x4 matrix.</returns>
        public float4x4 ToInverseMatrix() => Inverse().ToMatrix();

        /// <summary>
        /// Gets an identical transform with a new position value.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The transform.</returns>
        public LocalTransform2D WithPosition(float2 position) => new() { Position = position, Scale = Scale, Rotation = Rotation };

        /// <summary>
        /// Creates a transform that is identical but with a new position value.
        /// </summary>
        /// <param name="x">The x coordinate of the new position.</param>
        /// <param name="y">The y coordinate of the new position.</param>
        /// <param name="z">The z coordinate of the new position.</param>
        /// <returns>The new transform.</returns>
        public LocalTransform2D WithPosition(float x, float y) => new() { Position = new float2(x,y), Scale = Scale, Rotation = Rotation };

        /// <summary>
        /// Gets an identical transform with a new rotation value.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        /// <returns>The transform.</returns>
        public LocalTransform2D WithRotation(quaternion rotation) => new() { Position = Position, Scale = Scale, Rotation = rotation };

        /// <summary>
        /// Gets an identical transform with a new scale value.
        /// </summary>
        /// <param name="scale">The scale.</param>
        /// <returns>The T.</returns>
        public LocalTransform2D WithScale(float scale) => new() { Position = Position, Scale = scale, Rotation = Rotation };

        /// <summary>
        /// Translates this transform by the specified vector.
        /// </summary>
        /// <remarks>
        /// Note that this doesn't modify the original transform. Rather it returns a new one.
        /// </remarks>
        /// <param name="translation">The translation vector.</param>
        /// <returns>A new, translated Transform.</returns>
        public LocalTransform2D Translate(float2 translation) => new() { Position = Position + translation, Scale = Scale, Rotation = Rotation};

        /// <summary>
        /// Scales this transform by the specified factor.
        /// </summary>
        /// <remarks>
        /// Note that this doesn't modify the original transform. Rather it returns a new one.
        /// </remarks>
        /// <param name="scale">The scaling factor.</param>
        /// <returns>A new, scaled Transform.</returns>
        public LocalTransform2D ApplyScale(float scale) => new() { Position = Position, Scale = this.Scale * scale, Rotation = Rotation};

        /// <summary>
        /// Rotates this Transform by the specified quaternion.
        /// </summary>
        /// <remarks>
        /// Note that this doesn't modify the original transform. Rather it returns a new one.
        /// </remarks>
        /// <param name="rotation">The rotation quaternion of unit length.</param>
        /// <returns>A new, rotated Transform.</returns>
        public LocalTransform2D Rotate(quaternion rotation) => new() { Position = Position, Scale = Scale, Rotation = mul(Rotation, rotation)};

        /// <summary>
        /// Rotates this Transform around the X axis.
        /// </summary>
        /// <remarks>
        /// Note that this doesn't modify the original transform. Rather it returns a new one.
        /// </remarks>
        /// <param name="angle">The X rotation.</param>
        /// <returns>A new, rotated Transform.</returns>
        public LocalTransform2D RotateX(float angle) => Rotate(quaternion.RotateX(angle));

        /// <summary>
        /// Rotates this Transform around the Y axis.
        /// </summary>
        /// <remarks>
        /// Note that this doesn't modify the original transform. Rather it returns a new one.
        /// </remarks>
        /// <param name="angle">The Y rotation.</param>
        /// <returns>A new, rotated Transform.</returns>
        public LocalTransform2D RotateY(float angle) => Rotate(quaternion.RotateY(angle));

        /// <summary>
        /// Rotates this Transform around the Z axis.
        /// </summary>
        /// <remarks>
        /// Note that this doesn't modify the original transform. Rather it returns a new one.
        /// </remarks>
        /// <param name="angle">The Z rotation.</param>
        /// <returns>A new, rotated Transform.</returns>
        public LocalTransform2D RotateZ(float angle) => Rotate(quaternion.RotateZ(angle));

        /// <summary>Checks if a transform has equal position, rotation, and scale to another.</summary>
        /// <param name="other">The TransformData to compare.</param>
        /// <returns>Returns true if the position, rotation, and scale are equal.</returns>
        public bool Equals(in LocalTransform2D other)
        {
            return Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);
        }
    }
}
