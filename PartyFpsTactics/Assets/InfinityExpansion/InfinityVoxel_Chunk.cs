using Fraktalia.VoxelGen;
using Fraktalia.VoxelGen.Visualisation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InfinityVoxel_Chunk : MonoBehaviour
{
	public InfinityVoxelSystem Infinity;

	public VoxelGenerator Generator;
	public ChunkManifest ChunkInformation;

	[NonSerialized]
	public InfinityVoxel_Chunk[] Neighbours;
	public int NeighbourCount;

	public InfinityVoxel_ChunkState CurrentChunkState;
	
	public List<ChunkLoaderReference> referencedChunkLoaders;

	public bool WasUpdated;
	public bool IsInPriorityList;
	public bool HasVoxelData;

	public bool neednewdata = false;
	public bool needsrebuild = false;
	public bool needsreset;

	public void Initialize()
	{
		referencedChunkLoaders = new List<ChunkLoaderReference>();
		CurrentChunkState = InfinityVoxel_ChunkState.Loaded;
		
		Neighbours = new InfinityVoxel_Chunk[27];
		Generator.IsExtension = true;
		Generator.enabled = false;

		Generator.hullGenerators = Generator.GetComponentsInChildren<BasicNativeVisualHull>();
		for (int i = 0; i < Generator.hullGenerators.Length; i++)
		{
			Generator.hullGenerators[i].Shrink = 0;
		}


		Generator.GenerateBlock();

		NeighbourCount = 0;

	}

	public void AddChunkLoaderReference(ChunkManifest manifest, InfinityVoxel_ChunkLoader chunkloader)
	{
		for (int i = 0; i < referencedChunkLoaders.Count; i++)
		{
			if (referencedChunkLoaders[i].Loader == chunkloader)
			{
				referencedChunkLoaders[i].Manifest = manifest;
				referencedChunkLoaders[i].IsInside = true;
				Generator.ChunkHash = manifest.ChunkPosition;
				return;
			}
		}

		referencedChunkLoaders.Add(new ChunkLoaderReference(manifest, chunkloader));
		Generator.ChunkHash = manifest.ChunkPosition;
	}

	public void AddNeighbour(int index, InfinityVoxel_Chunk chunk)
	{
		int neighborindex = 26 - index;

		Neighbours[index] = chunk;
		Generator.SetNeighbor(index, chunk.Generator);
		NeighbourCount++;
		needsrebuild = true;

		chunk.Neighbours[neighborindex] = this;
		chunk.Generator.SetNeighbor(neighborindex, Generator);
		chunk.NeighbourCount++;
		chunk.needsrebuild = true;
	}

	public void RemoveNeighbour(int index)
	{
		int neighborindex = 26 - index;

		InfinityVoxel_Chunk chunk = Neighbours[index];
		Neighbours[index] = null;
		Generator.RemoveNeighbor(index);
		NeighbourCount--;
		needsrebuild = true;

		chunk.Neighbours[neighborindex] = null;
		chunk.Generator.RemoveNeighbor(neighborindex);
		chunk.NeighbourCount--;
		chunk.needsrebuild = true;


	}


	public void UpdateContent()
	{
		if (Infinity.CullBorders)
		{
			if (CurrentChunkState == InfinityVoxel_ChunkState.Core)
			{
				if (ChunkInformation.State == InfinityVoxel_ChunkState.Core)
				{
					return;
				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.HasBorder)
				{
					needsrebuild = true;
				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.Border)
				{
					needsreset = true;
				}
			}

			if (CurrentChunkState == InfinityVoxel_ChunkState.HasBorder)
			{
				if (ChunkInformation.State == InfinityVoxel_ChunkState.Core)
				{
					needsrebuild = true;
				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.HasBorder)
				{
					needsrebuild = true;
				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.Border)
				{
					needsreset = true;
				}
			}

			if (CurrentChunkState == InfinityVoxel_ChunkState.Border)
			{
				if (ChunkInformation.State == InfinityVoxel_ChunkState.Border)
				{
					needsreset = true;
				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.Core)
				{
					neednewdata = true;
				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.HasBorder)
				{
					neednewdata = true;
				}
			}
		}

		if (CurrentChunkState == InfinityVoxel_ChunkState.Loaded)
		{
			if (Infinity.CullBorders)
			{
				if (ChunkInformation.State == InfinityVoxel_ChunkState.Border)
				{

					needsrebuild = true;
				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.Core)
				{
					neednewdata = true;

				}

				if (ChunkInformation.State == InfinityVoxel_ChunkState.HasBorder)
				{
					neednewdata = true;
				}
			}
			else
			{
				neednewdata = true;			
			}
		}

		

		CurrentChunkState = ChunkInformation.State;
		UpdateLOD();
	}

	public bool IsInsideChunks()
	{
		bool isinside = false;
		bool changed = false;

		for (int i = 0; i < referencedChunkLoaders.Count; i++)
		{
			var reference = referencedChunkLoaders[i];
			if (reference.Loader == null)
			{
				referencedChunkLoaders.RemoveAt(i);
				i--;
				changed = true;
			}
			else
			{
				if (reference.Loader._ContainsChunk(ChunkInformation.ChunkPosition))
				{
					isinside = true;
					reference.IsInside = true;
				}
				else
				{

					changed = true;
					referencedChunkLoaders.RemoveAt(i);
					reference.IsInside = false;
				}
			}
		}

		if (isinside && changed)
		{
			WasUpdated = true;
		}

		return isinside;

	}

	internal void UpdateTrees()
	{
		Generator.UpdateVoxelTreeRoutine.MoveNext();	
	}

	internal void UpdateRegions()
	{
		Generator.updateRegions();
	}

	internal void Unload()
	{
		if (HasVoxelData)
		{
			Infinity.saveChunk(this);
			HasVoxelData = false;
		}
		

		Generator.ResetData();

		
		Generator.RestrictDimension[0] = true;

		Generator.ClearMeshes();

		referencedChunkLoaders.Clear();
	}

	internal void UpdateLOD()
	{
		if (Infinity.EnableLOD)
		{
			int lod = Mathf.FloorToInt(ChunkInformation.Distance * Infinity.LODMultiplier);
			Generator.LODDistance = lod;
			Generator.updateLOD();
		}
	}

	internal void _CleanUp()
	{
		Generator.CleanUp();
	}

	
}
