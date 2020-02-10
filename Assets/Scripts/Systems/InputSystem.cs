using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Steering
{
	public class InputSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			if ( Input.GetMouseButton( 0 ) )
			{
				var ray = Camera.main.ScreenPointToRay( Input.mousePosition );
				var origin = ray.origin;
				var end = ray.GetPoint( 999 );

				var input = new RaycastInput { Start = origin, End = end, Filter = CollisionFilter.Default };
				var physicsWorld = Environment.world.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
				if ( physicsWorld.CastRay( input, out var hit ) )
				{
					Entities.ForEach( ( Entity vehicle, ref VehicleData vehicleData ) =>
					 {
						 vehicleData.targetPosition = new Unity.Mathematics.float2( hit.Position.x, hit.Position.z );
						 SteeringSystem.ArriveOn( vehicle );
					 } );
				}
			}
		}
	}
}
