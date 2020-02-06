using Steering;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

//class MyCustomBootStrap : ICustomBootstrap
//{
//	public bool Initialize( string defaultWorldName )
//	{
//		Debug.Log( "Executing bootstrap" );
//		var world = new World( "Custom world" );
//		World.DefaultGameObjectInjectionWorld = world;
//		var systems = DefaultWorldInitialization.GetAllSystems( WorldSystemFilterFlags.Default );

//		DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups( world, systems );
//		ScriptBehaviourUpdateOrder.UpdatePlayerLoop( world );
//		return true;
//	}
//}

public class Bootstrap : MonoBehaviour
{
	public GameObject prefab;
	public Mesh mesh;
	public Material material;
	public int amount;

	//[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
	private void Start()
	{
		this.mesh = CreateMesh();
	}

	private void OnDestroy()
	{
		Object.Destroy( this.mesh );
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Z ) )
		{
			this.Spawn();
		}
		if ( Input.GetKeyDown( KeyCode.X ) )
		{
			this.SpawnPrefab();
		}
	}

	private void Spawn()
	{
		var manager = Environment.world.EntityManager;

		var archeType = manager.CreateArchetype(
			typeof( RenderMesh ),
			typeof( LocalToWorld ),
			typeof( Translation ),
			typeof( Rotation )
		);

		var charactors = new NativeArray<Entity>( this.amount, Allocator.Temp );
		manager.CreateEntity( archeType, charactors );

		for ( int i = 0; i < this.amount; i++ )
		{
			var position = new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -4f, 4f ) );
			var rotation = quaternion.AxisAngle( new float3( 0, 1, 0 ), math.radians( UnityEngine.Random.Range( 0, 360 ) ) );
			manager.SetComponentData( charactors[i], new Translation { Value = position } );
			manager.SetComponentData( charactors[i], new Rotation { Value = rotation } );
			manager.SetSharedComponentData( charactors[i], new RenderMesh { mesh = this.mesh, material = this.material, castShadows = UnityEngine.Rendering.ShadowCastingMode.Off } );
		}

		charactors.Dispose();
	}

	private void SpawnPrefab()
	{
		var manager = Environment.world.EntityManager;

		var setting = GameObjectConversionSettings.FromWorld( Environment.world, new BlobAssetStore() );
		var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy( this.prefab, setting );

		var charactors = new NativeArray<Entity>( this.amount, Allocator.Temp );
		manager.Instantiate( entityPrefab, charactors );

		manager.DestroyEntity( entityPrefab );

		for ( int i = 0; i < this.amount; i++ )
		{
			var position = new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -4f, 4f ) );
			var rotation = quaternion.AxisAngle( new float3( 0, 1, 0 ), math.radians( UnityEngine.Random.Range( 0, 360 ) ) );
			var forward = math.forward( rotation );

			manager.AddBuffer<NeighbourElement>( charactors[i] );
			manager.AddBuffer<ObstacleElement>( charactors[i] );
			manager.SetComponentData( charactors[i], new Translation { Value = position } );
			manager.SetComponentData( charactors[i], new Rotation { Value = rotation } );
			manager.AddSharedComponentData( charactors[i], new RenderMesh { mesh = this.mesh, material = this.material, castShadows = UnityEngine.Rendering.ShadowCastingMode.Off } );
			manager.SetComponentData( charactors[i], new EntityData { position = new float2( position.x, position.z ) } );
			var movingData = manager.GetComponentData<MovingData>( charactors[i] );
			movingData.forward = new float2( forward.x, forward.z );
			movingData.right = new float2( forward.z, -forward.x );
			movingData.velocity = movingData.forward;
			manager.SetComponentData( charactors[i], movingData );
		}

		charactors.Dispose();
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
}
