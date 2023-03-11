using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Fraktalia.VoxelGen.Modify.Positioning;
using UnityEngine.XR;
using UnityEngine.UIElements;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.VoxelGen;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Fraktalia.VoxelGen.Modify.Procedural
{
	public class InfinityProceduralVoxelModifier : MonoBehaviour
	{
		public ProceduralVoxelModifier Modifier;
		public InfinityVoxelSystem InfinitySystem;

		private void OnDrawGizmosSelected()
		{
			if(InfinitySystem && Modifier)
			{
				Modifier.TargetGenerator = InfinitySystem.GetComponentInChildren<VoxelGenerator>();
			}
		}


		public void ApplyProceduralModifier()
		{
		

			if (InfinitySystem)
			{
				if (InfinitySystem.Initialized)
				{
					List<VoxelGenerator> modifiedgenerators = new List<VoxelGenerator>();

					Modifier.TargetGenerator = InfinitySystem.ChunkTemplate.Generator;
					Bounds bounds = Modifier.CalculateBounds();


					InfinitySystem.LocalPositionToGenerators(bounds.center, bounds.extents, bounds.extents, modifiedgenerators);

					for (int i = 0; i < modifiedgenerators.Count; i++)
					{
						Modifier.TargetGenerator = modifiedgenerators[i];
						Modifier.ApplyProceduralModifier();					
					}
					Modifier.FinishApplication();
					
				}

			}

			

		}
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(InfinityProceduralVoxelModifier))]
	[CanEditMultipleObjects]
	public class InfinityProceduralVoxelModifierEditor : Editor
	{	
		public override void OnInspectorGUI()
		{



			GUIStyle bold = new GUIStyle();
			bold.fontStyle = FontStyle.Bold;
			bold.fontSize = 14;
			bold.richText = true;

			DrawDefaultInspector();

			InfinityProceduralVoxelModifier myTarget = target as InfinityProceduralVoxelModifier;

			if (GUILayout.Button("Apply Procedural Modifier"))
			{
				myTarget.ApplyProceduralModifier();
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}
		}




	}
#endif

}
