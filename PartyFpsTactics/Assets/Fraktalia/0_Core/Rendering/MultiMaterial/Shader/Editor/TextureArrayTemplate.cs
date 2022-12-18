#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using Fraktalia.Core.FraktaliaAttributes;
using System.Runtime.Serialization;
using System.IO;

namespace Fraktalia.Core.FraktaliaAttributes
{
	public class TextureArrayTemplate : ScriptableObject
	{
		[NonReorderable]
		[Header("Materials To Extract")]
		public Material[] Materials;

		[Header("NULL Placeholders")]
		public Texture2D WhiteDefaultTexture;
		public Texture2D BlackDefaultTexture;
		public Texture2D NormalDefaultTexture;
		public Texture2D HeightDefaultTexture;

		[NonReorderable]
		[Header("Texture Arrays")]
		public Texture2D[] OcclusionTextures;
		[NonReorderable]
		public Texture2D[] DiffuseTextures;
		[NonReorderable]
		public Texture2D[] EmmissiveTextures;
		[NonReorderable]
		public Texture2D[] HeightTextures;
		[NonReorderable]
		public Texture2D[] MetallicTextures;
		[NonReorderable]
		public Texture2D[] NormalTextures;

		[NonReorderable]
		public Color[] BaseTextureMultiplier;
		[NonReorderable]
		public Color[] MetallicMultiplier;
		[NonReorderable]
		public Color[] EmissionMultiplier;


		public string OutputPath;
		public string FinalName;

		[Header("Target Material for Material Assignment")]
		[Tooltip("Target Material which should use texture arrays. Shader must be compatible")]
		public Material TargetMaterial;

		[Header("Texture Atlas creation")]
	
		[Tooltip("Target Material for texture atlasses created by extracting from target material into")]
		public Material AtlasMaterial;
		
		[Space]
		public bool UseMaterialPath;




		public Texture2DArray CreateTextureArray(Texture2D[] ordinaryTextures, Texture2D nullTexture, string FileName, Color[] _Colors = null)
		{
			

			Texture2DArray texture2DArray = null;

			int width = 16;
			int height = 16;
			for (int i = 0; i < ordinaryTextures.Length; i++)
			{
				if (ordinaryTextures[i] != null)
				{
					width = Mathf.Max(width, ordinaryTextures[i].width);
					height = Mathf.Max(height, ordinaryTextures[i].height);
				}
			}

			texture2DArray = new Texture2DArray(width, height, ordinaryTextures.Length, TextureFormat.RGBA32, true, false);
			texture2DArray.filterMode = FilterMode.Bilinear;
			texture2DArray.wrapMode = TextureWrapMode.Repeat;    // Loop through ordinary textures and copy pixels to the


			for (int i = 0; i < ordinaryTextures.Length; i++)
			{
				Texture2D texture = ordinaryTextures[i];
				if (texture == null)
				{
					texture = nullTexture;
				}

				if (!texture.isReadable)
				{
					SetTextureReadable(texture, true);
				}


				Color[] pixels = new Color[texture2DArray.width * texture2DArray.height];
				Color _Color = Color.white;
				if (_Colors != null)
				{
					_Color = _Colors[i];
				}

				for (int y = 0; y < texture2DArray.height; y++)
				{
					for (int x = 0; x < texture2DArray.width; x++)
					{
						int x2 = x % texture.width;
						int y2 = y % texture.height;

						pixels[x + y * texture2DArray.width] = texture.GetPixel(x2, y2) * _Color;

					}
				}

				texture2DArray.SetPixels(pixels, i, 0);


			}
			texture2DArray.Apply();

			string path = OutputPath;
			if (!AssetDatabase.IsValidFolder(path))
			{
				path = "Assets";
			}

			path += "/" + FileName + ".asset";

			AssetDatabase.CreateAsset(texture2DArray, path);
			AssetDatabase.SaveAssets();

			Debug.Log("Saved asset to " + path);

			return texture2DArray;
		}

