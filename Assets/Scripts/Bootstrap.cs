using Steering;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Environment = Steering.Environment;

public class Bootstrap : MonoBehaviour
{
	//vehicles
	public GameObject vehiclePrefab;
	[HideInInspector]
	public Mesh mesh;
	public Material material;
	public int amount;
	public float minSpeed;
	public float maxSpeed;
	public float minMass;
	public float maxMass;

	//walls
	public GameObject wallPrefab;
	public float2 minXY;
	public float2 maxXY;
	public float wallBorderSize;

	//quadrant
	public int numCellX;
	public int numCellZ;

	//obstacles
	public Mesh obstacleMesh;
	public Material obstacleMaterial;
	public int numObstacles;
	public float minObstacleRadius;
	public float maxObstacleRadius;
	public float obstacleMinSeparation;
	public float obstacleBorder;

	private void Start()
	{
		Application.targetFrameRate = 60;

		Environment.world = World.DefaultGameObjectInjectionWorld;
		Environment.random.InitState( ( uint )new System.Random().Next() );
		Environment.minXY = this.minXY;
		Environment.maxXY = this.maxXY;
		Environment.numCell = new int2( this.numCellX, this.numCellZ );
		var worldSize = Environment.maxXY - Environment.minXY;
		Environment.cellSize = new float2( worldSize.x / Environment.numCell.x, worldSize.y / Environment.numCell.y );

		this.mesh = CreateMesh();
		this.CreateCells();
		this.CreateWalls();
		this.CreateObstacles();
	}

	private void OnDestroy()
	{
		Destroy( this.mesh );
	}

	public void OnAmountTextEnd( string text )
	{
		this.amount = int.Parse( text );
	}

	public void OnSpawnButtonClick()
	{
		this.SpawnPrefab();
	}

	private void Update()
	{
		this.DebugDrawCells();
	}

	private void CreateCells()
	{
		var manager = Environment.world.EntityManager;

		var archeType = manager.CreateArchetype(
			typeof( CellData )
		);

		int k = 0;
		for ( int i = 0; i < Environment.numCell.y; i++ )
		{
			for ( int j = 0; j < Environment.numCell.x; j++ )
			{
				var entity = manager.CreateEntity( archeType );
				manager.SetComponentData( entity, new CellData
				{
					index = k++,
					center = new float2( Environment.minXY.x + Environment.cellSize.x * 0.5f + Environment.cellSize.x * j,
					Environment.minXY.y + Environment.cellSize.y * 0.5f + Environment.cellSize.y * i ),
					extends = Environment.cellSize * 0.5f
				} );
			}
		}
	}

	private void CreateWalls()
	{
		float2[] wallDatas = new[]
		{
			//right
			new float2(this.maxXY.x+this.wallBorderSize, this.minXY.y),
			new float2(this.minXY.x-this.wallBorderSize, this.minXY.y-this.wallBorderSize),
			//bottom
			new float2(this.maxXY.x+this.wallBorderSize, this.maxXY.y),
			new float2(this.maxXY.x, this.minXY.y),
			//left
			new float2(this.minXY.x-this.wallBorderSize, this.maxXY.y),
			new float2(this.maxXY.x+this.wallBorderSize, this.maxXY.y+this.wallBorderSize),
			//top
			new float2(this.minXY.x-this.wallBorderSize, this.minXY.y),
			new float2(this.minXY.x, this.maxXY.y),
		};

		var manager = Environment.world.EntityManager;

		var blobAssetStore = new BlobAssetStore();
		var setting = GameObjectConversionSettings.FromWorld( Environment.world, blobAssetStore );
		var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy( this.wallPrefab, setting );
		blobAssetStore.Dispose();

		var wallCount = wallDatas.Length / 2;
		var walls = new NativeArray<Entity>( wallCount, Allocator.Temp );
		manager.Instantiate( entityPrefab, walls );
		manager.DestroyEntity( entityPrefab );

		for ( int i = 0; i < wallCount; i++ )
		{
			var wall = walls[i];
			var from = wallDatas[i * 2 + 0];
			var to = wallDatas[i * 2 + 1];
			var size = from - to;
			var normal = math.normalize( size );
			normal = new float2( normal.y, -normal.x );
			var center = ( from + to ) / 2.0f;

			manager.SetComponentData( wall, new WallData
			{
				from = from,
				to = to,
				size = size,
				normal = normal,
				center = center
			} );
			manager.SetComponentData( wall, new Translation { Value = new float3( center.x, 0, center.y ) } );
			manager.SetComponentData( wall, new Rotation { Value = quaternion.identity } );
			manager.AddComponentData( wall, new NonUniformScale { Value = new float3( size.x, 1, size.y ) } );
		}

		walls.Dispose();
	}

