using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	[GenerateAuthoringComponent]
	public struct WallData : IComponentData
	{
		/// <summary>
		/// 获取墙的起始位置
		/// </summary>
		public float2 from;

		/// <summary>
		/// 获取墙的结束位置
		/// </summary>
		public float2 to;

		/// <summary>
		/// 获取墙的法线
		/// </summary>
		public float2 normal;

		/// <summary>
		/// 获取墙的中心点
		/// </summary>
		public float2 center;

		/// <summary>
		/// 获取墙的大小
		/// </summary>
		public float2 size;
	}
}
