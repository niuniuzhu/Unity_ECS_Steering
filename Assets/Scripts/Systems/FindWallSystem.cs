using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Steering
{
	public class FindWallSystem : JobComponentSystem
	{
		struct WallInfo
		{
			public Entity entity;
			public float2 from;
			public float2 to;
			public float2 normal;
		}

		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			var targetQuery = this.GetEntityQuery( typeof( WallData ) );
			var targetEntityArray = targetQuery.ToEntityArray( Allocator.TempJob );
			var targetEntityDataArray = targetQuery.ToComponentDataArray<WallData>( Allocator.TempJob );

			var targetInfos = new NativeArray<WallInfo>( targetEntityArray.Length, Allocator.TempJob );
			for ( var i = 0; i < targetInfos.Length; i++ )
				targetInfos[i] = new WallInfo
				{
					entity = targetEntityArray[i],
					from = targetEntityDataArray[i].from,
					to = targetEntityDataArray[i].to,
					normal = targetEntityDataArray[i].normal
				};

			targetEntityArray.Dispose();
			targetEntityDataArray.Dispose();

			var wallDetectionFeelers = new NativeArray<float2>( 3, Allocator.TempJob );

			var jobHandle = Entities.ForEach( ( Entity vehicle, ref EntityData entityData, ref MovingData movingData, ref VehicleData vehicleData ) =>
			 {
				 // feeler pointing straight in front
				 vehicleData.wallDetectionFeeler0 = entityData.position + movingData.forward * vehicleData.wallDetectionFeelerLength;

				 // feeler to left
				 var temp = TransformUtil.Rotation( ( float )System.Math.PI * 1.75f ).Apply( movingData.forward );
				 vehicleData.wallDetectionFeeler1 = entityData.position + vehicleData.wallDetectionFeelerLength / 2f * temp;

				 // feeler to right
				 temp = TransformUtil.Rotation( ( float )System.Math.PI * 0.25f ).Apply( movingData.forward );
				 vehicleData.wallDetectionFeeler2 = entityData.position + vehicleData.wallDetectionFeelerLength / 2f * temp;

				 var distToThisIP = 0.0f;
				 var distToClosestIP = float.MaxValue;
				 var steeringForce = float2.zero;

				 wallDetectionFeelers[0] = vehicleData.wallDetectionFeeler0;
				 wallDetectionFeelers[1] = vehicleData.wallDetectionFeeler1;
				 wallDetectionFeelers[2] = vehicleData.wallDetectionFeeler2;

				 // examine each feeler in turn
				 var count = wallDetectionFeelers.Length;
				 var flr = 0;
				 for ( ; flr < count; ++flr )
				 {
					 // run through each wall checking for any intersection points
					 var c2 = targetInfos.Length;
					 for ( var i = 0; i < c2; i++ )
					 {
						 var targetInfo = targetInfos[i];
						 if ( GeometryUtil.LineSegmentIntersectionPoint(
								entityData.position,
								wallDetectionFeelers[flr],
								targetInfo.from,
								targetInfo.to,
								out var point ) )
						 {
							 // is this the closest found so far? If so keep a record
							 if ( math.lengthsq( entityData.position - point ) < distToClosestIP )
							 {
								 distToClosestIP = distToThisIP;
								 vehicleData.wallDetectionData = new VehicleData.WallDetectionData
								 {
									 wall = targetInfo.entity,
									 wallNormal = targetInfo.normal,
									 closestPoint = point,
									 wallDetectionFeelerIndex = flr
								 };
								 break;
							 }
						 }
					 }
				 }
			 } ).Schedule( inputDeps );

			targetInfos.Dispose( jobHandle );
			wallDetectionFeelers.Dispose( jobHandle );

			return jobHandle;
		}
	}
}
