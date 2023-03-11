using Fraktalia.VoxelGen;
using Fraktalia.VoxelGen.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fraktalia.Core.Math;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InfinityVoxelSystem : MonoBehaviour
{
	public InfinityVoxel_Chunk ChunkTemplate;

	private List<List<InfinityVoxel_Chunk>> PrioritizedChunks = new List<List<InfinityVoxel_Chunk>>();

	[NonSerialized]
	public List<InfinityVoxel_Chunk> ExistingChunks;

	

	public Stack<InfinityVoxel_Chunk> UnloadedChunks;	

	public List<InfinityVoxel_ChunkLoader> ChunkLoaders;

	[NonSerialized]
	public List<InfinityVoxel_ChunkLoader> DirtyChunkLoaders;

	public VoxelSaveSystem SaveSystem;
	public bool EditorSaveOnUnload;
	public bool SaveChunkOnUnload;

	public WorldGenerator ProceduralGenerator;

	public int LoadContainerWidth = 100;

	public int UpdateSpeed_Data = 8;
	public int UpdateSpeed_Trees = 8;
	public int UpdateSpeed_Hulls = 8;

	public bool CullBorders;

	[Header("LOD Settings")]
	public bool EnableLOD;
	public float LODMultiplier;

	[Space]
	public bool InitializeOnStart;

	[Header("Debug Functionality")]
	public bool ShowDebugInfo;
	public Color DebugColor_Core;
	public Color DebugColor_HasBorder;
	public Color DebugColor_Border;
	public Color DebugColor_HullDirty;
	public Color DebugColor_TreeDirty;
	public bool DebugBox;
	public bool DebugSpheres;

	private float VolumeSize;

	[NonSerialized]
	public bool Initialized;



	private GameObject chunkContainer;
	
	


	private IEnumerator ActiveRoutine;

	[NonSerialized]
	public InfinityVoxel_Chunk[] ChunkLayoutMap;

	private void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if (PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.Regular)
		{
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);			
		}
#endif
		if (!Initialized)
		{
			if(ChunkTemplate)
			{
				VolumeSize = ChunkTemplate.Generator.RootSize;
			}

			for (int i = 0; i < ChunkLoaders.Count; i++)
			{
				if(ChunkLoaders[i])
				ChunkLoaders[i]._DrawPreview(this);
			}

			return;
		}
#if UNITY_EDITOR
		if (EditorApplication.isCompiling)
		{
			_CleanUp();
			return;
		}