		public void CreateAllTextureArrays()
		{
			Texture2DArray[] array = new Texture2DArray[6];
			array[0] = CreateTextureArray(OcclusionTextures, WhiteDefaultTexture, FinalName + "_OcclusionMaps");


			array[1] = CreateTextureArray(DiffuseTextures, WhiteDefaultTexture, FinalName + "_DiffuseMaps", BaseTextureMultiplier);

			array[2] = CreateTextureArray(EmmissiveTextures, BlackDefaultTexture, FinalName + "_EmissionMaps", EmissionMultiplier);

			array[3] = CreateTextureArray(HeightTextures, HeightDefaultTexture, FinalName + "_HeightMaps");

			array[4] = CreateTextureArray(MetallicTextures, WhiteDefaultTexture, FinalName + "_MetallicMaps", MetallicMultiplier);

			array[5] = CreateTextureArray(NormalTextures, NormalDefaultTexture, FinalName + "_NormalMaps");

			if (TargetMaterial)
			{

				AssignTextureArray("_OcclusionMap", array[0]);
				AssignTextureArray("_DiffuseMap", array[1]);
				AssignTextureArray("_EmissionMap", array[2]);
				AssignTextureArray("_ParallaxMap", array[3]);
				AssignTextureArray("_MetallicGlossMap", array[4]);
				AssignTextureArray("_BumpMap", array[5]);
			}

		}

		public void CreateAllTextureAtlases()
        {
			Texture2D texture;
			if (AtlasMaterial)
			{
				texture = CreateTextureAtlas("_OcclusionMap");
				AssignTexture2D("_OcclusionMap", texture, AtlasMaterial);

				texture = CreateTextureAtlas("_DiffuseMap");
				AssignTexture2D("_DiffuseMap", texture, AtlasMaterial);

				texture = CreateTextureAtlas("_EmissionMap");
				AssignTexture2D("_EmissionMap", texture, AtlasMaterial);

				texture = CreateTextureAtlas("_ParallaxMap");
				AssignTexture2D("_ParallaxMap", texture, AtlasMaterial);

				texture = CreateTextureAtlas("_MetallicGlossMap");
				AssignTexture2D("_MetallicGlossMap", texture, AtlasMaterial);

				texture = CreateTextureAtlas("_BumpMap");
				AssignTexture2D("_BumpMap", texture, AtlasMaterial);

			
			}
		}

