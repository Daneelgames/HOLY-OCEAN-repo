using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamableObject : MonoBehaviour
{
    public Rigidbody rb;
    public float rbStreamingDistance = 50;
    void Start()
    {
        ObjectsStreamer.Instance.AddStreamable(this);
    }
    void OnDestroy()
    {
        ObjectsStreamer.Instance.RemoveStreamable(this);
    }
}