#endif

		


		if (ShowDebugInfo)
		{
			for (int i = 0; i < PrioritizedChunks.Count; i++)
			{
				List<InfinityVoxel_Chunk> LoadedChunks = PrioritizedChunks[i];

				for (int k = 0; k < LoadedChunks.Count; k++)
				{
					InfinityVoxel_Chunk chunk = LoadedChunks[k];

					if (chunk.CurrentChunkState == InfinityVoxel_ChunkState.Border)
					{
						Gizmos.color = DebugColor_Border;
					}

					if (chunk.CurrentChunkState == InfinityVoxel_ChunkState.HasBorder)
					{
						Gizmos.color = DebugColor_HasBorder;
					}

					if (chunk.CurrentChunkState == InfinityVoxel_ChunkState.Core)
					{
						Gizmos.color = DebugColor_Core;
					}

					if (chunk.Generator.HullsDirty)
					{
						Gizmos.color = DebugColor_HullDirty;
					}

					if (chunk.Generator.VoxelTreeDirty)
					{
						Gizmos.color = DebugColor_TreeDirty;
					}

					//Gizmos.matrix = transform.localToWorldMatrix;

					Vector3 worldposition = transform.localToWorldMatrix.MultiplyPoint3x4(chunk.transform.position);

					if(DebugBox)
					Gizmos.DrawWireCube(chunk.transform.position + new Vector3(VolumeSize, VolumeSize, VolumeSize) / 2, new Vector3(VolumeSize, VolumeSize, VolumeSize));

					if(DebugSpheres)
					Gizmos.DrawSphere(chunk.transform.position + new Vector3(VolumeSize, VolumeSize, VolumeSize) / 2, VolumeSize/10);

				}

			}
		}
		


		Update();
	}


	public void _Generate()
	{
		ChunkTemplate.Generator.CleanUp();

		chunkContainer = new GameObject("__CHUNKROOT");
		chunkContainer.AddComponent<InfinityVoxel_ChunkRoot>();
		chunkContainer.transform.SetParent(transform);
		chunkContainer.transform.localPosition = Vector3.zero;
		chunkContainer.transform.localRotation = Quaternion.identity;
		chunkContainer.transform.localScale = Vector3.one;
		chunkContainer.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
		VolumeSize = ChunkTemplate.Generator.RootSize;

		PrioritizedChunks = new List<List<InfinityVoxel_Chunk>>();
		for (int i = 0; i < 10; i++)
		{
			PrioritizedChunks.Add(new List<InfinityVoxel_Chunk>());
		}

		ChunkLayoutMap = new InfinityVoxel_Chunk[LoadContainerWidth * LoadContainerWidth * LoadContainerWidth];
		ExistingChunks = new List<InfinityVoxel_Chunk>();
		UnloadedChunks = new Stack<InfinityVoxel_Chunk>();

		
		if(ProceduralGenerator)
		{
			ProceduralGenerator.Initialize(ChunkTemplate.Generator);
		}


		for (int i = 0; i < ChunkLoaders.Count; i++)
		{
			if(ChunkLoaders[i])
			ChunkLoaders[i]._Initialize();
		}

		DirtyChunkLoaders = new List<InfinityVoxel_ChunkLoader>();

		Initialized = true;
		ActiveRoutine = processInfinity();
		
	}

	public void SaveActiveChunks()
	{
		for (int i = 0; i < PrioritizedChunks.Count; i++)
		{
			List<InfinityVoxel_Chunk> LoadedChunks = PrioritizedChunks[i];
			for (int k = 0; k < LoadedChunks.Count; k++)
			{
				InfinityVoxel_Chunk chunk = LoadedChunks[k];

				if (chunk.CurrentChunkState != InfinityVoxel_ChunkState.Unloaded)
				{
					if (chunk.HasVoxelData)
						saveChunk(chunk);
				}
			}
		}
	}

	public void _CleanUp()
	{
		

		Initialized = false;

		

		int count = 0;
		while(ActiveRoutine != null)
		{
			if (count > 10000) break;
			count++;
			ActiveRoutine.MoveNext();
		}

		if (ProceduralGenerator)
		{
			while(ProceduralGenerator.workStack.Count > 0)
			{
				ProceduralGenerator.UpdateRoutines();
				
			}		

			ProceduralGenerator.CleanUp();
		}

		for (int i = 0; i < ChunkLoaders.Count; i++)
		{
			if(ChunkLoaders[i])
			ChunkLoaders[i]._CleanUp();
		}		

		for (int i = 0; i < PrioritizedChunks.Count; i++)
		{
			List<InfinityVoxel_Chunk> LoadedChunks = PrioritizedChunks[i];
			for (int k = 0; k < LoadedChunks.Count; k++)
			{
				InfinityVoxel_Chunk chunk = LoadedChunks[k];
				chunk._CleanUp();
			}
		}

		PrioritizedChunks.Clear();

		if (ChunkLayoutMap != null)
		ChunkLayoutMap = new InfinityVoxel_Chunk[0];

		var root = GetComponentsInChildren<InfinityVoxel_ChunkRoot>();
		for (int i = 0; i < root.Length; i++)
		{
			DestroyImmediate(root[i].gameObject);
		}
	}

	private void Update()
	{
		if (Initialized)
		{
			ActiveRoutine.MoveNext();
		}
	}

	private void Start()
	{
		if(InitializeOnStart)
		{
			_Generate();

			for (int i = 0; i < 1000; i++)
			{
				if (Initialized)
					ActiveRoutine.MoveNext();
			}
		}



	}

	private IEnumerator processInfinity()
	{
		int i;
		int k;

		int updates = 0;
		while (Initialized)
		{
			#region Update Chunklayout

			DirtyChunkLoaders.Clear();

			bool chunkchanged = false;
			for (i = 0; i < ChunkLoaders.Count; i++)
			{
				if (ChunkLoaders[i] == null)
				{
					ChunkLoaders.RemoveAt(i);
					i--;
					chunkchanged = true;
				}
				else
				if (ChunkLoaders[i]._PositionChanged(this))
				{
					chunkchanged = true;
					DirtyChunkLoaders.Add(ChunkLoaders[i]);
				}
			}


			if (chunkchanged)
			{

				for (i = 0; i < PrioritizedChunks.Count; i++)
				{
					PrioritizedChunks[i].Clear();
				}


				for (k = 0; k < ExistingChunks.Count; k++)
				{
					InfinityVoxel_Chunk chunk = ExistingChunks[k];
					if (chunk.CurrentChunkState != InfinityVoxel_ChunkState.Unloaded)
					{
						if (!chunk.IsInsideChunks())
						{
							chunk.IsInPriorityList = false;
							unloadChunk(chunk);
						}
						else
						{
							chunk.IsInPriorityList = true;
							PrioritizedChunks[chunk.ChunkInformation.Priority].Add(chunk);
						}
					}
				}

				for (i = 0; i < DirtyChunkLoaders.Count; i++)
				{
					InfinityVoxel_ChunkLoader loader = DirtyChunkLoaders[i];
					loader._DefineChunks(this);
					List<ChunkManifest> manifests = loader.Chunks;

					for (int y = 0; y < manifests.Count; y++)
					{
						InfinityVoxel_Chunk chunk = obtainChunk(manifests[y], loader);
						chunk.AddChunkLoaderReference(manifests[y], loader);
						chunk.WasUpdated = true;
					}
				}

				for (i = 0; i < ExistingChunks.Count; i++)
				{
					var chunk = ExistingChunks[i];
					if (chunk.WasUpdated)
					{
						chunk.ChunkInformation = ChunkLoaderReference.CombineManifest(chunk.referencedChunkLoaders);
						
					}
				}

				for (i = 0; i < ExistingChunks.Count; i++)
				{
					var chunk = ExistingChunks[i];
					if (chunk.WasUpdated)
					{
						setNeighbours(ExistingChunks[i]);
					}
				}

				int count = 0;
				for (i = 0; i < ExistingChunks.Count; i++)
				{
					var chunk = ExistingChunks[i];
					if (chunk.WasUpdated)
					{
						count++;

						chunk.UpdateContent();

						if (!chunk.IsInPriorityList)
						{
							PrioritizedChunks[chunk.ChunkInformation.Priority].Add(chunk);
						}

						chunk.WasUpdated = false;
					}
				}
			}

			#endregion

			#region Update Content and Generators

			for (i = 0; i < PrioritizedChunks.Count; i++)
			{
				List<InfinityVoxel_Chunk> LoadedChunks = PrioritizedChunks[i];
				for (k = 0; k < LoadedChunks.Count; k++)
				{
					InfinityVoxel_Chunk chunk = LoadedChunks[k];

					#region WORLDGENERATION AND LOADING				
					if (chunk.neednewdata)
					{
						chunk.Generator.RestrictDimension[0] = false;
						if (SaveSystem)
						{
							setSaveInformation(chunk);
						}

						if (SaveSystem && SaveSystem.SaveDataExists)
						{
							SaveSystem.Load();
						}
						else if (ProceduralGenerator)
						{
							ProceduralGenerator.Generate(chunk.Generator);
						}
						else
						{
							chunk.Generator._SetVoxel(new Vector3(), 0, (byte)ChunkTemplate.Generator.InitialValue);
						}

						updates = 0;
						if (ProceduralGenerator)
						{
							while (ProceduralGenerator.workStack.Count > 0)
							{
								ProceduralGenerator.UpdateRoutines();
								updates++;

								if(updates > UpdateSpeed_Data)
								{
									yield return null;
									updates = 0;
								}
								
							}
						}

						if (SaveSystem)
						{
							while (SaveSystem.IsWorking)
							{
								SaveSystem.UpdateRoutines();
								updates++;

								if (updates > UpdateSpeed_Data)
								{
									yield return null;
									updates = 0;
								}
							}
						}

						chunk.HasVoxelData = true;
						chunk.neednewdata = false;

						
					}

					if (chunk.needsreset)
					{
						if (chunk.HasVoxelData)
						{
							saveChunk(chunk);
							chunk.HasVoxelData = false;
						}
						chunk.Generator.ResetData();
						chunk.Generator.RestrictDimension[0] = true;

						chunk.needsreset = false;
					}

					if (chunk.needsrebuild)
					{
						chunk.Generator.Rebuild();
						chunk.needsrebuild = false;
						for (int j = 0; j < chunk.Generator.hullGenerators.Length; j++)
						{
							chunk.Generator.hullGenerators[j].NoPostProcess = true;
						}
					}

					#endregion

								
					chunk.UpdateRegions();
				}
			}
				
			bool prioritycomplete = true;

			updates = 0;
			for (i = 0; i < PrioritizedChunks.Count; i++)
			{
				List<InfinityVoxel_Chunk> LoadedChunks = PrioritizedChunks[i];

				for (k = 0; k < LoadedChunks.Count; k++)
				{
					InfinityVoxel_Chunk chunk = LoadedChunks[k];
					
					if (chunk.Generator.VoxelTreeDirty)
					{
						updates++;
						prioritycomplete = false;
						while (chunk.Generator.VoxelTreeDirty)
						{
							chunk.Generator.LockHullUpdates = true;
							chunk.UpdateTrees();

							if (updates >= UpdateSpeed_Hulls)
							{
								yield return null;
								updates = 0;
							}
						}
					}

					if (updates >= UpdateSpeed_Hulls)
					{
						yield return null;
						updates = 0;
					}
				}

				if (!prioritycomplete)
				{
					prioritycomplete = true;
					i--;
				}

				if (updates >= UpdateSpeed_Hulls)
				{
					yield return null;
					updates = 0;
				}
			}

			for (i = 0; i < PrioritizedChunks.Count; i++)
			{
				List<InfinityVoxel_Chunk> LoadedChunks = PrioritizedChunks[i];

				for (k = 0; k < LoadedChunks.Count; k++)
				{
					InfinityVoxel_Chunk chunk = LoadedChunks[k];

					if (chunk.Generator.HullsDirty)
					{
						updates++;
						prioritycomplete = false;
						while (chunk.Generator.HullsDirty)
						{
							chunk.Generator.LockHullUpdates = false;
							chunk.UpdateTrees();

							if (updates >= UpdateSpeed_Hulls)
							{
								yield return null;
								updates = 0;
							}
						}
					}

					if (updates >= UpdateSpeed_Hulls)
					{
						yield return null;
						updates = 0;
					}
				}

				if (!prioritycomplete)
				{
					prioritycomplete = true;
					i--;
				}

				if (updates >= UpdateSpeed_Hulls)
				{
					yield return null;
					updates = 0;
				}
			}


			#endregion

			yield return null;
		}

		ActiveRoutine = null;
	}

	public Vector3Int _WorldPositionToChunk(Vector3 worldposition)
	{
		Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(worldposition); 

		Vector3 position = localPosition / VolumeSize;
		return new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z));
	}

	public Vector3 _ChunkToWorldposition(Vector3Int chunk)
	{
		Vector3 localPosition = new Vector3(chunk.x * VolumeSize, chunk.y * VolumeSize, chunk.z * VolumeSize);

		return transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);
	}

	private InfinityVoxel_Chunk obtainChunk(ChunkManifest manifest, InfinityVoxel_ChunkLoader loader)
	{
		InfinityVoxel_Chunk chunk = null;
		if(GetLoadedChunk(manifest.ChunkPosition.x, manifest.ChunkPosition.y, manifest.ChunkPosition.z, out chunk))
		{
			return chunk;
		}
		else if (UnloadedChunks.Count > 0)
		{
			chunk = UnloadedChunks.Pop();
			chunk.transform.position = _ChunkToWorldposition(manifest.ChunkPosition);

			chunk.CurrentChunkState = InfinityVoxel_ChunkState.Loaded;
			
			AddLoadedChunk(manifest.ChunkPosition.x, manifest.ChunkPosition.y, manifest.ChunkPosition.z, chunk);
		}
		else 
		{
			chunk = Instantiate(ChunkTemplate, chunkContainer.transform);
			chunk.transform.position = _ChunkToWorldposition(manifest.ChunkPosition);
		
			chunk.Initialize();			
			chunk.Infinity = this;
			chunk.CurrentChunkState = InfinityVoxel_ChunkState.Loaded;
			chunk.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
			AddLoadedChunk(manifest.ChunkPosition.x, manifest.ChunkPosition.y, manifest.ChunkPosition.z, chunk);
			ExistingChunks.Add(chunk);

		}

		return chunk;
	}

	private void setNeighbours(InfinityVoxel_Chunk centerchunk)
	{
		if (centerchunk.NeighbourCount == 28) return;

		Vector3Int centerPosition = centerchunk.ChunkInformation.ChunkPosition;

		int center_x = centerPosition.x;
		int center_y = centerPosition.y;
		int center_z = centerPosition.z;

		InfinityVoxel_Chunk neighbour = null;

		if (centerchunk.CurrentChunkState == InfinityVoxel_ChunkState.Border || centerchunk.CurrentChunkState == InfinityVoxel_ChunkState.Loaded)
		{
			int index = 0;
			

			for (int z = -1; z <= 1; z++)
			{
				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						if (centerchunk.Neighbours[index] == null)
						{													
							if (GetLoadedChunk(center_x + x, center_y + y, center_z + z, out neighbour))
							{
								centerchunk.AddNeighbour(index, neighbour);
							}													
						}

						index++;
					}
				}
			}		
		}
	}

	private void removeAllNeighbours(InfinityVoxel_Chunk centerchunk)
	{
		for (int i = 0; i < centerchunk.Neighbours.Length; i++)
		{
			if (centerchunk.Neighbours[i])
			{
				centerchunk.RemoveNeighbour(i);
			}		
		}
	}

	public void saveChunk(InfinityVoxel_Chunk chunk)
	{
		if (SaveSystem)
		{		
			setSaveInformation(chunk);
			SaveSystem.IsDirty = true;

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (EditorSaveOnUnload)
				{
					SaveSystem.Save();
				}
			}
#endif
			if (Application.isPlaying)
			{
				if (SaveChunkOnUnload)
				{
					SaveSystem.Save();
				}
			}

			while (chunk.Generator.Locked)
			{
				SaveSystem.UpdateRoutines();
			}
		}
	}

	private void unloadChunk(InfinityVoxel_Chunk chunk)
	{
		if (chunk.HasVoxelData)
		{
			saveChunk(chunk);
			chunk.HasVoxelData = false;
		}

		chunk.Unload();
		chunk.WasUpdated = false;
		chunk.CurrentChunkState = InfinityVoxel_ChunkState.Unloaded;
		
		removeAllNeighbours(chunk);
		RemoveLoadedChunk(chunk.ChunkInformation.ChunkPosition.x, chunk.ChunkInformation.ChunkPosition.y, chunk.ChunkInformation.ChunkPosition.z);
		UnloadedChunks.Push(chunk);	
	}

	public void LocalPositionToGenerators(Vector3 worldPosition, Vector3 Extends_Min, Vector3 Extends_Max, List<VoxelGenerator> generatorList)
	{
		float size = VolumeSize;

		Vector3 region_min = (worldPosition - Extends_Min) / size;
		Vector3 region_max = (worldPosition + Extends_Max) / size;


		Vector3Int hash_min = new Vector3Int(Mathf.FloorToInt(region_min.x), Mathf.FloorToInt(region_min.y), Mathf.FloorToInt(region_min.z));
		Vector3Int hash_max = new Vector3Int(Mathf.FloorToInt(region_max.x), Mathf.FloorToInt(region_max.y), Mathf.FloorToInt(region_max.z));

		generatorList.Clear();

		VoxelGenerator chosengenerator;
		InfinityVoxel_Chunk chunk;

		for (int x = hash_min.x; x <= hash_max.x; x++)
		{
			for (int y = hash_min.y; y <= hash_max.y; y++)
			{
				for (int z = hash_min.z; z <= hash_max.z; z++)
				{
					
					
				
					if (GetLoadedChunk(x,y,z, out  chunk))
					{
						chosengenerator = chunk.Generator;
						generatorList.Add(chosengenerator);
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
		_CleanUp();
	}

	private void setSaveInformation(InfinityVoxel_Chunk chunk)
	{
		SaveSystem.nativevoxelengine = chunk.Generator;
		SaveSystem.ModuleWorld.WorldChunk = chunk.ChunkInformation.ChunkPosition;

		//Comment out after next update.
		SaveSystem.ModuleWorld_V2.WorldChunk = chunk.ChunkInformation.ChunkPosition;
	}

	public void AddLoadedChunk(int X, int Y, int Z, InfinityVoxel_Chunk chunk)
	{
		int index = GetIndex(X, Y, Z);
		if (ChunkLayoutMap[index])
		{
			Debug.LogError("Layout Conflict");
		}


		ChunkLayoutMap[index] = chunk;
	}

	public void RemoveLoadedChunk(int X, int Y, int Z)
	{
		int index = GetIndex(X, Y, Z);
		ChunkLayoutMap[index] = null;
	}

	public bool GetLoadedChunk(int X, int Y, int Z, out InfinityVoxel_Chunk chunk)
	{
		int index = GetIndex(X, Y, Z);
		chunk = ChunkLayoutMap[index];
		return chunk != null;
	}

	public int GetIndex(int X, int Y, int Z)
	{
		X = (X % LoadContainerWidth + LoadContainerWidth) % LoadContainerWidth;
		Y = (Y % LoadContainerWidth + LoadContainerWidth) % LoadContainerWidth;
		Z = (Z % LoadContainerWidth + LoadContainerWidth) % LoadContainerWidth;

		return MathUtilities.Convert3DTo1D(X, Y, Z, LoadContainerWidth, LoadContainerWidth, LoadContainerWidth);
	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(InfinityVoxelSystem))]
public class InfinityVoxelSystemEditor : Editor
{


	public override void OnInspectorGUI()
	{
		GUIStyle title = new GUIStyle();
		title.fontStyle = FontStyle.Bold;
		title.fontSize = 14;
		title.richText = true;

		GUIStyle bold = new GUIStyle();
		bold.fontStyle = FontStyle.Bold;
		bold.fontSize = 12;
		bold.richText = true;


		EditorStyles.textField.wordWrap = true;



		InfinityVoxelSystem myTarget = (InfinityVoxelSystem)target;
		
		DrawDefaultInspector();

		EditorGUILayout.LabelField("Is Initialized: " + myTarget.Initialized);
		if (myTarget.Initialized)
		{
			EditorGUILayout.LabelField("Existing Chunks: " + myTarget.ExistingChunks.Count);
			
			EditorGUILayout.LabelField("Unloaded Chunks: " + myTarget.UnloadedChunks.Count);
		}

		if (GUILayout.Button("Generate"))
		{
			myTarget._Generate();
		}

		if (GUILayout.Button("CleanUp"))
		{
			myTarget._CleanUp();
		}

		if (myTarget.Initialized)
		{
			if (GUILayout.Button("Save Active Chunks"))
			{
				myTarget.SaveActiveChunks();
			}




		}
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}
#endif
