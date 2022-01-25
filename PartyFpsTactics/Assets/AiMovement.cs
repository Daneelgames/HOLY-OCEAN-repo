using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiMovement : MonoBehaviour
{
    public Transform targetToFollow;
    public NavMeshAgent agent;
    
    private void Start()
    {
        FollowTarget(targetToFollow);
    }

    void FollowTarget(Transform target)
    {
        if (followTargetCoroutine != null)
            StopCoroutine(followTargetCoroutine);

        followTargetCoroutine = StartCoroutine(FollowTargetCoroutine(target));
    }

    private Coroutine followTargetCoroutine;
    IEnumerator FollowTargetCoroutine(Transform target)
    {
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
            agent.SetPath(path);
        }
    }
}
