using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Steering
{
	public struct TransformUtil
	{
#pragma warning disable SA1132 // Multiple fields on one libe to easily one summary to all.
		/// <summary>
		/// The value of row r# and column c# of the 3x3 transformation matrix. Note that we don't store row 3, because it is always 0 0 1.
		/// </summary>
		private float r1c1, r1c2, r1c3, r2c1, r2c2, r2c3;
#pragma warning restore SA1132

		/// <summary>
		/// Initializes a new instance of the <see cref="TransformUtil"/> struct.
		/// </summary>
		/// <param name="r1c1">The value of the cell in the row 1, column 1 of the transformation matrix.</param>
		/// <param name="r1c2">The value of the cell in the row 1, column 2 of the transformation matrix.</param>
		/// <param name="r1c3">The value of the cell in the row 1, column 3 of the transformation matrix.</param>
		/// <param name="r2c1">The value of the cell in the row 2, column 1 of the transformation matrix.</param>
		/// <param name="r2c2">The value of the cell in the row 2, column 2 of the transformation matrix.</param>
		/// <param name="r2c3">The value of the cell in the row 2, column 3 of the transformation matrix.</param>
		private TransformUtil( float r1c1, float r1c2, float r1c3, float r2c1, float r2c2, float r2c3 )
		{
			this.r1c1 = r1c1;
			this.r1c2 = r1c2;
			this.r1c3 = r1c3;
			this.r2c1 = r2c1;
			this.r2c2 = r2c2;
			this.r2c3 = r2c3;
		}

		/// <summary>
		/// Returns a <see cref="TransformUtil"/> that is the identity - leaves input vectors unchanged.
		/// </summary>
		/// <returns>A <see cref="TransformUtil"/> that is the identity transformation - leaves input vectors unchanged.</returns>
		public static TransformUtil Identity()
		{
#pragma warning disable SA1117 // Parameters lined up like this to echo matrix structure
			return new TransformUtil(
				1, 0, 0,
				0, 1, 0 );
#pragma warning restore SA1117
		}

		/// <summary>
		/// Returns a <see cref="TransformUtil"/> that translates input by the given x and y offsets.
		/// </summary>
		/// <param name="x">The amount to translate input vectors in the x axis.</param>
		/// <param name="y">The amount to translate input vectors in the y axis.</param>
		/// <returns>A <see cref="TransformUtil"/> that translates input by the given x and y offsets.</returns>
		public static TransformUtil Translation( float x, float y )
		{
#pragma warning disable SA1117 // Parameters lined up like this to echo matrix structure
			return new TransformUtil(
				1, 0, x,
				0, 1, y );
#pragma warning restore SA1117
		}

		/// <summary>
		/// Returns a <see cref="TransformUtil"/> that scales input (from the origin) by the given x and y scales.
		/// </summary>
		/// <param name="xScale">The factor to scale by in the x-axis.</param>
		/// <param name="yScale">The factor to scale by in the y-axis.</param>
		/// <returns>A <see cref="TransformUtil"/> that scales input (from the origin) by the given x and y scales.</returns>
		public static TransformUtil Scaling( float xScale, float yScale )
		{
#pragma warning disable SA1117 // Parameters lined up like this to echo matrix structure
			return new TransformUtil(
				xScale, 0, 0,
				0, yScale, 0 );
#pragma warning restore SA1117
		}

		/// <summary>
		/// Returns a <see cref="TransformUtil"/> that applies an anti-clockwise (if the axes are considered in the standard / right-handed orientation - otherwise clockwise) rotation around the origin.
		/// </summary>
		/// <param name="rot">The number of radians by which to rotate.</param>
		/// <returns>A <see cref="TransformUtil"/> instance.</returns>
		public static TransformUtil Rotation( float rot )
		{
			float sin = ( float )Math.Sin( rot );
			float cos = ( float )Math.Cos( rot );
#pragma warning disable SA1117 // Parameters lined up like this to echo matrix structure
			return new TransformUtil(
				cos, -sin, 0,
				sin, cos, 0 );
#pragma warning restore SA1117
		}

		/// <summary>
		/// Performs a linear transformation.
		/// </summary>
		/// <param name="localUnitX">The vector to map the vector (1, 0) to.</param>
		/// <param name="localUnitY">The vector to map the vector (0, 1) to.</param>
		/// <returns>A <see cref="TransformUtil"/> instance.</returns>
		public static TransformUtil Linear( float2 localUnitX, float2 localUnitY )
		{
#pragma warning disable SA1117 // Parameters lined up like this to echo matrix structure
			return new TransformUtil(
				localUnitX.x, localUnitY.x, 0,
				localUnitX.y, localUnitY.y, 0 );
#pragma warning restore SA1117
		}

		/// <summary>
		/// Returns a transform for converting local coordinates to world coordinates.
		/// Just a rotation then a translation - but constructing the matrix directly is faster.
		/// </summary>
		/// <param name="localOrigin">The position of the local origin in world space.</param>
		/// <param name="localUnitX">The local unit-x vector in world space.</param>
		/// <param name="localUnitY">The local unit-y vector in world space.</param>
		/// <returns>A transform for converting local coordinates to world coordinates.</returns>
		public static TransformUtil ToWorldSpace( float2 localOrigin, float2 localUnitX, float2 localUnitY )
		{
#pragma warning disable SA1117 // Parameters lined up like this to echo matrix structure
			return new TransformUtil(
				localUnitX.x, localUnitY.x, localOrigin.x,
				localUnitX.y, localUnitY.y, localOrigin.y );
#pragma warning restore SA1117
		}

		/// <summary>
		/// Returns a transform for converting world coordinates to local coordinates.
		/// Just a translation then a rotation (the inverse of the world transform) - but constructing the matrix directly is faster.
		/// </summary>
		/// <param name="localOrigin">The position of the local origin in world space.</param>
		/// <param name="localUnitX">The local unit-x vector in world space.</param>
		/// <param name="localUnitY">The local unit-y vector in world space.</param>
		/// <returns>A transform for converting world coordinates to local coordinates..</returns>
		public static TransformUtil ToLocalSpace( float2 localOrigin, float2 localUnitX, float2 localUnitY )
		{
#pragma warning disable SA1117 // Parameters lined up like this to echo matrix structure
			return new TransformUtil(
				localUnitX.x, localUnitX.y, -math.dot( localOrigin, localUnitX ),
				localUnitY.x, localUnitY.y, -math.dot( localOrigin, localUnitY ) );
#pragma warning restore SA1117
		}

		/// <summary>
		/// Applies the transformation to a <see cref="float2"/>.
		/// </summary>
		/// <param name="vector">The vector to transform.</param>
		/// <returns>The result of the transformation.</returns>
		public float2 Apply( float2 vector )
		{
			// NB: This is just multiplication of the vector (x, y, 1) by the transformation matrix.
			return new float2(
				( this.r1c1 * vector.x ) + ( this.r1c2 * vector.y ) + this.r1c3,
				( this.r2c1 * vector.x ) + ( this.r2c2 * vector.y ) + this.r2c3 );
		}

		/// <summary>
		/// Applies the transformation to a list of vectors, updating them in-place.
		/// </summary>
		/// <param name="vectors">The list of vectors to transform.</param>
		public void Apply( IList<float2> vectors )
		{
			for ( int i = 0; i < vectors.Count; i++ )
			{
				vectors[i] = this.Apply( vectors[i] );
			}
		}

		/// <summary>
		/// Composes this transform with another.
		/// </summary>
		/// <param name="transform">The transform to compose with this one.</param>
		/// <returns>A single <see cref="TransformUtil"/> representing this transform followed by the other.</returns>
		public TransformUtil Compose( TransformUtil transform )
		{
			// NB: This is just matrix multiplication of transform * this (not forgetting the implicit third row of 0 0 1 in each).
			return new TransformUtil(
				transform.r1c1 * this.r1c1 + transform.r1c2 * this.r2c1,
				transform.r1c1 * this.r1c2 + transform.r1c2 * this.r2c2,
				transform.r1c1 * this.r1c3 + transform.r1c2 * this.r2c3 + transform.r1c3,
				transform.r2c1 * this.r1c1 + transform.r2c2 * this.r2c1,
				transform.r2c1 * this.r1c2 + transform.r2c2 * this.r2c2,
				transform.r2c1 * this.r1c3 + transform.r2c2 * this.r2c3 + transform.r2c3 );
		}
	}
}
