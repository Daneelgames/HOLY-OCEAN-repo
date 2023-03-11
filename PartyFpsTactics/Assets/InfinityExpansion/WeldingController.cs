using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fraktalia.Core.Samples;
using Fraktalia.VoxelGen.Modify;

namespace Fraktalia.VoxelGen.Samples
{
	public class WeldingController : MonoBehaviour
	{
		public NoClipMouseLook mouseLook;
		public InfinityVoxelModifier modifier;			
		public ParticleSystem Fire;
			
		private void Update()
		{
			if (Input.GetKey(KeyCode.LeftControl))
			{
				if (mouseLook)
					mouseLook.enabled = false;				
			}
			else
			{
				if (mouseLook)
					mouseLook.enabled = true;				
			}

			if(Input.GetMouseButton(0) && !mouseLook.enabled)
			{
				Fire.gameObject.SetActive(true);
			}
			else
			{
				Fire.gameObject.SetActive(false);
			}
		}

		

	}
}
