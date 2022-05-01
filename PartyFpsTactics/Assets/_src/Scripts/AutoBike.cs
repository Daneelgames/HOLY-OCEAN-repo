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
            Game.Player.Movement.transform.parent.position = transform.position;
            Game.Player.Movement.transform.parent.parent = transform;
        
            while (!BuildingGenerator.Instance.IsLevelReady)
            {
                yield return new WaitForSeconds(Random.Range(1, 4));
                currentYTorque = Random.Range(-1f, 1f);
            }

            transform.position = Game.Player.Movement.transform.position;
            rb.drag = 1;
            rb.angularDrag = 1;
        }

        private void FixedUpdate()
        {
            if (BuildingGenerator.Instance.IsLevelReady)
            {
                return;
            }
        
            rb.AddForce(transform.forward * movementSpeed * Time.deltaTime, ForceMode.Acceleration);
        
            rb.AddTorque(0, currentYTorque * torqueForce * Time.deltaTime, 0);
        }
    
    }
}