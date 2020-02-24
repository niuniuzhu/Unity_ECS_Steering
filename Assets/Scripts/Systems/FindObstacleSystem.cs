using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Steering
{
	public class FindObstacleSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			var targetQuery = this.GetEntityQuery( typeof( ObstacleData ) );
			var targetEntityDataArray = targetQuery.ToComponentDataArray<ObstacleData>( Allocator.TempJob );

			var targetInfos = new NativeArray<ObstacleData>( targetEntityDataArray.Length, Allocator.TempJob );
			for ( int i = 0; i < targetInfos.Length; i++ )
				targetInfos[i] = new ObstacleData { position = targetEntityDataArray[i].position, radius = targetEntityDataArray[i].radius };

			targetEntityDataArray.Dispose();

			var jobHandle = Entities.ForEach( ( Entity vehicle, ref DynamicBuffer<ObstacleElement> obstacles,
				in EntityData entityData, in MovingData movingData, in VehicleData vehicleData ) =>
			 {
				 obstacles.Clear();

				 var detectionBoxLength = entityData.radius + SteeringSettings.MinDetectionBoxLength + ( movingData.speed / movingData.maxSpeed ) * SteeringSettings.MinDetectionBoxLength;

				 for ( int i = 0; i < targetInfos.Length; i++ )
				 {
					 var targetInfo = targetInfos[i];

					 var to = targetInfo.position - entityData.position;

					 // the bounding radius of the other is taken into account by adding it to the range
					 float totalRange = detectionBoxLength + targetInfo.radius;

					 // if entity within range, tag for further consideration.
					 // (working in distance-squared space to avoid sqrts)
					 if ( math.lengthsq( to ) < totalRange * totalRange )
					 {
						 if ( obstacles.Length < 10 )
							 obstacles.Add( new ObstacleElement()
							 {
								 position = targetInfo.position,
								 radius = targetInfo.radius
							 } );
					 }
				 }
			 } ).Schedule( inputDeps );

			targetInfos.Dispose( jobHandle );

			return jobHandle;
		}
	}
}
