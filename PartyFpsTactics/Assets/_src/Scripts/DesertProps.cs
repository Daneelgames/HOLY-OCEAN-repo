using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DesertProps : MonoBehaviour
{
    public static DesertProps Instance;

    public List<DesertProp> desertProps;

    [Serializable]
    public class DesertProp
    {
        public GameObject prop;
        public Vector2 minMaxScale = new Vector2(1, 1);
    }
    private void Awake()
    {
        Instance = this;
    }

    public GameObject SpawnRandomProp(Vector3 pos)
    {
        var prop = desertProps[Random.Range(0, desertProps.Count)];
        var newProp = Instantiate(prop.prop);
                            
        newProp.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
        newProp.transform.position = pos;
        newProp.transform.localScale = Vector3.one * Random.Range(prop.minMaxScale.x,prop.minMaxScale.y);
        return newProp;
    }
}
