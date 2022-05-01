using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    void Update()
    {
        if (!target)
            return;
        
        transform.position = target.position;
    }
}
