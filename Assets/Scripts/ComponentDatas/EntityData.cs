using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	[GenerateAuthoringComponent]
	public struct EntityData : IComponentData
	{
		public float2 position;
		public float radius;
	}
}
