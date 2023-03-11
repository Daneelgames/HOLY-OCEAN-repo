using Fraktalia.VoxelGen.Visualisation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityVoxel_ChunkLoader_Plane : InfinityVoxel_ChunkLoader
{
	
	public int HeightPosition;
	public int HeightWidth;

	public override void _DefineChunks(InfinityVoxelSystem system)
	{
		currentPosition.y = HeightPosition;

		Vector3Int Box = new Vector3Int(Range, HeightWidth, Range);

		Chunks.Clear();
		
		for (int x = -Box.x; x <= Box.x; x++)
		{
			for (int y = -Box.y; y <= Box.y; y++)
			{
				for (int z = -Box.z; z <= Box.z; z++)
				{
					ChunkManifest manifest = new ChunkManifest();
					manifest.ChunkPosition = currentPosition + new Vector3Int(x, y, z);

					int Distance = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));
					manifest.Distance = Distance;
					manifest.Priority = Mathf.Max(0, manifest.Distance - 1);

					

					if(Mathf.Abs(x) < Box.x -1 && Mathf.Abs(y) < Box.y -1 && Mathf.Abs(z) < Box.z -1)
					{
						manifest.State = InfinityVoxel_ChunkState.Core;
					}
					else if(Mathf.Abs(x) < Box.x && Mathf.Abs(y) < Box.y && Mathf.Abs(z) < Box.z)
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

	public override bool _ContainsChunk(Vector3Int chunkPosition)
	{
		Vector3Int extent = new Vector3Int(Range, HeightWidth, Range);
		Vector3 min = currentPosition - extent;
		Vector3 max = currentPosition + extent;

		if (chunkPosition.x < min.x || chunkPosition.x > max.x) return false;
		if (chunkPosition.y < min.y || chunkPosition.y > max.y) return false;
		if (chunkPosition.z < min.z || chunkPosition.z > max.z) return false;

		return true;
	}
}
