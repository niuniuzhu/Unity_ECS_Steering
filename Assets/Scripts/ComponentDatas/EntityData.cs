using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	[GenerateAuthoringComponent]
	public struct EntityData : IComponentData
	{
		/// <summary>
		/// 位置
		/// </summary>
		public float2 position;
		/// <summary>
		/// 半径
		/// </summary>
		public float radius;
		/// <summary>
		/// 重量
		/// </summary>
		public float mass;
	}
}