	private void CreateObstacles()
	{
		var manager = Environment.world.EntityManager;

		var archeType = manager.CreateArchetype(
			typeof( LocalToWorld ),
			typeof( Translation ),
			typeof( RenderBounds ),
			typeof( WorldRenderBounds ),
			typeof( Scale ),
			typeof( ObstacleData )
		);

		var obstacleDatas = new List<ObstacleData>();
		for ( int i = 0; i < this.numObstacles; ++i )
		{
			// keep creating tiddlywinks until we find one that doesn't overlap
			// any others. Sometimes this can get into an endless loop because the
			// obstacle has nowhere to fit. We test for this case and exit accordingly
			var maxTries = 2000;
			var isOverlapped = true;
			for ( int numTries = 0; isOverlapped; numTries++ )
			{
				if ( numTries > maxTries )
					return;

				var radius = Environment.random.NextFloat( this.minObstacleRadius, this.maxObstacleRadius );

				// TODO: inefficient - do the check before creating the obstacle.
				var position = new float2(
					Environment.random.NextFloat( this.minXY.x + radius + this.obstacleBorder, this.maxXY.x - radius - this.obstacleBorder ),
					Environment.random.NextFloat( this.minXY.y + radius + this.obstacleBorder, this.maxXY.y - radius - this.obstacleBorder ) );

				var obstacleData = new ObstacleData
				{
					position = position,
					radius = radius
				};

				if ( !IsOverlapped( obstacleData, obstacleDatas, this.obstacleMinSeparation ) )
				{
					obstacleDatas.Add( obstacleData );
					isOverlapped = false;

					var entity = manager.CreateEntity( archeType );
					manager.SetComponentData( entity, obstacleData );
					manager.SetComponentData( entity, new Translation { Value = new float3( position.x, 0, position.y ) } );
					manager.SetComponentData( entity, new Scale { Value = radius * 2 } );
					manager.AddSharedComponentData( entity, new RenderMesh
					{
						mesh = this.obstacleMesh,
						material = this.obstacleMaterial,
						castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
						receiveShadows = false
					} );
				}
			}
		}
	}

	private static bool IsOverlapped( in ObstacleData obstacleData, List<ObstacleData> conOb, float minSeparation )
	{
		foreach ( var it in conOb )
		{
			if ( GeometryUtil.TwoCirclesOverlapped(
				obstacleData.position,
				obstacleData.radius + minSeparation,
				it.position,
				it.radius ) )
			{
				return true;
			}
		}
		return false;
	}

