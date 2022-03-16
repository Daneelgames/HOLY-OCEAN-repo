using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    private Transform rotateVisual;
    private bool collected = false;
    void Start()
    {
        rotateVisual = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        rotateVisual.transform.localEulerAngles += Vector3.up * 100 * Time.deltaTime;
    }

    void OnTriggerEnter(Collider coll)
    {
        if (collected)
            return;
        
        if (coll.gameObject == Player.GameObject)
        {
            collected = true;
            GameManager.Instance.LevelCompleted();
        }
    }
}