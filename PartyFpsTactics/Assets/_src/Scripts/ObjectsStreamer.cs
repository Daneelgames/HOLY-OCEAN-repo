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
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        int pauseCounter = 0;
        int pauseCounterMax = 30;
        float distance = 0;
        StreamableObject str;
        while (true)
        {
            for (int i = 0; i < StreamableObjects.Count; i++)
            {
                str = StreamableObjects[i];
                distance = Vector3.Distance(Game.LocalPlayer.MainCamera.transform.position, str.transform.position);
                if (distance > cullDistance)
                {
                    str.gameObject.SetActive(false);
                }
                else
                {
                    str.gameObject.SetActive(true);

                    if (str.rb)
                    {
                        if(distance > str.rbStreamingDistance)
                            str.rb.isKinematic = true;
                        else
                            str.rb.isKinematic = false;
                    }
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