	private void SpawnPrefab()
	{
		var blobAssetStore = new BlobAssetStore();
		var setting = GameObjectConversionSettings.FromWorld( Environment.world, blobAssetStore );
		var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy( this.vehiclePrefab, setting );
		blobAssetStore.Dispose();

		var manager = Environment.world.EntityManager;
		var vehicles = new NativeArray<Entity>( this.amount, Allocator.Temp );
		manager.Instantiate( entityPrefab, vehicles );
		manager.DestroyEntity( entityPrefab );

		var rnd = new System.Random();
		for ( int i = 0; i < this.amount; i++ )
		{
			var vehicle = vehicles[i];

			var entityData = manager.GetComponentData<EntityData>( vehicle );
			var mass = ( float )rnd.NextDouble() * ( this.maxMass - this.minMass ) + this.minMass;
			var radius = entityData.radius * mass * 0.5f;
			var wallBorderSize = this.wallBorderSize * 0.5f;
			var position = new Vector3( UnityEngine.Random.Range( this.minXY.x + wallBorderSize + radius, this.maxXY.x - wallBorderSize - radius ), 0,
				UnityEngine.Random.Range( this.minXY.y + wallBorderSize + radius, this.maxXY.y - wallBorderSize - radius ) );
			var rotation = quaternion.AxisAngle( new float3( 0, 1, 0 ), math.radians( UnityEngine.Random.Range( 0, 360 ) ) );

			entityData = new EntityData
			{
				position = new float2( position.x, position.z ),
				radius = radius,
				mass = mass
			};
			manager.SetComponentData( vehicle, entityData );
			manager.SetComponentData( vehicle, new Translation { Value = position } );
			manager.SetComponentData( vehicle, new Rotation { Value = rotation } );
			manager.AddComponent<Scale>( vehicle );
			manager.SetComponentData( vehicle, new Scale { Value = entityData.mass * 0.5f } );

			manager.AddBuffer<NeighbourElement>( vehicle );
			manager.AddBuffer<ObstacleElement>( vehicle );
			manager.AddSharedComponentData( vehicle, new RenderMesh
			{
				mesh = this.mesh,
				material = this.material,
				castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
				receiveShadows = false
			} );
			manager.AddComponent<RenderBounds>( vehicle );
			manager.AddComponent<WorldRenderBounds>( vehicle );
			var forward = math.forward( rotation );
			var movingData = manager.GetComponentData<MovingData>( vehicle );
			movingData.maxSpeed = ( float )rnd.NextDouble() * ( this.maxSpeed - this.minSpeed ) + this.minSpeed;
			movingData.forward = new float2( forward.x, forward.z );
			movingData.right = new float2( forward.z, -forward.x );
			//movingData.velocity = movingData.forward * movingData.maxSpeed;
			manager.SetComponentData( vehicle, movingData );

			var vehicleData = manager.GetComponentData<VehicleData>( vehicle );
			SteeringSystem.ObstacleAvoidanceOn( ref vehicleData );
			manager.SetComponentData( vehicle, vehicleData );
		}

		vehicles.Dispose();
	}

	private static Mesh CreateMesh()
	{
		var vertices = new Vector3[3];
		vertices[0] = new Vector3( -0.25f, 0, -0.25f );
		vertices[1] = new Vector3( 0f, 0, 0.5f );
		vertices[2] = new Vector3( 0.25f, 0, -0.25f );
		var normals = new Vector3[3];
		normals[0] = new Vector3( 0, 1, 0 );
		normals[1] = new Vector3( 0, 1, 0 );
		normals[2] = new Vector3( 0, 1, 0 );
		var triangles = new int[3];
		triangles[0] = 0;
		triangles[1] = 1;
		triangles[2] = 2;
		var mesh = new Mesh { vertices = vertices, normals = normals, triangles = triangles };
		return mesh;
	}

	private void DebugDrawCells()
	{
		//var manager = Environment.world.EntityManager;
		//var cellQuery = manager.CreateEntityQuery( typeof( CellData ) );
		//var cellDataArray = cellQuery.ToComponentDataArray<CellData>( Allocator.Temp );
		//var count = cellDataArray.Length;
		//for ( int i = 0; i < count; i++ )
		//{
		//	var cellData = cellDataArray[i];
		//	var min = cellData.center - cellData.extends;
		//	var max = cellData.center + cellData.extends;
		//	Debug.DrawLine( new Vector3( min.x, 0, min.y ), new Vector3( min.x, 0, max.y ) );
		//	Debug.DrawLine( new Vector3( min.x, 0, min.y ), new Vector3( max.x, 0, min.y ) );
		//	Debug.DrawLine( new Vector3( max.x, 0, max.y ), new Vector3( min.x, 0, max.y ) );
		//	Debug.DrawLine( new Vector3( max.x, 0, max.y ), new Vector3( max.x, 0, min.y ) );
		//}
		//cellDataArray.Dispose();
	}
}
