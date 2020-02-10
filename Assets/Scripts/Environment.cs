using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	public static class Environment
	{
		public static World world;
		public static Random random;

		public static float2 minXY;
		public static float2 maxXY;
		public static int2 numCell;
		public static float2 cellSize;
	}
}
