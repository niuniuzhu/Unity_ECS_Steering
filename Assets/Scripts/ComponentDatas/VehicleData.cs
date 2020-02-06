using Unity.Entities;
using Unity.Mathematics;

namespace Steering
{
	[GenerateAuthoringComponent]
	public struct VehicleData : IComponentData
	{
		public enum Deceleration
		{
			Normal,
			Slow,
			Fast
		}

		[System.Flags]
		public enum Behaviors
		{
			None = 0x00000,
			Seek = 0x00002,
			Flee = 0x00004,
			Arrive = 0x00008,
			Wander = 0x00010,
			Cohesion = 0x00020,
			Separation = 0x00040,
			Alignment = 0x00080,
			ObstacleAvoidance = 0x00100,
			WallAvoidance = 0x00200,
			FollowPath = 0x00400,
			Pursuit = 0x00800,
			Evade = 0x01000,
			Interpose = 0x02000,
			Hide = 0x04000,
			Flock = 0x08000,
			OffsetPursuit = 0x10000
		}

		/// <summary>
		/// 群集行为的组合方式的枚举
		/// </summary>
		public enum SummingMethod
		{
			/// <summary>
			/// 根据各个转向力计算出加权平均值
			/// </summary>
			WeightedAverage,
			/// <summary>
			/// 先施加较高优先级的转向力
			/// </summary>
			Prioritized,
			/// <summary>
			/// 在每个更新步骤中都会应用随机选择的转向力
			/// </summary>
			Dithered
		}

		public struct WallDetectionData
		{
			public Entity wall;
			public int wallDetectionFeelerIndex;
			public float2 wallNormal;
			public float2 closestPoint;
		}

		/// <summary>
		/// 徘徊圆半径
		/// </summary>
		public float wanderRadius;
		/// <summary>
		/// 徘徊圈到智能体的距离
		/// </summary>
		public float wanderDistance;
		/// <summary>
		/// 每帧沿圆的最大位移量
		/// </summary>
		public float wanderJitterPerSec;
		/// <summary>
		/// 智能体必须先于路径航路点的距离(平方)
		/// 开始寻找下个航路点
		/// </summary>
		public float waypointSeekDistSquared;

		/// <summary>
		/// 群集行为的组合方式
		/// </summary>
		public SummingMethod summingMethod;

		//乘数,这些可以调整以影响强度
		public float weightSeparation;
		public float weightCohesion;
		public float weightAlignment;
		public float weightWander;
		public float weightObstacleAvoidance;
		public float weightWallAvoidance;
		public float weightSeek;
		public float weightFlee;
		public float weightArrive;
		public float weightPursuit;
		public float weightOffsetPursuit;
		public float weightInterpose;
		public float weightHide;
		public float weightEvade;
		public float weightFollowPath;
		/// <summary>
		/// 用于追踪friends, pursuers, or prey
		/// </summary>
		public Entity targetAgent1;
		/// <summary>
		/// 用于追踪friends, pursuers, or prey
		/// </summary>
		public Entity targetAgent2;
		/// <summary>
		/// 最近的墙
		/// </summary>
		public WallDetectionData wallDetectionData;
		/// <summary>
		/// 用于arrive等行为的目标位置
		/// </summary>
		public float2 targetPosition;
		/// <summary>
		/// 用于arrive等行为的减速度
		/// </summary>
		public float decelerationTweaker;

		public float2 offset;

		public Deceleration deceleration;
		/// <summary>
		/// 徘徊目标点
		/// </summary>
		public float2 wanderTarget;
		/// <summary>
		/// 避障中使用的检测盒的长度
		/// </summary>
		public float detectionBoxLength;
		/// <summary>
		/// 墙壁检测中使用的触须长度
		/// </summary>
		public float wallDetectionFeelerLength;
		/// <summary>
		/// 当前路径
		/// </summary>
		//public Path path;
		/// <summary>
		/// 二进制标志,指示行为是否应处于活动状态
		/// </summary>
		public Behaviors flags;
		/// <summary>
		/// 墙壁检测中的触须
		/// </summary>
		public float2 wallDetectionFeeler0;
		/// <summary>
		/// 墙壁检测中的触须
		/// </summary>
		public float2 wallDetectionFeeler1;
		/// <summary>
		/// 墙壁检测中的触须
		/// </summary>
		public float2 wallDetectionFeeler2;
	}

	[GenerateAuthoringComponent]
	public struct NeighbourElement : IBufferElementData
	{
		public Entity neighbour;
	}

	[GenerateAuthoringComponent]
	public struct ObstacleElement : IBufferElementData
	{
		public Entity obstacle;
	}
}
