using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Steering
{
	/// <summary>
	/// 表示代理导航的路径
	/// </summary>
	public class Path
	{
		private LinkedList<float2> _waypoints;
		private LinkedListNode<float2> _currentWaypoint;

		/// <summary>
		/// 获取或设置一个值,该值指示是否应该将第一个航点导航到最后一个航点之后
		/// </summary>
		public bool isLooped;

		/// <summary>
		/// 获取当前活动的航路点
		/// </summary>
		public float2 current => this._currentWaypoint.Value;

		/// <summary>
		/// 获取一个值,该值指示是否已到达路径的结尾
		/// </summary>
		public bool isFinished => this._currentWaypoint == this._waypoints.Last;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="waypoints">组成路径的点列表</param>
		/// <param name="isLooped">是否循环</param>
		public Path( IEnumerable<float2> waypoints, bool isLooped )
		{
			this._waypoints = new LinkedList<float2>( waypoints );
			this.isLooped = isLooped;
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="numWaypoints">要创建的航点数。</ param>
		/// <param name="minX">航点的边界框的最小x位置。</ param>
		/// <param name="minY">航路点的边界框的最小y位置。</ param>
		/// <param name="maxX">航点的边界框的最大x位置。</ param>
		/// <param name="maxY">航路点的边界框的最大y位置。</ param>
		/// <param name="isLooped">是否循环</param>
		public Path(
			int numWaypoints,
			float minX,
			float minY,
			float maxX,
			float maxY,
			bool isLooped )
		{
			this._waypoints = new LinkedList<float2>();

			float midX = ( maxX + minX ) / 2.0f;
			float midY = ( maxY + minY ) / 2.0f;

			float smaller = Math.Min( midX, midY );
			float spacing = ( float )Math.PI * 2 / numWaypoints;

			for ( int i = 0; i < numWaypoints; ++i )
			{
				float radialDist = 0.6f * smaller; // RandInRange(smaller * 0.2f, smaller);
				var transform = TransformUtil.Rotation( i * spacing ).Compose( TransformUtil.Translation( midX, midY ) );
				this._waypoints.AddLast( transform.Apply( new float2( radialDist, 0.0f ) ) );
			}

			this.isLooped = isLooped;
			this._currentWaypoint = this._waypoints.First;
		}

		/// <summary>
		/// 使路径中的下一个航路点处于活动状态
		/// </summary>
		/// <returns>如果下一个航路点已被激活,则为真:如果我们已经在路径的末尾,则为false</returns>
		public bool MoveNext()
		{
			if ( this._currentWaypoint.Next != null )
			{
				this._currentWaypoint = this._currentWaypoint.Next;
				return true;
			}
			else if ( this.isLooped )
			{
				this._currentWaypoint = this._waypoints.First;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// 在路径的末尾添加一个航点
		/// </summary>
		/// <param name="waypoint">要添加的航点</param>
		public void AddWayPoint( float2 waypoint ) => this._waypoints.AddLast( waypoint );

		/// <summary>
		/// 从路径中清除所有航路点
		/// </summary>
		public void Clear() => this._waypoints.Clear();
	}
}
