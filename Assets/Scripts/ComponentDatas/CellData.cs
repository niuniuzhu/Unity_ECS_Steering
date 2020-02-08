using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	public struct CellData : IComponentData
	{
		public int index;
		public float2 center;
		public float2 extends;
	}

	public struct CellEntityElement : IBufferElementData
	{
		public Entity entity;
		public float2 position;
		public float radius;
	}
}
