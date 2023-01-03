using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingInfiniteBackground : MonoBehaviour
{
    [SerializeField] private Transform f0;
    [SerializeField] private Transform f1;
    [SerializeField] private Vector3 movement = new Vector3(10,0,0);
    [SerializeField] private float max = 500;
    private void Update()
    {
        f0.transform.localPosition += movement * Time.deltaTime;
        f1.transform.localPosition += movement * Time.deltaTime;
        if (f0.transform.localPosition.x > max)
        {
            f0.transform.localPosition = new Vector3(-max,f0.transform.localPosition.y,0);
            f1.transform.localPosition = new Vector3(-max,f1.transform.localPosition.y,0);
        }
    }
}
