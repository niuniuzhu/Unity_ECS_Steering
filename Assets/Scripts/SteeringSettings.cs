namespace Steering
{
	public static class SteeringSettings
	{
		/// <summary>
		/// 用于避障转向行为的检测盒的长度
		/// </summary>
		public const float MinDetectionBoxLength = 1.0f;

		/// <summary>
		/// Gets the length of the feelers to use in the wall avoidance steering behaviour.
		/// </summary>
		public const float WallDetectionFeelerLength = 1.0f;

		/// <summary>
		/// Gets the multiplier to apply to the steering force AND all the multipliers
		/// found in SteeringBehavior.
		/// </summary>
		public const float SteeringForceTweaker = 1f;

		/// <summary>
		/// Gets the weighting to apply to the "separation" steering behaviour.
		/// </summary>
		public const float SeparationWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "alignment" steering behaviour.
		/// </summary>
		public const float AlignmentWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "cohesion" steering behaviour.
		/// </summary>
		public const float CohesionWeight = 2.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "obstacle" steering behaviour.
		/// </summary>
		public const float ObstacleAvoidanceWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "wall avoidance" steering behaviour.
		/// </summary>
		public const float WallAvoidanceWeight = 10.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "wander" steering behaviour.
		/// </summary>
		public const float WanderWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "seek" steering behaviour.
		/// </summary>
		public const float SeekWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "flee" steering behaviour.
		/// </summary>
		public const float FleeWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "arrive" steering behaviour.
		/// </summary>
		public const float ArriveWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "pursuit" steering behaviour.
		/// </summary>
		public const float PursuitWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "offset pursuit" steering behaviour.
		/// </summary>
		public const float OffsetPursuitWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "interpose" steering behaviour.
		/// </summary>
		public const float InterposeWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "hide" steering behaviour.
		/// </summary>
		public const float HideWeight = 1.0f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "evade" steering behaviour.
		/// </summary>
		public const float EvadeWeight = 0.01f * SteeringForceTweaker;

		/// <summary>
		/// Gets the weighting to apply to the "follow path" steering behaviour.
		/// </summary>
		public const float FollowPathWeight = 0.05f * SteeringForceTweaker;

		/// <summary>
		/// Gets the probability that the 'wall avoidance' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrWallAvoidance = 0.5f;

		/// <summary>
		/// Gets the probability that the 'obstacle avoidance' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrObstacleAvoidance = 0.5f;

		/// <summary>
		/// Gets the probability that the 'separation' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrSeparation = 0.2f;

		/// <summary>
		/// Gets the probability that the 'flee' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrFlee = 0.6f;

		/// <summary>
		/// Gets the probability that the 'evade' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrEvade = 1.0f;

		/// <summary>
		/// Gets the probability that the 'hide' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrHide = 0.8f;

		/// <summary>
		/// Gets the probability that the 'arrive' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrArrive = 0.5f;

		/// <summary>
		/// Gets the probability that the 'alignment' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrAlignment = 0.3f;

		/// <summary>
		/// Gets the probability that the 'cohesion' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrCohesion = 0.6f;

		/// <summary>
		/// Gets the probability that the 'wander' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrWander = 0.8f;

		/// <summary>
		/// Gets the probability that the 'seek' steering behaviour will be used when the prioritized dither method is used to sum behaviours.
		/// </summary>
		public const float PrSeek = 0.8f;
	}
}
