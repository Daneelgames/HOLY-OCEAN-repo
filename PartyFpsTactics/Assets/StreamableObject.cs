using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamableObject : MonoBehaviour
{
    void Start()
    {
        ObjectsStreamer.Instance.AddStreamable(this);
    }
    void OnDestroy()
    {
        ObjectsStreamer.Instance.RemoveStreamable(this);
    }
}
