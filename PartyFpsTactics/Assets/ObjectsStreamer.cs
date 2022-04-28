using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;

public class ObjectsStreamer : MonoBehaviour
{
    public static ObjectsStreamer Instance;

    public float cullDistance = 150;
    public List<StreamableObject> StreamableObjects = new List<StreamableObject>();
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(Stream());
    }

    IEnumerator Stream()
    {
        int pauseCounter = 0;
        int pauseCounterMax = 30;
        while (true)
        {
            for (int i = 0; i < StreamableObjects.Count; i++)
            {
                var str = StreamableObjects[i];
                if (Vector3.Distance(Game.Player._mainCamera.transform.position, str.transform.position) > cullDistance)
                {
                    str.gameObject.SetActive(false);
                }
                else
                {
                    str.gameObject.SetActive(true);
                }

                if (pauseCounter < pauseCounterMax)
                {
                    pauseCounter++;
                }
                else
                {
                    yield return null;
                    pauseCounter = 0;
                }
            }
            yield return null;
        }
    }

    public void AddStreamable(StreamableObject str)
    {
        if (!StreamableObjects.Contains(str))
            StreamableObjects.Add(str);
    }
    
    public void RemoveStreamable(StreamableObject str)
    {
        if (StreamableObjects.Contains(str))
            StreamableObjects.Remove(str);
    }
}
