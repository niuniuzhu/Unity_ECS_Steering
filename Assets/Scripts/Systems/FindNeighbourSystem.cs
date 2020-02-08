using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Steering
{
	[UpdateAfter( typeof( QuadrantSystem ) )]
	public class FindNeighbourSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			var cellSize = Environment.cellSize;
			var offset = Environment.minXY;
			var numCell = Environment.numCell;
			var cellEntityElementHashMap = QuadrantSystem.cellEntityElementHashMap;
			var cellIndexArray = new NativeArray<int>( Environment.numCell.x * Environment.numCell.y, Allocator.TempJob );

			var jobHandle = Entities.WithAll<VehicleData>().WithReadOnly( cellEntityElementHashMap ).
				ForEach( ( Entity vehicle, ref DynamicBuffer<NeighbourElement> neighbours, in EntityData entityData, in MovingData movingData ) =>
			 {
				 neighbours.Clear();

				 var viewDistance = movingData.viewDistance;
				 //用视野作为半径的圆的外接矩形
				 var topLeft = entityData.position - viewDistance;
				 var bottomRight = entityData.position + viewDistance;

				 var minIndex = QuadrantSystem.GetCellIndex( topLeft, offset, numCell, cellSize );
				 var maxIndex = QuadrantSystem.GetCellIndex( bottomRight, offset, numCell, cellSize );
				 var size = maxIndex - minIndex;

				 var numIndices = ( size.x + 1 ) * ( size.y + 1 );
				 int k = 0;
				 for ( int i = minIndex.y; i <= maxIndex.y; i++ )
					 for ( int j = minIndex.x; j <= maxIndex.x; j++ )
						 cellIndexArray[k++] = j + i * numCell.x;

				 for ( int i = 0; i < numIndices; i++ )
				 {
					 //var it = cellEntityElementHashMap.GetValuesForKey( _cellIndexArray[i] );
					 //while ( it.MoveNext() )
					 //{
					 // var cellEntityElement = it.Current;
					 // if ( cellEntityElement.entity == vehicle )
					 //	 continue;

					 // var to = cellEntityElement.position - entityData.position;

					 // // the bounding radius of the other is taken into account by adding it to the range
					 // float totalRange = viewDistance + cellEntityElement.radius;

					 // // if entity within range, tag for further consideration.
					 // // (working in distance-squared space to avoid sqrts)
					 // if ( math.lengthsq( to ) < totalRange * totalRange )
					 // {
					 //	 if ( neighbours.Length < 10 )
					 //		 neighbours.Add( new NeighbourElement() { neighbour = cellEntityElement.entity } );
					 // }
					 //}
					 if ( cellEntityElementHashMap.TryGetFirstValue( cellIndexArray[i], out var cellEntityElement, out var nativeMultiHashMapIterator ) )
					 {
						 do
						 {
							 if ( cellEntityElement.entity == vehicle )
								 continue;

							 var to = cellEntityElement.position - entityData.position;

							 // the bounding radius of the other is taken into account by adding it to the range
							 float totalRange = viewDistance + cellEntityElement.radius;

							 // if entity within range, tag for further consideration.
							 // (working in distance-squared space to avoid sqrts)
							 if ( math.lengthsq( to ) < totalRange * totalRange )
							 {
								 if ( neighbours.Length < 10 )
									 neighbours.Add( new NeighbourElement() { neighbour = cellEntityElement.entity } );
							 }
						 }
						 while ( cellEntityElementHashMap.TryGetNextValue( out cellEntityElement, ref nativeMultiHashMapIterator ) );
					 }
				 }
			 } ).Schedule( inputDeps );

			cellIndexArray.Dispose( jobHandle );

			return jobHandle;
		}
	}
}
