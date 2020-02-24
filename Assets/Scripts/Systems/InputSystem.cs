using System;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steering
{
	public class InputSystem : ComponentSystem
	{
		private PlayerInput _inputAction;
		private Vector2 _touchPosition;
		private bool _hasTouch;

		protected override void OnCreate()
		{
			this._inputAction = new PlayerInput();
			this._inputAction.Player.Fire.performed += this.OnFireAction;
			this._inputAction.Player.Fire.Enable();
		}

		protected override void OnDestroy()
		{
			this._inputAction.Player.Fire.performed -= this.OnFireAction;
			this._inputAction.Player.Fire.Disable();
		}

		private void OnFireAction( InputAction.CallbackContext context )
		{
			this._touchPosition = Pointer.current.position.ReadValue();
			this._hasTouch = true;
			//this.DoAction( this._touchPosition );
		}

		protected override void OnUpdate()
		{
			//if ( Input.GetMouseButton( 0 ) )
			//{
			//	this.DoAction( Input.mousePosition );
			//}
			if ( !this._hasTouch )
				return;
			this._hasTouch = false;
			this.DoAction( this._touchPosition );
		}

		private void DoAction( Vector3 touchPosition )
		{
			var ray = Camera.main.ScreenPointToRay( touchPosition );
			var origin = ray.origin;
			var end = ray.GetPoint( 999 );

			var input = new RaycastInput { Start = origin, End = end, Filter = CollisionFilter.Default };
			var physicsWorld = Environment.world.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
			if ( physicsWorld.CastRay( input, out var hit ) )
			{
				Entities.ForEach( ( Entity vehicle, ref VehicleData vehicleData ) =>
				{
					vehicleData.targetPosition = new Unity.Mathematics.float2( hit.Position.x, hit.Position.z );
					SteeringSystem.ArriveOn( ref vehicleData );
				} );
			}
		}
	}
}
