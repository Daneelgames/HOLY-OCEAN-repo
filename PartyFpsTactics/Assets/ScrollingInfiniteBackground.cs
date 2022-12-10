using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingInfiniteBackground : MonoBehaviour
{
    [SerializeField] private Transform f0;
    [SerializeField] private Transform f1;
    [SerializeField] private Vector3 movement = new Vector3(10,0,0);
    private void Update()
    {
        f0.transform.localPosition += movement * Time.deltaTime;
        f1.transform.localPosition += movement * Time.deltaTime;
        if (f0.transform.position.x > 500)
        {
            f0.transform.localPosition = new Vector3(-500,2,0);
            f1.transform.localPosition = new Vector3(-500,2,0);
        }
    }
}
