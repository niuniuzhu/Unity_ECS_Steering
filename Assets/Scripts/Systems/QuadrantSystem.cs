using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Steering
{
	public class QuadrantSystem : SystemBase
	{
		public static NativeMultiHashMap<int, CellEntityElement> cellEntityElementHashMap;

		protected override void OnCreate()
		{
			base.OnCreate();

			cellEntityElementHashMap = new NativeMultiHashMap<int, CellEntityElement>( 0, Allocator.Persistent );
		}

		protected override void OnDestroy()
		{
			cellEntityElementHashMap.Dispose();

			base.OnDestroy();
		}

		protected override void OnUpdate()
		{
			var cellSize = Environment.cellSize;
			var offset = Environment.minXY;
			var numCell = Environment.numCell;

			var entityQuery = this.GetEntityQuery( typeof( VehicleData ) );
			cellEntityElementHashMap.Clear();
			if ( entityQuery.CalculateEntityCount() > cellEntityElementHashMap.Capacity )
				cellEntityElementHashMap.Capacity = entityQuery.CalculateEntityCount();
			var parallelWriter = cellEntityElementHashMap.AsParallelWriter();

			this.Entities.WithAll<VehicleData>().ForEach( ( Entity entity, in EntityData entityData ) =>
			{
				var index = GetCellRawIndex( entityData.position, offset, numCell, cellSize );
				parallelWriter.Add( index, new CellEntityElement
				{
					entity = entity,
					entityData = entityData,
					position = entityData.position,
					radius = entityData.radius
				} );
			} ).ScheduleParallel();
		}

		public static int GetCellRawIndex( float2 position, float2 offset, int2 numCell, float2 cellSize )
		{
			var x = ( int )math.floor( ( position.x - offset.x ) / cellSize.x );
			x = x < 0 ? 0 : ( x >= numCell.x ? numCell.x - 1 : x );
			var y = ( int )math.floor( ( position.y - offset.y ) / cellSize.y );
			y = y < 0 ? 0 : ( y >= numCell.y ? numCell.y - 1 : y );
			return x + y * numCell.x;
		}

		public static int2 GetCellIndex( float2 position, float2 offset, int2 numCell, float2 cellSize )
		{
			var x = ( int )math.floor( ( position.x - offset.x ) / cellSize.x );
			x = x < 0 ? 0 : ( x >= numCell.x ? numCell.x - 1 : x );
			var y = ( int )math.floor( ( position.y - offset.y ) / cellSize.y );
			y = y < 0 ? 0 : ( y >= numCell.y ? numCell.y - 1 : y );
			return new int2( x, y );
		}

		private static int GetEntityCountInHashMap( NativeMultiHashMap<int, Entity> quadrantMuliHashMap, int key )
		{
			int count = 0;
			if ( quadrantMuliHashMap.TryGetFirstValue( key, out _, out var nativeMultiHashMapIterator ) )
			{
				do
				{
					++count;
				}
				while ( quadrantMuliHashMap.TryGetNextValue( out _, ref nativeMultiHashMapIterator ) );
			}
			return count;
		}
	}
}
