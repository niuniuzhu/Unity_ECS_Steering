using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Steering.VehicleData;

namespace Steering
{
	[DisableAutoCreation]
	public class SteeringSystem_Old : SystemBase
	{
		protected override void OnDestroy()
		{
		}

		protected override void OnUpdate()
		{
			var dt = Time.DeltaTime;

			Entities.ForEach( ( Entity vehicle, ref Translation translation, ref Rotation rotation,
				   ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
				   ref DynamicBuffer<NeighbourElement> neighbours, ref DynamicBuffer<ObstacleElement> obstacles ) =>
			{
				//计算速度
				var steeringForce = Calculate( ref entityData, ref movingData, ref vehicleData, ref neighbours, ref obstacles );
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
			} ).ScheduleParallel();
		}

		#region 计算合操纵力
		/// <summary>
		/// 计算合操纵力
		/// </summary>
		/// <param name="vehicle">智能体</param>
		/// <returns>合操纵力</returns>
		public static float2 Calculate( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			ref DynamicBuffer<NeighbourElement> neighbours, ref DynamicBuffer<ObstacleElement> obstacles )
		{
			var steeringForce = float2.zero;
			switch ( vehicleData.summingMethod )
			{
				case SummingMethod.WeightedAverage:
					steeringForce = CalculateWeightedSum( ref entityData, ref movingData, ref vehicleData, ref neighbours, ref obstacles );
					break;
				case SummingMethod.Prioritized:
					steeringForce = CalculatePrioritized( ref entityData, ref movingData, ref vehicleData, ref neighbours, ref obstacles );
					break;
				case SummingMethod.Dithered:
					//steeringForce = CalculateDithered( ref entityData, ref movingData, ref vehicleData, ref neighbours, ref obstacles );
					break;
			}

			return steeringForce;
		}

