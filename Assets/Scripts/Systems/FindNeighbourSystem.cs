using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Steering
{
	public class FindNeighbourSystem : JobComponentSystem
	{
		struct TargetInfo
		{
			public Entity entity;
			public float2 position;
			public float radius;
		}

		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			var targetQuery = this.GetEntityQuery( typeof( EntityData ), typeof( VehicleData ) );
			var targetEntityArray = targetQuery.ToEntityArray( Allocator.TempJob );
			var targetEntityDataArray = targetQuery.ToComponentDataArray<EntityData>( Allocator.TempJob );

			var targetInfos = new NativeArray<TargetInfo>( targetEntityArray.Length, Allocator.TempJob );
			for ( int i = 0; i < targetInfos.Length; i++ )
				targetInfos[i] = new TargetInfo { entity = targetEntityArray[i], position = targetEntityDataArray[i].position, radius = targetEntityDataArray[i].radius };

			targetEntityArray.Dispose();
			targetEntityDataArray.Dispose();

			var jobHandle = Entities.WithAll<VehicleData>().ForEach( ( Entity vehicle, ref EntityData entityData, ref MovingData movingData, ref DynamicBuffer<NeighbourElement> neighbours ) =>
			 {
				 neighbours.Clear();

				 var viewDistance = movingData.viewDistance;
				 var count = targetInfos.Length;
				 for ( int i = 0; i < count; i++ )
				 {
					 var targetInfo = targetInfos[i];

					 if ( targetInfo.entity == vehicle )
						 continue;

					 var to = targetInfo.position - entityData.position;

					 // the bounding radius of the other is taken into account by adding it to the range
					 float totalRange = viewDistance + targetInfo.radius;

					 // if entity within range, tag for further consideration.
					 // (working in distance-squared space to avoid sqrts)
					 if ( math.lengthsq( to ) < totalRange * totalRange )
					 {
						 if ( neighbours.Length < 10 )
							 neighbours.Add( new NeighbourElement() { neighbour = targetInfo.entity } );
					 }
				 }
			 } ).Schedule( inputDeps );

			targetInfos.Dispose( jobHandle );

			return jobHandle;
		}
	}
}
