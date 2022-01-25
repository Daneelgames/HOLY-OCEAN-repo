using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CharacterIk : MonoBehaviour
{
    public List<Transform> IkHelpers;
    public List<Transform> IkTargets;
    public LayerMask layersToRaycast;
    public Transform raycastTarget;
    void Start()
    {
        StartCoroutine(LegsIk());
    }

    IEnumerator LegsIk()
    {
        while (true)
        {
            for (int i = 0; i < IkHelpers.Count; i++)
            {
                if (Physics.Raycast(IkHelpers[i].position, (raycastTarget.position - IkHelpers[i].position).normalized,  out var hit, Vector3.Distance(IkHelpers[i].position, raycastTarget.position),  layersToRaycast))
                {
                    IkTargets[i].position = hit.point;
                }
                else
                {
                    IkTargets[i].position = IkHelpers[i].position;
                    IkTargets[i].rotation = IkHelpers[i].rotation;
                }
                yield return null;
            }

            yield return null;
        }
    }
}
