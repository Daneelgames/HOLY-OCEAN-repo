using Fraktalia.VoxelGen.Visualisation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityVoxel_ChunkLoader : MonoBehaviour
{
	[NonSerialized]
	public List<ChunkManifest> Chunks = new List<ChunkManifest>();
	public int Range = 3;

	protected Vector3Int currentPosition = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

	public void _DrawPreview(InfinityVoxelSystem system)
	{
		currentPosition = system._WorldPositionToChunk(transform.position);
		_DefineChunks(system);
		
		float VolumeSize = system.ChunkTemplate.Generator.RootSize;
		for (int i = 0; i < Chunks.Count; i++)
		{
			if(Chunks[i].State == InfinityVoxel_ChunkState.Core)
			{
				Gizmos.color = system.DebugColor_Core;
			}
			else if (Chunks[i].State == InfinityVoxel_ChunkState.HasBorder)
			{
				Gizmos.color = system.DebugColor_HasBorder;
			}
			else
			{
				Gizmos.color = system.DebugColor_Border;
			}

			Gizmos.DrawWireCube(system._ChunkToWorldposition(Chunks[i].ChunkPosition) + new Vector3(VolumeSize, VolumeSize, VolumeSize) / 2, Vector3.one * VolumeSize);
		}
	}

	public void _Initialize()
	{
		currentPosition = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
		Chunks = new List<ChunkManifest>();
	}

	public virtual void _DefineChunks(InfinityVoxelSystem system)
	{
		Chunks.Clear();
		
		for (int x = -Range; x <= Range; x++)
		{
			for (int y = -Range; y <= Range; y++)
			{
				for (int z = -Range; z <= Range; z++)
				{
					ChunkManifest manifest = new ChunkManifest();
					manifest.ChunkPosition = currentPosition + new Vector3Int(x, y, z);

					int Distance = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));
					manifest.Distance = Distance;
					manifest.Priority = Mathf.Max(0, manifest.Distance - 1);

					if(Distance < Range -1)
					{
						manifest.State = InfinityVoxel_ChunkState.Core;
					}
					else if(Distance < Range)
					{
						manifest.State = InfinityVoxel_ChunkState.HasBorder;
					}
					else
					{
						manifest.State = InfinityVoxel_ChunkState.Border;
					}


					Chunks.Add(manifest);
				}
			}
		}		
	}

	public bool _PositionChanged(InfinityVoxelSystem system)
	{
		Vector3Int position =  system._WorldPositionToChunk(transform.position);
		if(position != currentPosition)
		{
			currentPosition = position;
			return true;
		}

		return false;
	}

	public void _CleanUp()
	{
		Chunks.Clear();
	}

	public virtual bool _ContainsChunk(Vector3Int chunkPosition)
	{
		Vector3Int extent = new Vector3Int(Range, Range, Range);
		Vector3 min = currentPosition - extent;
		Vector3 max = currentPosition + extent;

		if (chunkPosition.x < min.x || chunkPosition.x > max.x) return false;
		if (chunkPosition.y < min.y || chunkPosition.y > max.y) return false;
		if (chunkPosition.z < min.z || chunkPosition.z > max.z) return false;

		return true;
	}
}

[System.Serializable]
public struct ChunkManifest
{
	public Vector3Int ChunkPosition;
	public int Distance;
	public int Priority;
	public InfinityVoxel_ChunkState State;
}

public class ChunkLoaderReference
{
	public ChunkManifest Manifest;
	public InfinityVoxel_ChunkLoader Loader;
	public bool IsInside;


	public ChunkLoaderReference(ChunkManifest manifest, InfinityVoxel_ChunkLoader loader)
	{
		Manifest = manifest;
		Loader = loader;
		IsInside = true;
	}

	public static ChunkManifest CombineManifest(List<ChunkLoaderReference> manifests)
	{
		ChunkManifest output = new ChunkManifest();
		output.Distance = 1000;
		output.Priority = 1000;
		output.State = InfinityVoxel_ChunkState.Loaded;

		for (int i = 0; i < manifests.Count; i++)
		{
			ChunkLoaderReference manifest = manifests[i];

			ChunkManifest current = manifest.Manifest;
			output.ChunkPosition = current.ChunkPosition;
			output.Distance = Mathf.Min(output.Distance, current.Distance);
			output.Priority = Mathf.Min(output.Priority, current.Priority);
			output.State = (InfinityVoxel_ChunkState)Mathf.Max((int)output.State, (int)current.State);

		}

		return output;
	}
}



public enum InfinityVoxel_ChunkState
{
	Loaded,
	Unloaded,
	Border,
	HasBorder,
	Core
	
}