		/// <summary>
		/// 计算合操纵力
		/// </summary>
		/// <returns>合操纵力</returns>
		private static float2 CalculateWeightedSum( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			ref DynamicBuffer<NeighbourElement> neighbours, ref DynamicBuffer<ObstacleElement> obstacles )
		{
			var steeringForce = float2.zero;

			if ( ( vehicleData.flags & Behaviors.WallAvoidance ) > 0 )
			{
				steeringForce += WallAvoidance( ref vehicleData ) * vehicleData.weightWallAvoidance;
			}

			if ( ( vehicleData.flags & Behaviors.ObstacleAvoidance ) > 0 )
			{
				//steeringForce += ObstacleAvoidance( ref entityData, ref movingData, ref vehicleData, ref obstacles ) * vehicleData.weightObstacleAvoidance;
			}

			if ( ( vehicleData.flags & Behaviors.Evade ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "Evade target not assigned" );
				//steeringForce += Evade( ref entityData, ref movingData, ref  ) * vehicleData.weightEvade;
			}

			// 以下三个可以合并为flocking行为
			if ( ( vehicleData.flags & Behaviors.Separation ) > 0 )
			{
				//steeringForce += Separation( ref entityData, ref vehicleData, ref neighbours ) * vehicleData.weightSeparation;
			}

			if ( ( vehicleData.flags & Behaviors.Alignment ) > 0 )
			{
				//steeringForce += Alignment( ref movingData, ref vehicleData, ref neighbours ) * vehicleData.weightAlignment;
			}

			if ( ( vehicleData.flags & Behaviors.Cohesion ) > 0 )
			{
				//steeringForce += Cohesion( ref entityData, ref movingData, ref vehicleData, ref neighbours ) * vehicleData.weightCohesion;
			}

			if ( ( vehicleData.flags & Behaviors.Wander ) > 0 )
			{
				//steeringForce += Wander( ref entityData, ref movingData, ref vehicleData ) * vehicleData.weightWander;
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
				steeringForce += Arrive( ref entityData, ref movingData, ref vehicleData, vehicleData.targetPosition ) * vehicleData.weightArrive;
			}

			if ( ( vehicleData.flags & Behaviors.Pursuit ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "pursuit target not assigned" );
				//steeringForce += Pursuit( vehicle, vehicleData.targetAgent1 ) * vehicleData.weightPursuit;
			}

			if ( ( vehicleData.flags & Behaviors.OffsetPursuit ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "pursuit target not assigned" );
				//Logger.Assert( !offset.IsZero(), "No offset assigned" );
				//steeringForce += OffsetPursuit( vehicle, vehicleData.targetAgent1, vehicleData.offset ) * vehicleData.weightOffsetPursuit;
			}

			if ( ( vehicleData.flags & Behaviors.Interpose ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null && targetAgent2 != null, "Interpose agents not assigned" );
				//steeringForce += Interpose( vehicle, vehicleData.targetAgent1, vehicleData.targetAgent2 ) * vehicleData.weightInterpose;
			}

			if ( ( vehicleData.flags & Behaviors.Hide ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "Hide target not assigned" );
				//steeringForce += Hide( vehicle, vehicleData.targetAgent1 ) * vehicleData.weightHide;
			}

			if ( ( vehicleData.flags & Behaviors.FollowPath ) > 0 )
			{
				steeringForce += FollowPath() * vehicleData.weightFollowPath;
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
			ref DynamicBuffer<NeighbourElement> neighbours, ref DynamicBuffer<ObstacleElement> obstacles )
		{
			float2 force;
			var steeringForce = float2.zero;

			if ( ( vehicleData.flags & Behaviors.WallAvoidance ) > 0 )
			{
				force = WallAvoidance( ref vehicleData ) * vehicleData.weightWallAvoidance;
				if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.ObstacleAvoidance ) > 0 )
			{
				//force = ObstacleAvoidance( ref entityData, ref movingData, ref vehicleData, ref obstacles ) * vehicleData.weightObstacleAvoidance;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Evade ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "Evade target not assigned" );
				//force = Evade( vehicle, vehicleData.targetAgent1 ) * vehicleData.weightEvade;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Flee ) > 0 )
			{
				//force = Flee( vehicle, vehicleData.targetPosition ) * vehicleData.weightFlee;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			// 以下三个可以合并为flocking行为
			if ( ( vehicleData.flags & Behaviors.Separation ) > 0 )
			{
				//force = Separation( ref entityData, ref vehicleData, ref neighbours ) * vehicleData.weightSeparation;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Alignment ) > 0 )
			{
				//force = Alignment( ref movingData, ref vehicleData, ref neighbours ) * vehicleData.weightAlignment;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Cohesion ) > 0 )
			{
				//force = Cohesion( ref entityData, ref movingData, ref vehicleData, ref neighbours ) * vehicleData.weightCohesion;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Seek ) > 0 )
			{
				force = Seek( ref entityData, ref movingData, vehicleData.targetPosition ) * vehicleData.weightSeek;
				if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Arrive ) > 0 )
			{
				force = Arrive( ref entityData, ref movingData, ref vehicleData, vehicleData.targetPosition ) * vehicleData.weightArrive;
				if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Wander ) > 0 )
			{
				//force = Wander( ref entityData, ref movingData, ref vehicleData ) * vehicleData.weightWander;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Pursuit ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "pursuit target not assigned" );
				//force = Pursuit( vehicle, vehicleData.targetAgent1 ) * vehicleData.weightPursuit;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.OffsetPursuit ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "pursuit target not assigned" );
				//Logger.Assert( !offset.IsZero(), "No offset assigned" );
				//force = OffsetPursuit( vehicle, vehicleData.targetAgent1, vehicleData.offset );
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Interpose ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null && targetAgent2 != null, "Interpose agents not assigned" );
				//force = Interpose( vehicle, vehicleData.targetAgent1, vehicleData.targetAgent2 ) * vehicleData.weightInterpose;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.Hide ) > 0 )
			{
				//Logger.Assert( targetAgent1 != null, "Hide target not assigned" );
				//force = Hide( ref entityData, ref movingData, ref vehicleData, vehicleData.targetAgent1 ) * vehicleData.weightHide;
				//if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}

			if ( ( vehicleData.flags & Behaviors.FollowPath ) > 0 )
			{
				force = FollowPath() * vehicleData.weightFollowPath;
				if ( !AccumulateForce( ref movingData, ref steeringForce, force ) ) { return steeringForce; }
			}
			return steeringForce;
		}

		/// <summary>
		/// 此方法通过分配概率来总结活动行为
		/// 计算每个行为。 然后测试第一个优先级
		/// 看看是否应该在此模拟步骤中进行计算。 如果是这样
		/// 计算由此产生的转向力。 如果是
		/// 大于零则返回力。 如果为零，或者行为为
		/// 跳过它继续到下一个优先级，依此类推。
		/// 
		/// 注意：并非所有行为都已通过此方法实现，
		/// 只是几个，所以您有了大致的了解
		/// </summary>
		/// <returns>合操纵力</returns>
		private static float2 CalculateDithered( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			ref DynamicBuffer<NeighbourElement> neighbours, ref DynamicBuffer<ObstacleElement> obstacles )
		{
			var steeringForce = float2.zero;

			if ( ( vehicleData.flags & Behaviors.WallAvoidance ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrWallAvoidance )
			{
				steeringForce = WallAvoidance( ref vehicleData ) * vehicleData.weightWallAvoidance / SteeringSettings.PrWallAvoidance;
				var b2 = steeringForce == float2.zero;
				if ( b2.x && b2.y )
					return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.ObstacleAvoidance ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrObstacleAvoidance )
			{
				//steeringForce += ObstacleAvoidance( ref entityData, ref movingData, ref vehicleData, ref obstacles ) * vehicleData.weightObstacleAvoidance / SteeringSettings.PrObstacleAvoidance;
				//var b2 = steeringForce == float2.zero;
				//if ( b2.x && b2.y )
				//	return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Separation ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrSeparation )
			{
				//steeringForce += Separation( ref entityData, ref vehicleData, ref neighbours ) * vehicleData.weightSeparation / SteeringSettings.PrSeparation;
				//var b2 = steeringForce == float2.zero;
				//if ( b2.x && b2.y )
				//	return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Flee ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrFlee )
			{
				steeringForce += Flee( ref entityData, ref movingData, vehicleData.targetPosition ) * vehicleData.weightFlee / SteeringSettings.PrFlee;
				var b2 = steeringForce == float2.zero;
				if ( b2.x && b2.y )
					return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Evade ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrEvade )
			{
				//Logger.Assert( targetAgent1 != null, "Evade target not assigned" );
				//steeringForce += Evade( vehicle, vehicleData.targetAgent1 ) * vehicleData.weightEvade / SteeringSettings.PrEvade;
				//var b2 = steeringForce == float2.zero;
				//if ( b2.x && b2.y )
				//	return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Alignment ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrAlignment )
			{
				//steeringForce += Alignment( ref movingData, ref vehicleData, ref neighbours ) * vehicleData.weightAlignment / SteeringSettings.PrAlignment;
				//var b2 = steeringForce == float2.zero;
				//if ( b2.x && b2.y )
				//	return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Cohesion ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrCohesion )
			{
				//steeringForce += Cohesion( ref entityData, ref movingData, ref vehicleData, ref neighbours ) * vehicleData.weightCohesion / SteeringSettings.PrCohesion;
				//var b2 = steeringForce == float2.zero;
				//if ( b2.x && b2.y )
				//	return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Wander ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrWander )
			{
				//steeringForce += Wander( ref entityData, ref movingData, ref vehicleData ) * vehicleData.weightWander / SteeringSettings.PrWander;
				//var b2 = steeringForce == float2.zero;
				//if ( b2.x && b2.y )
				//	return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Seek ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrSeek )
			{
				steeringForce += Seek( ref entityData, ref movingData, vehicleData.targetPosition ) * vehicleData.weightSeek / SteeringSettings.PrSeek;
				var b2 = steeringForce == float2.zero;
				if ( b2.x && b2.y )
					return Truncate( steeringForce, movingData.maxForce );
			}

			if ( ( vehicleData.flags & Behaviors.Arrive ) > 0 && Environment.random.NextFloat() < SteeringSettings.PrArrive )
			{
				steeringForce += Arrive( ref entityData, ref movingData, ref vehicleData, vehicleData.targetPosition ) * vehicleData.weightArrive / SteeringSettings.PrArrive;
				var b2 = steeringForce == float2.zero;
				if ( b2.x && b2.y )
					return Truncate( steeringForce, movingData.maxForce );
			}

			return steeringForce;
		}

		private static float2 Truncate( float2 force, float max )
		{
			if ( math.lengthsq( force ) > max * max )
				force = math.normalize( force ) * max;
			return force;
		}

		/// <summary>
		/// 此函数计算代理剩余的最大转向力要施加多少，然后再施加要增加的那部分力
		/// </summary>
		/// <param name="movingData">智能体</param>
		/// <param name="runningTotal">正在执行的操纵力,这将会被更新</param>
		/// <param name="forceToAdd">需要添加的力</param>
		/// <returns>如果剩余容量尚待增加，则为真，否则为false</returns>
		private static bool AccumulateForce( ref MovingData movingData, ref float2 runningTotal, float2 forceToAdd )
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
		/// <param name="vehicle">智能体</param>
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
		private static float2 Arrive( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData, float2 targetPos )
		{
			var toTarget = targetPos - entityData.position;
			var dist = math.length( toTarget );

			//if ( dist > 0 )
			//{
			//	// calculate the speed required to reach the target given the desired
			//	// deceleration
			//	var speed = dist / vehicleData.decelerationTweaker;

			//	// make sure the velocity does not exceed the max
			//	speed = speed > movingData.maxSpeed ? speed : movingData.maxSpeed;

			//	// from here proceed just like Seek except we don't need to normalize
			//	// the ToTarget vector because we have already gone to the trouble
			//	// of calculating its length: dist.
			//	var desiredVelocity = toTarget * speed / dist;

			//	return desiredVelocity - movingData.velocity;
			//}
			return float2.zero;
		}

		/// <summary>
		/// 此行为可预测智能体在时间T处的位置，并朝该点进行拦截.
		/// </summary>
		/// <param name="vehicle">智能体</param>
		/// <param name="evader">需要拦截的智能体</param>
		/// <returns>操纵力</returns>
		private static float2 Pursuit( ref EntityData entityData, ref MovingData movingData, ref EntityData evaderData, ref MovingData evaderMovingData )
		{
			// if the evader is ahead and facing the agent then we can just seek
			// for the evader's current position.
			var toEvader = evaderData.position - entityData.position;

			var relativeHeading = math.dot( movingData.forward, evaderMovingData.forward );

			// NB acos(0.95) = 18 degs
			if ( math.dot( toEvader, movingData.forward ) > 0 && relativeHeading < -0.95f )
				return Seek( ref entityData, ref movingData, evaderData.position );

			// Not considered ahead so we predict where the evader will be.

			// the lookahead time is propotional to the distance between the evader
			// and the pursuer; and is inversely proportional to the sum of the
			// agent's velocities
			var lookAheadTime = math.length( toEvader ) / ( movingData.maxSpeed + evaderMovingData.speed );

			// now seek to the predicted future position of the evader
			return Seek( ref entityData, ref movingData, evaderData.position + evaderMovingData.velocity * lookAheadTime );
		}

		/// <summary>
		/// 追逐指定的智能体,并可偏移给定的值
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 OffsetPursuit() => default;

		/// <summary>
		/// 逃离指定的智能体
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Evade( ref EntityData entityData, ref MovingData movingData, ref EntityData pursuerData, ref MovingData pursuerMovingData )
		{
			/* Not necessary to include the check for facing direction this time */
			var toPursuer = pursuerData.position - entityData.position;

			// uncomment the following two lines to have Evade only consider pursuers
			// within a 'threat range'
			const float ThreatRange = 36f;
			if ( math.lengthsq( toPursuer ) > ThreatRange * ThreatRange )
				return float2.zero;

			// the lookahead time is propotional to the distance between the pursuer
			// and the pursuer; and is inversely proportional to the sum of the
			// agents' velocities
			var lookAheadTime = math.length( toPursuer ) / ( movingData.maxSpeed + pursuerMovingData.speed );

			// now flee away from predicted future position of the pursuer
			return Flee( ref entityData, ref movingData, pursuerData.position + pursuerMovingData.velocity * lookAheadTime );
		}

		/// <summary>
		/// 徘徊
		/// </summary>
		/// <param name="vehicle">智能体</param>
		/// <returns>操纵力</returns>
		private static float2 Wander( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData )
		{
			// reproject this new vector back on to a unit circle
			// then increase the length of the vector to the same as the radius of the wander circle
			float angle = Environment.random.NextFloat( 0f, math.PI * 2 );
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
		private static float2 ObstacleAvoidance( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData, ref DynamicBuffer<ObstacleElement> obstacles )
		{
			// the detection box length is proportional to the agent's velocity
			var detectionBoxLength = SteeringSettings.MinDetectionBoxLength + ( movingData.speed / movingData.maxSpeed ) * SteeringSettings.MinDetectionBoxLength;

			// this will keep track of the closest intersecting obstacle (CIB)
			var closestIntersectingObstacle = Entity.Null;

			// this will be used to track the distance to the CIB
			var distToClosestIP = float.MaxValue;

			// this will record the transformed local coordinates of the CIB
			var localPosOfClosestObstacle = float2.zero;

			var count = obstacles.Length;
			for ( var i = 0; i < count; i++ )
			{
				var obstacleElement = obstacles[i];
				var obstacleData = Environment.world.EntityManager.GetComponentData<EntityData>( obstacleElement.obstacle );

				// calculate this obstacle's position in local space
				var localPos = TransformUtil.ToLocalSpace( entityData.position, movingData.forward, movingData.right ).Apply( obstacleData.position );

				// if the local position has a negative x value then it must lay
				// behind the agent. (in which case it can be ignored)
				if ( localPos.x >= 0 )
				{
					// if the distance from the x axis to the object's position is less
					// than its radius + half the width of the detection box then there
					// is a potential intersection.
					var expandedRadius = obstacleData.radius + entityData.radius;

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
							distToClosestIP = ip;
							closestIntersectingObstacle = obstacleElement.obstacle;
							localPosOfClosestObstacle = localPos;
						}
					}
				}
			}

			// if we have found an intersecting obstacle, calculate a steering
			// force away from it
			var localSteeringForce = float2.zero;

			if ( closestIntersectingObstacle != Entity.Null )
			{
				var closestIntersectingObstacleData = Environment.world.EntityManager.GetComponentData<EntityData>( closestIntersectingObstacle );

				// the closer the agent is to an object, the stronger the
				// steering force should be
				var multiplier = 1.0f + ( detectionBoxLength - localPosOfClosestObstacle.x ) / detectionBoxLength;

				// apply a braking force proportional to the obstacles distance from the _vehicle.
				const float BrakingWeight = 0.2f;

				localSteeringForce = new float2(
					( closestIntersectingObstacleData.radius - localPosOfClosestObstacle.x ) * BrakingWeight,
					( closestIntersectingObstacleData.radius - localPosOfClosestObstacle.y ) * multiplier );
			}

			// finally, convert the steering vector from local to world space
			return TransformUtil.ToWorldSpace( float2.zero, movingData.forward, movingData.right ).Apply( localSteeringForce );
		}

		/// <summary>
		/// 让智能体保持远离墙
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 WallAvoidance( ref VehicleData vehicleData )
		{
			var steeringForce = float2.zero;
			// if an intersection point has been detected, calculate a force
			// that will direct the agent away
			if ( vehicleData.wallDetectionData.wall != Entity.Null )
			{
				float2 wallDetectionFeeler;

				// calculate by what distance the projected position of the agent will overshoot the wall
				switch ( vehicleData.wallDetectionData.wallDetectionFeelerIndex )
				{
					case 0:
						wallDetectionFeeler = vehicleData.wallDetectionFeeler0;
						break;
					case 1:
						wallDetectionFeeler = vehicleData.wallDetectionFeeler1;
						break;
					default:
						wallDetectionFeeler = vehicleData.wallDetectionFeeler2;
						break;
				}
				var overShoot = wallDetectionFeeler - vehicleData.wallDetectionData.closestPoint;
				// create a force in the direction of the wall normal, with a magnitude of the overshoot
				steeringForce = vehicleData.wallDetectionData.wallNormal * math.length( overShoot );
			}

			return steeringForce;
		}

		/// <summary>
		/// 给定一系列的向量,该方法产生一个力,让智能体沿着给定路径移动
		/// 智能体使用seek行为移动到另一个路点
		/// 到达最后一个路点时,将会使用arrive行为
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 FollowPath()
		{
			//var entityData = Environment.world.EntityManager.GetComponentData<EntityData>( vehicle );
			//var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );

			//// move to next target if close enough to current target
			//// (working in distance squared space)
			//if ( math.lengthsq( path.current - entityData.position ) < vehicleData.waypointSeekDistSquared )
			//	path.MoveNext();

			//if ( !path.isFinished )
			//	return Seek( vehicle, path.current );
			//else
			//	return Arrive( vehicle, path.current, Deceleration.Normal );
			return float2.zero;
		}

		/// <summary>
		/// 计算一个操纵力,让智能体能插入给定的两个智能体的中心位置
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Interpose( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			ref EntityData entityData0, ref MovingData movingData0,
			ref EntityData entityData1, ref MovingData movingData1 )
		{
			// first we need to figure out where the two agents are going to be at
			// time T in the future. This is approximated by determining the time
			// taken to reach the mid way point at the current time at at max speed.
			var midPoint = ( entityData0.position + entityData1.position ) / 2.0f;
			var timeToReachMidPoint = math.distance( entityData.position, midPoint ) / movingData.maxSpeed;

			// now we have T, we assume that agent A and agent B will continue on a
			// straight trajectory and extrapolate to get their future positions
			var aPos = entityData0.position + movingData0.velocity * timeToReachMidPoint;
			var bPos = entityData1.position + movingData1.velocity * timeToReachMidPoint;

			// calculate the mid point of these predicted positions
			midPoint = ( aPos + bPos ) / 2.0f;

			// then steer to Arrive at it
			return Arrive( ref entityData, ref movingData, ref vehicleData, midPoint );
		}

		/// <summary>
		/// 该方法让智能体能隐藏在给定一系列的障碍物后,尽量不被给定的捕猎者找到
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Hide( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData,
			ref EntityData hunterData, ref MovingData hunterMovingData, ref DynamicBuffer<ObstacleElement> obstacles )
		{
			var distToClosest = float.MaxValue;
			var bestHidingSpot = float2.zero;

			for ( int i = 0; i < obstacles.Length; i++ )
			{
				var obstacle = obstacles[i].obstacle;
				var curObData = Environment.world.EntityManager.GetComponentData<ObstacleData>( obstacle );

				// calculate the position of the hiding spot for this obstacle
				var hidingSpot = GetHidingPosition(
				curObData.position,
				curObData.radius,
				hunterData.position );

				// work in distance-squared space to find the closest hiding
				// spot to the agent
				var dist = math.lengthsq( hidingSpot - entityData.position );

				if ( dist < distToClosest )
				{
					distToClosest = dist;
					bestHidingSpot = hidingSpot;
				}
			}

			// if no suitable obstacles found then Evade the hunter,
			// else use Arrive on the hiding spot
			if ( distToClosest == float.MaxValue )
				return Evade( ref entityData, ref movingData, ref hunterData, ref hunterMovingData );
			else
				return Arrive( ref entityData, ref movingData, ref vehicleData, bestHidingSpot );
		}

		/// <summary>
		/// 在给定猎人的位置以及障碍物的位置和半径的情况下，此方法计算远离其边界半径并与猎人正好相反的位置DistanceFromBoundary
		/// </summary>
		/// <param name="posOb">障碍物的位置</param>
		/// <param name="radiusOb">障碍物的半径</param>
		/// <param name="posHunter">猎人的位置</param>
		/// <returns>隐藏位置</returns>
		private static float2 GetHidingPosition( float2 posOb, float radiusOb, float2 posHunter )
		{
			// calculate how far away the agent is to be from the chosen obstacle's
			// bounding radius
			const float distanceFromBoundary = 30.0f;
			var distAway = radiusOb + distanceFromBoundary;

			// calculate the heading toward the object from the hunter
			var toOb = math.normalize( posOb - posHunter );

			// scale it to size and add to the obstacles position to get
			// the hiding spot.
			return ( toOb * distAway ) + posOb;
		}

		/// <summary>
		/// 返回一个转向力，该转向力试图将智能体移动到其紧邻区域的智能体的质心
		/// </summary>
		/// <returns>操纵力</returns>
		private static float2 Cohesion( ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData, ref DynamicBuffer<NeighbourElement> neighbors )
		{
			// first find the center of mass of all the agents
			float2 centerOfMass = float2.zero, steeringForce = float2.zero;
			var neighborCount = 0;

			// iterate through the neighbors and sum up all the position vectors
			var count = neighbors.Length;
			for ( var i = 0; i < count; ++i )
			{
				// make sure *this* agent isn't included in the calculations and that
				// the agent being examined is close enough ***also make sure it doesn't
				// include the evade target ***
				var neighbor = neighbors[i].neighbour;
				var neighborData = Environment.world.EntityManager.GetComponentData<EntityData>( neighbor );
				if ( neighbor != vehicleData.targetAgent1 )
				{
					centerOfMass += neighborData.position;
					++neighborCount;
				}
			}

			if ( neighborCount > 0 )
			{
				// seek towards the center of mass - the average of the sum of positions
				centerOfMass /= neighborCount;
				steeringForce = Seek( ref entityData, ref movingData, centerOfMass );
			}

			// the magnitude of cohesion is usually much larger than separation or
			// alignment so it usually helps to normalize it.
			return math.normalize( steeringForce );
		}

		/// <summary>
		/// 计算与其邻居的斥力
		/// </summary>
		/// <returns>与其邻居的斥力</returns>
		private static float2 Separation( ref EntityData entityData, ref VehicleData vehicleData, ref DynamicBuffer<NeighbourElement> neighbors )
		{
			var steeringForce = float2.zero;
			var count = neighbors.Length;
			for ( var i = 0; i < count; ++i )
			{
				// make sure this agent isn't included in the calculations and that
				// the agent being examined is close enough. ***also make sure it doesn't
				// include the evade target ***
				var neighbor = neighbors[i].neighbour;
				var neighborData = Environment.world.EntityManager.GetComponentData<EntityData>( neighbor );

				if ( neighbor != vehicleData.targetAgent1 )
				{
					var toAgent = entityData.position - neighborData.position;
					var distFromEachOther = math.length( toAgent ) - entityData.radius - neighborData.radius;
					distFromEachOther = distFromEachOther < 0.1f ? 0.1f : distFromEachOther;
					// scale the force inversely proportional to the agents distance
					// from its neighbor.
					steeringForce += toAgent / distFromEachOther;
				}
			}

			return steeringForce;
		}

		/// <summary>
		/// 计算一个力,使其与邻近的智能体保持一个朝向
		/// </summary>
		/// <returns>一个试图使其与邻近的智能体保持一个朝向的力</returns>
		private static float2 Alignment( ref MovingData movingData, ref VehicleData vehicleData, ref DynamicBuffer<NeighbourElement> neighbors )
		{
			// used to record the average heading of the neighbors
			var averageHeading = float2.zero;

			// used to count the number of vehicles in the neighborhood
			var neighborCount = 0;

			// iterate through all the tagged vehicles and sum their heading vectors
			var count = neighbors.Length;
			for ( var i = 0; i < count; ++i )
			{
				// make sure *this* agent isn't included in the calculations and that
				// the agent being examined  is close enough ***also make sure it doesn't
				// include any evade target ***
				var neighbor = neighbors[i].neighbour;
				var neighborMovingData = Environment.world.EntityManager.GetComponentData<MovingData>( neighbor );
				if ( neighbor != vehicleData.targetAgent1 )
				{
					averageHeading += neighborMovingData.forward;
					++neighborCount;
				}
			}

			// if the neighborhood contained one or more vehicles, average their
			// heading vectors.
			if ( neighborCount > 0 )
			{
				averageHeading /= neighborCount;
				averageHeading -= movingData.forward;
			}

			return averageHeading;
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

		public static void PursuitOn( Entity vehicle, Entity v )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Pursuit;
			vehicleData.targetAgent1 = v;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void EvadeOn( Entity vehicle, Entity v )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Evade;
			vehicleData.targetAgent1 = v;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void CohesionOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Cohesion;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void SeparationOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Separation;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void AlignmentOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Alignment;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void ObstacleAvoidanceOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.ObstacleAvoidance;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void WallAvoidanceOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.WallAvoidance;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void FollowPathOn( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.FollowPath;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void InterposeOn( Entity vehicle, Entity v1, Entity v2 )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Interpose;
			vehicleData.targetAgent1 = v1;
			vehicleData.targetAgent2 = v2;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void HideOn( Entity vehicle, Entity v )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.Hide;
			vehicleData.targetAgent1 = v;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void OffsetPursuitOn( Entity vehicle, Entity v1, float2 offset )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags |= Behaviors.OffsetPursuit;
			vehicleData.offset = offset;
			vehicleData.targetAgent1 = v1;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void FlockingOn( Entity vehicle )
		{
			CohesionOn( vehicle );
			AlignmentOn( vehicle );
			SeparationOn( vehicle );
			WanderOn( vehicle );
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

		public static void PursuitOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Pursuit;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void EvadeOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Evade;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void CohesionOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Cohesion;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void SeparationOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Separation;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void AlignmentOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Alignment;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void ObstacleAvoidanceOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.ObstacleAvoidance;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void WallAvoidanceOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.WallAvoidance;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void FollowPathOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.FollowPath;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void InterposeOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Interpose;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void HideOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.Hide;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void OffsetPursuitOff( Entity vehicle )
		{
			var vehicleData = Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle );
			vehicleData.flags &= ~Behaviors.OffsetPursuit;
			Environment.world.EntityManager.SetComponentData( vehicle, vehicleData );
		}

		public static void FlockingOff( Entity vehicle )
		{
			CohesionOff( vehicle );
			AlignmentOff( vehicle );
			SeparationOff( vehicle );
			WanderOff( vehicle );
		}

		public static bool IsFleeOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Flee ) > 0;
		public static bool IsSeekOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Seek ) > 0;
		public static bool IsArriveOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Arrive ) > 0;
		public static bool IsWanderOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Wander ) > 0;
		public static bool IsPursuitOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Pursuit ) > 0;
		public static bool IsEvadeOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Evade ) > 0;
		public static bool IsCohesionOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Cohesion ) > 0;
		public static bool IsSeparationOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Separation ) > 0;
		public static bool IsAlignmentOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Alignment ) > 0;
		public static bool IsObstacleAvoidanceOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.ObstacleAvoidance ) > 0;
		public static bool IsWallAvoidanceOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.WallAvoidance ) > 0;
		public static bool IsFollowPathOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.FollowPath ) > 0;
		public static bool IsInterposeOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Interpose ) > 0;
		public static bool IsHideOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.Hide ) > 0;
		public static bool IsOffsetPursuitOn( Entity vehicle ) => ( Environment.world.EntityManager.GetComponentData<VehicleData>( vehicle ).flags & Behaviors.OffsetPursuit ) > 0;
		#endregion
	}
}
