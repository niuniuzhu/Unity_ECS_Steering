using Unity.Mathematics;
using UnityEngine;

namespace Steering
{
	public class LogicRandom : System.Random, IRandom
	{
		public const float PI = 3.1415926535897932384626433832795f;
		public const float PI2 = 6.283185307179586476925286766559f;
		public const float PI4 = 12.566370614359172953850573533118f;
		public const float PI_HALF = 1.5707963267948966192313216916398f;

		public float2 onUnitCircle
		{
			get
			{
				float angle = this.NextFloat( 0, PI2 );
				return new float2( Mathf.Cos( angle ), Mathf.Sin( angle ) );
			}
		}

		public float2 insideUnitCircle
		{
			get
			{
				float radius = this.NextFloat( 0, 1 );
				float angle = this.NextFloat( 0, PI2 );
				return new float2( radius * Mathf.Cos( angle ), radius * Mathf.Sin( angle ) );
			}
		}

		public Vector3 onUnitSphere
		{
			get
			{
				float theta = this.NextFloat( 0, PI2 );
				float phi = Mathf.Acos( 2 * this.NextFloat( 0, 1 ) - 1 );
				return new Vector3( Mathf.Cos( theta ) * Mathf.Sin( phi ), Mathf.Sin( theta ) * Mathf.Sin( phi ), Mathf.Cos( phi ) );
			}
		}

		public Vector3 insideUnitSphere
		{
			get
			{
				float theta = this.NextFloat( 0, PI2 );
				float phi = Mathf.Acos( 2 * this.NextFloat( 0, 1 ) - 1 );
				float r = Mathf.Pow( this.NextFloat( 0, 1 ), 1f / 3f );
				return new Vector3( r * Mathf.Cos( theta ) * Mathf.Sin( phi ), r * Mathf.Sin( theta ) * Mathf.Sin( phi ), r * Mathf.Cos( phi ) );
			}
		}

		public Quaternion rotation
		{
			get
			{
				float theta = this.NextFloat( 0, PI2 );
				float phi = this.NextFloat( -PI_HALF, PI_HALF );
				Vector3 v = new Vector3( Mathf.Sin( phi ) * Mathf.Sin( theta ), Mathf.Cos( phi ) * Mathf.Sin( theta ), Mathf.Cos( theta ) );
				return Quaternion.FromToRotation( Vector3.forward, v );
			}
		}

		public Quaternion rotationUniform => Quaternion.FromToRotation( Vector3.forward, this.onUnitSphere );

		public LogicRandom()
		{
		}

		public LogicRandom( int seed )
			: base( seed )
		{
		}

		public float NextFloat( float min, float max )
		{
			float value = this.NextFloat() * ( max - min ) + min;
			return value;
		}

		public float NextFloat()
		{
			float value = ( float )System.Math.Round( this.NextDouble(), 4 );
			return value;
		}

		public int NextInt( int min = int.MinValue, int max = int.MaxValue )
		{
			int value = this.Next( min, max );
			return value;
		}
	}

}