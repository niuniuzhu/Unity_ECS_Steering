using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	public static class Environment
	{
		public readonly static World world = World.DefaultGameObjectInjectionWorld;

		public readonly static Random random = new Random( ( uint )new System.Random().Next() );

		public static float2 minXY;
		public static float2 maxXY;
		public static int2 numCell;
		public static float2 cellSize;
	}
}
