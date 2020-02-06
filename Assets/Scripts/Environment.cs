using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	public static class Environment
	{
		public readonly static World world = World.DefaultGameObjectInjectionWorld;

		public readonly static Random random = new Random( ( uint )new System.Random().Next() );
	}
}
