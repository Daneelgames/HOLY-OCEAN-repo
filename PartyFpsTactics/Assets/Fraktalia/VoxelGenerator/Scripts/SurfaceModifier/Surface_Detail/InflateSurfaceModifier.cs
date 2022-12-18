using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Fraktalia.Core.Math;
using Fraktalia.Utility;

namespace Fraktalia.VoxelGen.Visualisation
{
    public class InflateSurfaceModifier : BasicSurfaceModifier
    {
        public float Inflation;

        public override void DefineSurface(VoxelPiece piece, NativeList<Vector3> surface_verticeArray, NativeList<int> surface_triangleArray, NativeList<Vector3> surface_normalArray, int slot)
        {
            InflateSurfaceJob job;
            job.Inflation = Inflation;
            job.vertices = surface_verticeArray;
            job.normals = surface_normalArray;
            job.Schedule(surface_verticeArray.Length, surface_verticeArray.Length / SystemInfo.processorCount).Complete();

            piece.SetVertices(job.vertices);
        }

        internal override float GetChecksum()
        {
            return base.GetChecksum() + Inflation;
        }
    }

    [BurstCompile]
    public struct InflateSurfaceJob : IJobParallelFor
    {
        public float Inflation;

        [NativeDisableParallelForRestriction]
        public NativeList<Vector3> vertices;

        [ReadOnly]
        public NativeList<Vector3> normals;

        public void Execute(int index)
        {
            Vector3 vertex = vertices[index];
            Vector3 normal = normals[index];

            vertex += normal.normalized * Inflation;

            vertices[index] = vertex;
        }
    }
}