		public Texture2D CreateTextureAtlas(string MaterialIdentifier)
        {
		
			Texture textureread = TargetMaterial.GetTexture(MaterialIdentifier);
			Texture2DArray texture2DArray = textureread as Texture2DArray;
			
			

			int requiredrows = Mathf.NextPowerOfTwo(texture2DArray.depth)/2;

			int requiredSize = texture2DArray.width * requiredrows;


			Texture2D texture = new Texture2D(requiredSize, requiredSize);
			texture.filterMode = FilterMode.Bilinear;
			texture.wrapMode = TextureWrapMode.Repeat;

            for (int i = 0; i < texture2DArray.depth; i++)
            {
				int offsetX = i % requiredrows;
				int offsetY = i / requiredrows;
				Color32[] pixels = texture2DArray.GetPixels32(i, 0);

				for (int y = 0; y < texture2DArray.height; y++)
				{
					for (int x = 0; x < texture2DArray.width; x++)
					{
						int x2 = x % texture.width;
						int y2 = y % texture.height;

						Color32 pixel = pixels[x + y * texture2DArray.width];
						texture.SetPixel(x2 + offsetX * texture2DArray.width, y2 + offsetY * texture2DArray.height, pixel);
					}
				}
			}

			string path = OutputPath;
			if (!AssetDatabase.IsValidFolder(path))
			{
				path = "Assets";
			}

			path += "/" + texture2DArray.name + "Atlas.png";
			byte[] bytes = texture.EncodeToPNG();	
			File.WriteAllBytes(path, bytes);

			/*
			Texture2D tex = null;
			byte[] fileData;

			if (File.Exists(path))
			{
				fileData = File.ReadAllBytes(path);
				tex = new Texture2D(2, 2);
				tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
			}
			*/
			AssetDatabase.SaveAssets();
			
			return (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
		}




		public void SaveProfile()
		{
			string path = OutputPath;
			if (!AssetDatabase.IsValidFolder(path))
			{
				path = "Assets";
			}

			path += "/" + "Template" + ".asset";
			
			AssetDatabase.CreateAsset(this, path);
			AssetDatabase.SaveAssets();

		}

		public void ExtractMaterials()
		{
			OcclusionTextures = new Texture2D[Materials.Length];
			DiffuseTextures = new Texture2D[Materials.Length];
			EmmissiveTextures = new Texture2D[Materials.Length];
			HeightTextures = new Texture2D[Materials.Length];
			MetallicTextures = new Texture2D[Materials.Length];
			NormalTextures = new Texture2D[Materials.Length];

			BaseTextureMultiplier = new Color[Materials.Length];
			MetallicMultiplier = new Color[Materials.Length];
			EmissionMultiplier = new Color[Materials.Length];

			for (int i = 0; i < BaseTextureMultiplier.Length; i++)
			{
				BaseTextureMultiplier[i] = Color.white;
				MetallicMultiplier[i] = Color.white;
				EmissionMultiplier[i] = Color.white;
			}


			for (int i = 0; i < Materials.Length; i++)
			{



				if (Materials[i] != null)
				{
					OcclusionTextures[i] = extracttexture(Materials[i], "_OcclusionMap", WhiteDefaultTexture);
					DiffuseTextures[i] = extracttexture(Materials[i], "_MainTex", WhiteDefaultTexture);
					EmmissiveTextures[i] = extracttexture(Materials[i], "_EmissionMap", BlackDefaultTexture);
					HeightTextures[i] = extracttexture(Materials[i], "_ParallaxMap", HeightDefaultTexture);
					MetallicTextures[i] = extracttexture(Materials[i], "_MetallicGlossMap", WhiteDefaultTexture);
					NormalTextures[i] = extracttexture(Materials[i], "_BumpMap", NormalDefaultTexture);



					BaseTextureMultiplier[i] = extractcolor(Materials[i], "_Color");
					EmissionMultiplier[i] = extractcolor(Materials[i], "_EmissionColor");

					if (MetallicTextures[i] == WhiteDefaultTexture)
					{
						float metallic = extractfloat(Materials[i], "_Metallic");
						float glossiness = extractfloat(Materials[i], "_Glossiness");

						MetallicMultiplier[i] = new Color(metallic, metallic, metallic, glossiness);

					}
					else
					{
						float glossiness = extractfloat(Materials[i], "_GlossMapScale");
						MetallicMultiplier[i] = new Color(1, 1, 1, glossiness);

					}


				}

			}
		}

		private Texture2D extracttexture(Material target, string PropertyName, Texture2D NullTexture)
		{
			if (target == null) return NullTexture;


			if (target.HasProperty(PropertyName) && target.GetTexture(PropertyName) != null)
			{
				return (Texture2D)target.GetTexture(PropertyName);
			}
			else
			{
				return NullTexture;
			}
		}
		private Color extractcolor(Material target, string PropertyName)
		{
			if (target == null) return Color.white;


			if (target.HasProperty(PropertyName) && target.GetColor(PropertyName) != null)
			{
				return target.GetColor(PropertyName);
			}
			else
			{
				return Color.white;
			}
		}

		private float extractfloat(Material target, string PropertyName)
		{
			if (target == null) return 1;


			if (target.HasProperty(PropertyName))
			{
				return target.GetFloat(PropertyName);
			}
			else
			{
				return 1;
			}
		}

		public bool AreTexturesReadable()
		{
			if (!IsReadable(OcclusionTextures)) return false;
			if (!IsReadable(DiffuseTextures)) return false;
			if (!IsReadable(EmmissiveTextures)) return false;
			if (!IsReadable(HeightTextures)) return false;
			if (!IsReadable(MetallicTextures)) return false;
			if (!IsReadable(NormalTextures)) return false;

			return true;
		}

		private bool IsReadable(Texture2D[] textures)
		{
			if (textures != null)
			{
				for (int i = 0; i < textures.Length; i++)
				{
					if(textures[i])
					{
						if (!textures[i].isReadable) return false;
					}
				}
			}

			return true;
		}

		public void SetTextureReadable(Texture2D texture, bool isReadable)
		{
			if (null == texture) return;

			string assetPath = AssetDatabase.GetAssetPath(texture);
			var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
			if (tImporter != null)
			{

				tImporter.isReadable = isReadable;

				AssetDatabase.ImportAsset(assetPath);
				AssetDatabase.Refresh();
			}
		}

		public void AssignTextureArray(string TextureName, Texture2DArray textureArray)
		{
			MaterialProperty property = MaterialEditor.GetMaterialProperty(new UnityEngine.Object[] { TargetMaterial }, TextureName);
			if (property.textureDimension == UnityEngine.Rendering.TextureDimension.Tex2DArray)
			{
				TargetMaterial.SetTexture(TextureName, textureArray);
			}
			else
			{
				Debug.LogError("Cannot assign texture array: " + TextureName + " to TargetMaterial. Shader has no texture array property with name: " + TextureName);
			}
		}

		public void AssignTexture2D(string TextureName, Texture2D texture2D, Material target)
		{
			MaterialProperty property = MaterialEditor.GetMaterialProperty(new UnityEngine.Object[] { target }, TextureName);
			if (property.textureDimension == UnityEngine.Rendering.TextureDimension.Tex2D)
			{
				target.SetTexture(TextureName, texture2D);
			}
			else
			{
				Debug.LogError("Cannot assign texture array: " + TextureName + " to TargetMaterial. Shader has no texture2D property with name: " + TextureName);
			}
		}

		public void AssignTexture3D(string TextureName, Texture3D textureArray)
		{
			MaterialProperty property = MaterialEditor.GetMaterialProperty(new UnityEngine.Object[] { TargetMaterial }, TextureName);
			if (property.textureDimension == UnityEngine.Rendering.TextureDimension.Tex3D)
			{
				TargetMaterial.SetTexture(TextureName, textureArray);
			}
			else
			{
				Debug.LogError("Cannot assign texture array: " + TextureName + " to TargetMaterial. Shader has no texture3D property with name: " + TextureName);
			}
		}

		public string CheckTexture(string TextureName)
		{
			string output = "Correct";
			MaterialProperty property = MaterialEditor.GetMaterialProperty(new UnityEngine.Object[] { TargetMaterial }, TextureName);
			if (property.textureDimension != UnityEngine.Rendering.TextureDimension.Tex2DArray)
			{
				output = "Not a 2D array";
			}

			return output;
		}

		public string checkMaterial()
		{
			string output = "Check if Materials can be assigned: \n\n";
			output += "_DiffuseMap" + " \t= " + CheckTexture("_DiffuseMap") + "\n";
			output += "_BumpMap" + " \t= " + CheckTexture("_BumpMap") + "\n";
			output += "_OcclusionMap" + " \t= " + CheckTexture("_OcclusionMap") + "\n";

			output += "_EmissionMap" + " \t= " + CheckTexture("_EmissionMap") + "\n";
			output += "_ParallaxMap" + " \t= " + CheckTexture("_ParallaxMap") + "\n";
			output += "_MetallicGlossMap" + " \t= " + CheckTexture("_MetallicGlossMap") + "\n";


			return output;
		}
	}



}

#endif
