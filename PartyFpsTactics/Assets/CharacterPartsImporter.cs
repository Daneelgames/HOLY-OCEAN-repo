using System.Collections;
using System.Collections.Generic;
using MrPink.Units;
using UnityEngine;

public class CharacterPartsImporter : MonoBehaviour
{
    public List<GameObject> parts;

    public HumanVisualController VisualController;
    
    [ContextMenu("CopyParts")]
    public void CopyParts()
    {
        var bones = VisualController.allBones;
        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            var partCopy = Instantiate(part);
            partCopy.name = part.name;
            partCopy.transform.parent = bones[i];
            partCopy.transform.localPosition = part.transform.localPosition;
            partCopy.transform.localRotation = part.transform.localRotation;
            partCopy.transform.localScale = part.transform.localScale;
        }
    }

    [ContextMenu("CopyClothes")]
    public void CopyClothes()
    {
        var bones = VisualController.allBones;
        for (int i = 0; i < parts.Count; i++)
        {
            foreach (Transform child in parts[i].transform.parent)
            {
                if (child == parts[i].transform)
                    continue;
                
                var meshRenderer = child.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    // it's a cloth part
                    var partCopy = Instantiate(child.gameObject);
                    partCopy.name = child.name;
                    partCopy.transform.parent = bones[i];
                    partCopy.transform.localPosition = child.transform.localPosition;
                    partCopy.transform.localRotation = child.transform.localRotation;
                    partCopy.transform.localScale = child.transform.localScale;
                }
            }
        }
    }
}
