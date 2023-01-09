using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Rigidbody ownRigidbody;
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null || other.attachedRigidbody.isKinematic) return;
        if (other.attachedRigidbody == Game.LocalPlayer.Movement.rb)
            Game.LocalPlayer.Movement.SetMovingPlatform(ownRigidbody);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == null || other.attachedRigidbody.isKinematic) return;
        if (other.attachedRigidbody == Game.LocalPlayer.Movement.rb)
            Game.LocalPlayer.Movement.SetMovingPlatform(ownRigidbody, true);
    }
}
