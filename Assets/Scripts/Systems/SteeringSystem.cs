using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Steering.VehicleData;

namespace Steering
{
	[UpdateAfter( typeof( FindNeighbourSystem ) )]
	[UpdateAfter( typeof( FindObstacleSystem ) )]
	[UpdateAfter( typeof( FindWallSystem ) )]
	public class SteeringSystem : JobComponentSystem
	{
		protected override void OnDestroy()
		{
		}

		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			var dt = Time.DeltaTime;
			var random = Environment.random;

			var jobHandle = Entities.ForEach( ( Entity vehicle, ref Translation translation, ref Rotation rotation,
				   ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
				   in DynamicBuffer<NeighbourElement> neighbours, in DynamicBuffer<ObstacleElement> obstacles ) =>
			 {
				 //计算速度
				 var steeringForce = Calculate( ref entityData, ref movingData, ref vehicleData, in neighbours, in obstacles, random );
				 var acceleration = steeringForce / movingData.mass;
				 movingData.velocity += acceleration * dt;

				 //计算速率
				 movingData.speed = math.length( movingData.velocity );
				 if ( movingData.speed > 0 )
				 {
					 //更新朝向
					 movingData.forward = movingData.velocity / movingData.speed;
					 movingData.right = new float2( movingData.forward.y, -movingData.forward.x );
					 //限制速度
					 if ( movingData.speed >= movingData.maxSpeed )
					 {
						 movingData.velocity = movingData.forward * movingData.maxSpeed;
						 movingData.speed = movingData.maxSpeed;
					 }
				 }

				 rotation.Value = quaternion.LookRotation( new float3( movingData.forward.x, 0, movingData.forward.y ), new float3( 0, 1, 0 ) );
				 entityData.position += movingData.velocity * dt;
				 translation.Value = new float3( entityData.position.x, 0, entityData.position.y );
			 } ).Schedule( inputDeps );

			return jobHandle;
		}

		#region 计算合操纵力
		/// <summary>
		/// 计算合操纵力
		/// </summary>
		/// <param name="vehicle">智能体</param>
		/// <returns>合操纵力</returns>
		public static float2 Calculate( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			in DynamicBuffer<NeighbourElement> neighbours, in DynamicBuffer<ObstacleElement> obstacles, Random random )
		{
			var steeringForce = float2.zero;
			switch ( vehicleData.summingMethod )
			{
				case SummingMethod.WeightedAverage:
					steeringForce = CalculateWeightedSum( ref entityData, ref movingData, ref vehicleData, in neighbours, in obstacles, random );
					break;
			}

			return steeringForce;
		}

		/// <summary>
		/// 计算合操纵力
		/// </summary>
		/// <returns>合操纵力</returns>
		private static float2 CalculateWeightedSum( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			in DynamicBuffer<NeighbourElement> neighbours, in DynamicBuffer<ObstacleElement> obstacles, Random random )
		{
			var steeringForce = float2.zero;

			if ( ( vehicleData.flags & Behaviors.Wander ) > 0 )
			{
				steeringForce += Wander( ref entityData, ref movingData, ref vehicleData, random ) * vehicleData.weightWander;
			}

			if ( ( vehicleData.flags & Behaviors.Seek ) > 0 )
			{
				steeringForce += Seek( ref entityData, ref movingData, vehicleData.targetPosition ) * vehicleData.weightSeek;
			}

			if ( ( vehicleData.flags & Behaviors.Flee ) > 0 )
			{
				steeringForce += Flee( ref entityData, ref movingData, vehicleData.targetPosition ) * vehicleData.weightFlee;
			}

			if ( ( vehicleData.flags & Behaviors.Arrive ) > 0 )
			{
				steeringForce += Arrive( ref entityData, ref movingData, ref vehicleData, vehicleData.targetPosition, vehicleData.deceleration ) * vehicleData.weightArrive;
			}

			return Truncate( steeringForce, movingData.maxForce );
		}

		private static float2 Truncate( float2 force, float max )
		{
			if ( math.lengthsq( force ) > max * max )
				force = math.normalize( force ) * max;
			return force;
		}
		#endregion

		#region 操纵行为

		/// <summary>
		/// 计算转向力，该转向力使智能体向指定位置加速
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Seek( ref EntityData entityData, ref MovingData movingData, float2 targetPos )
		{
			var desiredVelocity = math.normalize( targetPos - entityData.position ) * movingData.maxSpeed;
			return desiredVelocity - movingData.velocity;
		}

		/// <summary>
		/// 计算转向力，该转向力使智能体远离指定位置
		/// </summary>
		/// <param name="targetPos">目标位置</param>
		/// <returns>操纵力</returns>
		private static float2 Flee( ref EntityData entityData, ref MovingData movingData, float2 targetPos )
		{
			var desiredVelocity = math.normalize( entityData.position - targetPos ) * movingData.maxSpeed;
			return desiredVelocity - movingData.velocity;
		}

		/// <summary>
		/// 计算智能体到达给定点并以零速度到达该点的转向力
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Arrive( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData, float2 targetPos, Deceleration deceleration )
		{
			var toTarget = targetPos - entityData.position;
			var dist = math.length( toTarget );

			if ( dist > 0 )
			{
				// calculate the speed required to reach the target given the desired
				// deceleration
				var speed = dist / ( ( int )deceleration * vehicleData.decelerationTweaker );

				// make sure the velocity does not exceed the max
				speed = speed > movingData.maxSpeed ? speed : movingData.maxSpeed;

				// from here proceed just like Seek except we don't need to normalize
				// the ToTarget vector because we have already gone to the trouble
				// of calculating its length: dist.
				var desiredVelocity = toTarget * speed / dist;

				return desiredVelocity - movingData.velocity;
			}
			return float2.zero;
		}

		/// <summary>
		/// 徘徊
		/// </summary>
		/// <param name="vehicle">智能体</param>
		/// <returns>操纵力</returns>
		private static float2 Wander( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData, Random random )
		{
			// reproject this new vector back on to a unit circle
			// then increase the length of the vector to the same as the radius of the wander circle
			float angle = random.NextFloat( 0f, math.PI * 2 );
			var onUnitCircle = new float2( math.cos( angle ), math.sin( angle ) );

			vehicleData.wanderTarget = onUnitCircle * vehicleData.wanderRadius;

			// move the target into a position WanderDist in front of the agent
			var target = vehicleData.wanderTarget + new float2( vehicleData.wanderDistance, 0 );

			// project the target into world space
			target = TransformUtil.ToWorldSpace( entityData.position, movingData.forward, movingData.right ).Apply( target );

			// and steer towards it
			return target - entityData.position;
		}

		public static void FleeOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Flee;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void SeekOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Seek;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void ArriveOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Arrive;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void WanderOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Wander;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void FleeOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Flee;
		}

		public static void SeekOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Seek;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void ArriveOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Arrive;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void WanderOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Wander;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static bool IsFleeOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Flee ) > 0;
		public static bool IsSeekOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Seek ) > 0;
		public static bool IsArriveOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Arrive ) > 0;
		public static bool IsWanderOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Wander ) > 0;
		#endregion
	}
}
