using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Fraktalia.VoxelGen.Modify
{
	public class VM_PostProcess : MonoBehaviour
	{
		public bool Enabled = true;
		public virtual void ApplyPostprocess(NativeList<NativeVoxelModificationData_Inner> modifierData, VoxelGenerator generator, VoxelModifier_V2 modifier)
		{

		}

		public virtual void FinalizeModification(NativeList<NativeVoxelModificationData_Inner> modifierData,
			NativeList<NativeVoxelModificationData_Inner> preVoxelData,
			NativeList<NativeVoxelModificationData_Inner> postVoxelData, VoxelGenerator generator, VoxelModifier_V2 modifier)
		{

		}
	}
}
