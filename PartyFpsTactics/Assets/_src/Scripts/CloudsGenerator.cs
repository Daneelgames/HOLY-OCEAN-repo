    using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class CloudsGenerator : MonoBehaviour
{
    [SerializeField] private MeshRenderer cloudPrefab;
    [SerializeField] private int cloudsAmount = 300;
    [SerializeField] private Vector2 cloudScaleMinMax = new Vector2(200, 500);
    [SerializeField] private Vector2 cloudDistanceMinMax = new Vector2(200, 500);
    [SerializeField] private Vector2 rotationSpeedMinMax = new Vector2(-5, 5);
    private Transform cloudsParent;
    private static readonly int materialColorProperty = Shader.PropertyToID("_Color");

    private IEnumerator Start()
    {
        cloudsParent = new GameObject("CloudsParent").transform;
        cloudPrefab.sharedMaterial.SetColor(materialColorProperty, EnvironmentVisualManager.Instance.CurrentCloudsColor);
        cloudsParent.parent = transform;
        cloudsParent.localPosition = Vector3.zero;
        int index = 0;
        for (int i = 0; i < cloudsAmount; i++)
        {
            Vector3 pos = transform.position + Random.onUnitSphere * Random.Range(cloudDistanceMinMax.x, cloudDistanceMinMax.y);
            var newCloud = Instantiate(cloudPrefab, pos, Quaternion.LookRotation(pos - transform.position));
            newCloud.transform.localScale = Vector3.one * Random.Range(cloudScaleMinMax.x, cloudScaleMinMax.y);
            newCloud.transform.parent = cloudsParent;
            index++;
            if (index > 10)
            {
                index = 0;
                yield return null;
            }
        }
        
        var autoRotate = cloudsParent.AddComponent<CFX_AutoRotate>();
        var followTarget = cloudsParent.AddComponent<FollowTarget>();
        autoRotate.rotation = new Vector3(Random.Range(rotationSpeedMinMax.x, rotationSpeedMinMax.y), Random.Range(rotationSpeedMinMax.x, rotationSpeedMinMax.y), Random.Range(rotationSpeedMinMax.x, rotationSpeedMinMax.y));
        autoRotate.space = Space.Self;
    }
}
