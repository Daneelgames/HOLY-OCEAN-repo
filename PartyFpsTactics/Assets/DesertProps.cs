using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DesertProps : MonoBehaviour
{
    public static DesertProps Instance;

    public List<GameObject> desertProps;
    private void Awake()
    {
        Instance = this;
    }

    public GameObject SpawnRandomProp(Vector3 pos)
    {
        var newProp = Instantiate(desertProps[Random.Range(0, desertProps.Count)]);
                            
        newProp.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
        newProp.transform.position = pos;
        newProp.transform.localScale = Vector3.one * Random.Range(1,10);
        return newProp;
    }
}
