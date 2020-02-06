using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	[GenerateAuthoringComponent]
	public struct ObstacleData : IComponentData
	{
		public float2 position;
		public float radius;
	}
}
