using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SnakePartsFollow : MonoBehaviour
{
    [SerializeField] private List<Transform> snakeParts;
    [SerializeField] private float minDistance = 12.5f;
    [SerializeField] private float moveSpeed = 10;
    [SerializeField][ReadOnly] private bool active = false;
    
    public void OnEnable()
    {
        for (var index = 1; index < snakeParts.Count; index++)
        {
            var snakePart = snakeParts[index];
            snakePart.parent = null;
            snakePart.position = transform.position;
        }

        active = true;

    }

    public void OnRelease()
    {
        active = false;
        for (var index = 1; index < snakeParts.Count; index++)
        {
            var snakePart = snakeParts[index];
            snakePart.position = transform.position;
            snakePart.parent = transform;
        }
    }

    private void Update()
    {
        if (!active)
            return;
        
        Move();
    }
    
    void Move()
    {
        for (int i = 1; i < snakeParts.Count; i++)
        {
            var curBodyPart = snakeParts[i];
            var PrevBodyPart = snakeParts[i - 1];

            var dis = Vector3.Distance(PrevBodyPart.position,curBodyPart.position);

            Vector3 newpos = PrevBodyPart.position;

            float T = Time.deltaTime * dis / minDistance * moveSpeed;

            if (T > 0.5f)
                T = 0.5f;
            curBodyPart.position = Vector3.Slerp(curBodyPart.position, newpos, T);
            curBodyPart.rotation = Quaternion.Slerp(curBodyPart.rotation, PrevBodyPart.rotation, T);
        }
    }
}
