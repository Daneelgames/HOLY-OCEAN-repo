using System.Collections;
using MrPink.PlayerSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink
{
    public class AutoBike : MonoBehaviour
    {
        public float movementSpeed = 100;
        public float torqueForce = 5;
        public Rigidbody rb;
        private float currentYTorque = 0;
        IEnumerator Start()
        {
            Game.LocalPlayer.Movement.transform.parent.position = transform.position;
            Game.LocalPlayer.Movement.transform.parent.parent = transform;
        

            transform.position = Game.LocalPlayer.Movement.transform.position;
            rb.drag = 1;
            rb.angularDrag = 1;
            yield break;
        }

        private void FixedUpdate()
        {
            rb.AddForce(transform.forward * movementSpeed * Time.deltaTime, ForceMode.Acceleration);
        
            rb.AddTorque(0, currentYTorque * torqueForce * Time.deltaTime, 0);
        }
    }
}