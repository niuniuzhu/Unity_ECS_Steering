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
					 //确实的前向量
					 movingData.realForward = movingData.velocity / movingData.speed;
					 //更新朝向
					 movingData.forward = math.lerp( movingData.forward, movingData.realForward, dt * movingData.forwardSmooth );
					 movingData.right = new float2( movingData.forward.y, -movingData.forward.x );
					 //限制速度
					 if ( movingData.speed >= movingData.maxSpeed )
					 {
						 movingData.velocity = movingData.forward * movingData.maxSpeed;
						 movingData.speed = movingData.maxSpeed;
					 }

					 rotation.Value = quaternion.LookRotation( new float3( movingData.forward.x, 0, movingData.forward.y ), new float3( 0, 1, 0 ) );
					 entityData.position += movingData.velocity * dt;
					 translation.Value = new float3( entityData.position.x, 0, entityData.position.y );
				 }
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

				case SummingMethod.Prioritized:
					steeringForce = CalculatePrioritized( ref entityData, ref movingData, ref vehicleData, in neighbours, in obstacles, random );
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
				steeringForce += Wander( random, entityData, movingData, vehicleData ) * vehicleData.weightWander;
			}

			if ( ( vehicleData.flags & Behaviors.Seek ) > 0 )
			{
				steeringForce += Seek( vehicleData.targetPosition, entityData, movingData ) * vehicleData.weightSeek;
			}

			if ( ( vehicleData.flags & Behaviors.Flee ) > 0 )
			{
				steeringForce += Flee( vehicleData.targetPosition, entityData, movingData ) * vehicleData.weightFlee;
			}

			if ( ( vehicleData.flags & Behaviors.Arrive ) > 0 )
			{
				steeringForce += Arrive( vehicleData.targetPosition, entityData, movingData, vehicleData ) * vehicleData.weightArrive;
			}

			if ( ( vehicleData.flags & Behaviors.ObstacleAvoidance ) > 0 )
			{
				steeringForce += ObstacleAvoidance( entityData, movingData, obstacles ) * vehicleData.weightObstacleAvoidance;
			}

			return Truncate( steeringForce, movingData.maxForce );
		}



		/// <summary>
		/// 此方法按优先级顺序调用每个活动的转向行为
		/// 并计算合力，直到最大转向力大小
		/// 达到指定值时，函数将返回转向力
		/// </summary>
		/// <returns>合操纵力</returns>
		private static float2 CalculatePrioritized( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			in DynamicBuffer<NeighbourElement> neighbours, in DynamicBuffer<ObstacleElement> obstacles, Random random )
		{
			float2 force;
			var steeringForce = float2.zero;

			if ( ( vehicleData.flags & Behaviors.WallAvoidance ) > 0 )
			{
				//force = WallAvoidance( ref vehicleData ) * vehicleData.weightWallAvoidance;
				//if ( !AccumulateForce( force, ref steeringForce, movingData ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.ObstacleAvoidance ) > 0 )
			{
				force = ObstacleAvoidance( entityData, movingData, obstacles ) * vehicleData.weightObstacleAvoidance;
				if ( !AccumulateForce( force, ref steeringForce, movingData ) )
					return steeringForce;
			}

			if ( ( vehicleData.flags & Behaviors.Flee ) > 0 )
			{
				force = Flee( vehicleData.targetPosition, entityData, movingData ) * vehicleData.weightFlee;
				if ( !AccumulateForce( force, ref steeringForce, movingData ) )
					return steeringForce;
			}

			if ( ( vehicleData.flags & Behaviors.Seek ) > 0 )
			{
				force = Seek( vehicleData.targetPosition, entityData, movingData ) * vehicleData.weightSeek;
				if ( !AccumulateForce( force, ref steeringForce, movingData ) )
					return steeringForce;
			}

			if ( ( vehicleData.flags & Behaviors.Arrive ) > 0 )
			{
				force = Arrive( vehicleData.targetPosition, entityData, movingData, vehicleData ) * vehicleData.weightArrive;
				if ( !AccumulateForce( force, ref steeringForce, movingData ) )
					return steeringForce;
			}

			if ( ( vehicleData.flags & Behaviors.Wander ) > 0 )
			{
				force = Wander( random, entityData, movingData, vehicleData ) * vehicleData.weightWander;
				if ( !AccumulateForce( force, ref steeringForce, movingData ) )
					return steeringForce;
			}
			return steeringForce;
		}

		/// <summary>
		/// 此函数计算代理剩余的最大转向力要施加多少，然后再施加要增加的那部分力
		/// </summary>
		/// <returns>如果剩余容量尚待增加，则为真，否则为false</returns>
		private static bool AccumulateForce( float2 forceToAdd, ref float2 runningTotal, in MovingData movingData )
		{
			// calculate how much steering force the _vehicle has used so far
			var magnitudeSoFar = math.length( runningTotal );

			// calculate how much steering force remains to be used by this _vehicle
			var magnitudeRemaining = movingData.maxForce - magnitudeSoFar;

			// return false if there is no more force left to use
			if ( magnitudeRemaining <= 0.0f )
				return false;

			// calculate the magnitude of the force we want to add
			var magnitudeToAdd = math.length( forceToAdd );

			// if the magnitude of the sum of ForceToAdd and the running total
			// does not exceed the maximum force available to this _vehicle, just
			// add together. Otherwise add as much of the ForceToAdd vector is
			// possible without going over the max.
			if ( magnitudeToAdd < magnitudeRemaining )
				runningTotal += forceToAdd;
			else
				// add it to the steering force
				runningTotal += math.normalize( forceToAdd ) * magnitudeRemaining;

			return true;
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
		private static float2 Seek( float2 targetPos, in EntityData entityData, in MovingData movingData )
		{
			var desiredVelocity = math.normalize( targetPos - entityData.position ) * movingData.maxSpeed;
			return desiredVelocity - movingData.velocity;
		}

		/// <summary>
		/// 计算转向力，该转向力使智能体远离指定位置
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Flee( float2 targetPos, in EntityData entityData, in MovingData movingData )
		{
			var desiredVelocity = math.normalize( entityData.position - targetPos ) * movingData.maxSpeed;
			return desiredVelocity - movingData.velocity;
		}

		/// <summary>
		/// 计算智能体到达给定点并以零速度到达该点的转向力
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Arrive( float2 targetPos, in EntityData entityData, in MovingData movingData, in VehicleData vehicleData )
		{
			var toTarget = targetPos - entityData.position;
			var dist = math.lengthsq( toTarget );

			if ( dist <= 0.04f )
				return -movingData.velocity;

			float speed = movingData.maxSpeed;
			if ( dist < vehicleData.decelerationDistance * vehicleData.decelerationDistance )
				speed = movingData.maxSpeed * dist / vehicleData.decelerationDistance;

			var dir = toTarget / math.sqrt( dist );
			var desiredVelocity = dir * speed;

			//// calculate the speed required to reach the target given the desired
			//// deceleration
			//var speed = dist / vehicleData.decelerationTweaker;

			//// make sure the velocity does not exceed the max
			//speed = speed > movingData.maxSpeed ? movingData.maxSpeed : speed;

			//// from here proceed just like Seek except we don't need to normalize
			//// the ToTarget vector because we have already gone to the trouble
			//// of calculating its length: dist.
			//var desiredVelocity = toTarget * speed / dist;

			return desiredVelocity - movingData.velocity;
		}

		/// <summary>
		/// 徘徊
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Wander( Random random, in EntityData entityData, in MovingData movingData, in VehicleData vehicleData )
		{
			// reproject this new vector back on to a unit circle
			// then increase the length of the vector to the same as the radius of the wander circle
			float angle = random.NextFloat( 0f, math.PI * 2 );
			var onUnitCircle = new float2( math.cos( angle ), math.sin( angle ) );

			var wanderTarget = onUnitCircle * vehicleData.wanderRadius;

			// move the target into a position WanderDist in front of the agent
			var target = wanderTarget + new float2( vehicleData.wanderDistance, 0 );

			// project the target into world space
			target = TransformUtil.ToWorldSpace( entityData.position, movingData.forward, movingData.right ).Apply( target );

			// and steer towards it
			return target - entityData.position;
		}

		/// <summary>
		/// 逃避障碍物
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 ObstacleAvoidance( in EntityData entityData, in MovingData movingData, in DynamicBuffer<ObstacleElement> obstacles )
		{
			// is closest obstacle be found
			var foundClosest = false;

			// this will keep track of the closest intersecting obstacle (CIB)
			var closestIntersectingObstacle = new ObstacleElement();

			// this will be used to track the distance to the CIB
			var distToClosestIP = float.MaxValue;

			// this will record the transformed local coordinates of the CIB
			var localPosOfClosestObstacle = float2.zero;

			var count = obstacles.Length;
			for ( var i = 0; i < count; i++ )
			{
				var obstacleElement = obstacles[i];

				// calculate this obstacle's position in local space
				var localPos = TransformUtil.ToLocalSpace( entityData.position, movingData.forward, movingData.right ).Apply( obstacleElement.position );

				// if the local position has a negative x value then it must lay
				// behind the agent. (in which case it can be ignored)
				if ( localPos.x >= 0 )
				{
					// if the distance from the x axis to the object's position is less
					// than its radius + half the width of the detection box then there
					// is a potential intersection.
					var expandedRadius = obstacleElement.radius + entityData.radius;

					if ( math.abs( localPos.y ) < expandedRadius )
					{
						// now to do a line/circle intersection test. The center of the
						// circle is represented by (cX, cY). The intersection points are
						// given by the formula x = cX +/-sqrt(r^2-cY^2) for y=0.
						// We only need to look at the smallest positive value of x because
						// that will be the closest point of intersection.
						var cX = localPos.x;
						var cY = localPos.y;

						// we only need to calculate the sqrt part of the above equation once
						var sqrtPart = math.sqrt( expandedRadius * expandedRadius - cY * cY );
						var ip = cX - sqrtPart;

						if ( ip <= 0.0 )
						{
							ip = cX + sqrtPart;
						}

						// test to see if this is the closest so far. If it is keep a
						// record of the obstacle and its local coordinates
						if ( ip < distToClosestIP )
						{
							foundClosest = true;
							closestIntersectingObstacle = obstacleElement;
							distToClosestIP = ip;
							localPosOfClosestObstacle = localPos;
						}
					}
				}
			}

			// if we have found an intersecting obstacle, calculate a steering
			// force away from it
			var localSteeringForce = float2.zero;

			if ( foundClosest )
			{
				// the detection box length is proportional to the agent's velocity
				var detectionBoxLength = entityData.radius + SteeringSettings.MinDetectionBoxLength + ( movingData.speed / movingData.maxSpeed ) * SteeringSettings.MinDetectionBoxLength;

				// the closer the agent is to an object, the stronger the
				// steering force should be
				var multiplier = 1.0f + ( detectionBoxLength - ( localPosOfClosestObstacle.x - closestIntersectingObstacle.radius ) ) / detectionBoxLength;

				// apply a braking force proportional to the obstacles distance from the _vehicle.
				const float BrakingWeight = 0.2f;

				localSteeringForce = new float2(
					( closestIntersectingObstacle.radius + entityData.radius - localPosOfClosestObstacle.x ) * BrakingWeight,
					( closestIntersectingObstacle.radius + entityData.radius - localPosOfClosestObstacle.y ) * multiplier );
			}

			// finally, convert the steering vector from local to world space
			return TransformUtil.ToWorldSpace( float2.zero, movingData.forward, movingData.right ).Apply( localSteeringForce );
		}

		public static void FleeOn( ref VehicleData vehicleData )
		{
			vehicleData.flags |= Behaviors.Flee;
		}

		public static void SeekOn( ref VehicleData vehicleData )
		{
			vehicleData.flags |= Behaviors.Seek;
		}

		public static void ArriveOn( ref VehicleData vehicleData )
		{
			vehicleData.flags |= Behaviors.Arrive;
		}

		public static void WanderOn( ref VehicleData vehicleData )
		{
			vehicleData.flags |= Behaviors.Wander;
		}

		public static void ObstacleAvoidanceOn( ref VehicleData vehicleData )
		{
			vehicleData.flags |= Behaviors.ObstacleAvoidance;
		}

		public static void FleeOff( ref VehicleData vehicleData )
		{
			vehicleData.flags &= ~Behaviors.Flee;
		}

		public static void SeekOff( ref VehicleData vehicleData )
		{
			vehicleData.flags &= ~Behaviors.Seek;
		}

		public static void ArriveOff( ref VehicleData vehicleData )
		{
			vehicleData.flags &= ~Behaviors.Arrive;
		}

		public static void WanderOff( ref VehicleData vehicleData )
		{
			vehicleData.flags &= ~Behaviors.Wander;
		}

		public static void ObstacleAvoidanceOff( ref VehicleData vehicleData )
		{
			vehicleData.flags &= ~Behaviors.ObstacleAvoidance;
		}

		public static bool IsFleeOn( in VehicleData vehicleData ) => ( vehicleData.flags & Behaviors.Flee ) > 0;
		public static bool IsSeekOn( in VehicleData vehicleData ) => ( vehicleData.flags & Behaviors.Seek ) > 0;
		public static bool IsArriveOn( in VehicleData vehicleData ) => ( vehicleData.flags & Behaviors.Arrive ) > 0;
		public static bool IsWanderOn( in VehicleData vehicleData ) => ( vehicleData.flags & Behaviors.Wander ) > 0;
		public static bool IsObstacleAvoidanceOn( in VehicleData vehicleData ) => ( vehicleData.flags & Behaviors.ObstacleAvoidance ) > 0;
		#endregion
	}
}
