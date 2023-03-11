using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Fraktalia.VoxelGen.Modify.Positioning;
using UnityEngine.XR;
using UnityEngine.UIElements;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.VoxelGen.World;

#if UNITY_EDITOR
	using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Fraktalia.VoxelGen.Modify
{
	public class InfinityWorldAlgorithmUpdater : MonoBehaviour
	{
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(InfinityWorldAlgorithmUpdater))]
	[CanEditMultipleObjects]
	public class InfinityWorldAlgorithmUpdaterEditor : Editor
	{
		public override void OnInspectorGUI()
		{


		

			InfinityWorldAlgorithmUpdater myTarget = target as InfinityWorldAlgorithmUpdater;

		
			if (GUILayout.Button("Rebuild"))
			{
				InfinityVoxelSystem infinity = myTarget.GetComponentInParent<InfinityVoxelSystem>();
				if (infinity && infinity.Initialized)
				{
					if (GUI.changed)
					{
						for (int i = 0; i < infinity.ExistingChunks.Count; i++)
						{
							if(infinity.ProceduralGenerator)
							{
								infinity.ProceduralGenerator.Generate(infinity.ExistingChunks[i].Generator);
								infinity.ExistingChunks[i].neednewdata = true;
								//infinity.ExistingChunks[i].Generator.SetAllRegionsDirty();
							}
							
						}
					}
				}
			}
		}
	}
#endif

}
