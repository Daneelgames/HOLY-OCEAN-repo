using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.WeaponsSystem;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MrPink
{
    public class PlayerInteractor : MonoBehaviour
    {
        public Camera cam;
        public LayerMask raycastMask;
        public float raycastDistance = 3;
        private InteractiveObject selectedIO;
        private Transform selectedIOTransform;
        public Rigidbody carryingPortableRb;
        private GameObject selectedPortable;
        public float throwPortableForce = 100;
        public float carryingPortablePower = 500;

        public string pickUpText = "PICK UP";
        public Text uiItemNameFeedback;
        public Text uiItemNameFeedbackOutline;
    
        [Range(0,1)]
        public float skipChance = 0.9f;
        [Range(0.01f,1)]
        public float selectedNameUpdateTime = 0.1f;
        private void Start()
        {
            StartCoroutine(RaycastInteractables());
            StartCoroutine(UpdateSelectedNameFeedback());
        }
    
        private void Update()
        {
            if (Game.Player.Health.health <= 0)
                return;

            if (selectedIOTransform)
            {
                uiItemNameFeedbackOutline.transform.position = cam.WorldToScreenPoint(selectedIOTransform.position);
            }
            else if (selectedPortable)
                uiItemNameFeedbackOutline.transform.position = cam.WorldToScreenPoint(selectedPortable.transform.position);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (carryingPortableRb)
                {
                    StopCoroutine(CarryPortableObjectCoroutine);
                
                    carryingPortableRb.interpolation = RigidbodyInterpolation.None;
                    carryingPortableRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    carryingPortableRb.useGravity = true;
                    carryingPortableRb = null;
                    return;
                }
            
                if (selectedIO)
                {
                    InteractableEventsManager.Instance.InteractWithIO(selectedIO);
                    return;
                }
            
                if (selectedPortable)
                {
                    CarryPortableObjectCoroutine = StartCoroutine(CarryPortableObject());
                }
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (carryingPortableRb)
                {
                    StopCoroutine(CarryPortableObjectCoroutine);
                    var tileAttack = carryingPortableRb.gameObject.GetComponent<TileAttack>();
                    if (tileAttack)
                        tileAttack.dangerous = true;
                    carryingPortableRb.interpolation = RigidbodyInterpolation.None;
                    carryingPortableRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    carryingPortableRb.useGravity = true;
                    carryingPortableRb.AddForce((carryingPortableRb.transform.position - cam.transform.position) * throwPortableForce, ForceMode.VelocityChange);
                    carryingPortableRb = null;
                }
            }
        }

        private Coroutine CarryPortableObjectCoroutine;
        IEnumerator CarryPortableObject()
        {
            var rb = selectedPortable.GetComponent<Rigidbody>();
        
            if (!rb)
                yield break;

            carryingPortableRb = rb;
            carryingPortableRb.useGravity = false;
        
            carryingPortableRb.interpolation = RigidbodyInterpolation.Interpolate;
            carryingPortableRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            while (true)
            {
                carryingPortableRb.AddForce((cam.transform.position + cam.transform.forward /* * 2 - cam.transform.up*/ - carryingPortableRb.transform.position) * carryingPortablePower * Time.deltaTime, ForceMode.Acceleration);
                if (Vector3.Distance(cam.transform.position, carryingPortableRb.transform.position) > raycastDistance)
                {
                    carryingPortableRb.useGravity = true;
                    carryingPortableRb = null;
                    yield break;
                }
                yield return null;
            }
        }

        IEnumerator RaycastInteractables()
        {
            Debug.Log("RAYCAST START ");
            while (true)
            {
                Debug.Log("RAYCAST TRY ");
                yield return null;

                if (Game.Player.Health.health <= 0 || carryingPortableRb)
                {
                    Debug.Log("RAYCAST NULL ");
                    if (selectedIO == null && selectedPortable == null)
                        continue;
                
                    selectedPortable = null;
                    selectedIO = null;
                    selectedIOTransform = null;
                    uiItemNameFeedback.text = String.Empty;
                    uiItemNameFeedbackOutline.text = String.Empty;
                }
            
                if (PhoneDialogueEvents.Instance != null && PhoneDialogueEvents.Instance.InCutScene)
                {
                    Debug.Log("RAYCAST NULL ");
                    if (selectedIO == null)
                        continue;
                
                    selectedIO = null;
                    selectedIOTransform = null;
                    uiItemNameFeedback.text = String.Empty;
                    uiItemNameFeedbackOutline.text = String.Empty;
                    continue;
                }
            
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, raycastDistance, raycastMask))
                {
                    Debug.Log("RAYCAST HIT " + hit.collider.gameObject.name);
                    if (hit.collider.gameObject.layer != 11)
                    {
                        if (hit.collider.gameObject.CompareTag(GameManager.Instance.portableObjectTag))
                        {
                            selectedPortable = hit.collider.gameObject;
                        
                            uiItemNameFeedback.text = pickUpText;
                            uiItemNameFeedbackOutline.text = pickUpText;
                        }
                        else
                        {
                            uiItemNameFeedback.text = String.Empty;
                            uiItemNameFeedbackOutline.text = String.Empty;   
                        }
                    
                        selectedIO = null;
                        selectedIOTransform = null;
                        Debug.Log("RAYCAST NULL ");
                        continue;
                    }

                    selectedPortable = null;
                
                    if (hit.collider.transform == selectedIOTransform)
                    {
                        Debug.Log("RAYCAST NULL ");
                        continue;
                    }
                
                    var newIO = hit.collider.gameObject.GetComponent<InteractiveObject>();
                    if (newIO)
                    {
                        Debug.Log("RAYCAST newIO " + newIO);
                        if (newIO.hc && newIO.hc.health <= 0 && selectedIO != null)
                        {
                            selectedIO = null;
                            selectedIOTransform = null;
                            uiItemNameFeedback.text = String.Empty;
                            uiItemNameFeedbackOutline.text = String.Empty;
                            continue;
                        }
                    
                        selectedIO = newIO;
                        selectedIOTransform = newIO.transform;
                    }
                }
                else
                {
                    Debug.Log("RAYCAST NULL ");
                    selectedPortable = null;
                    selectedIO = null;
                    selectedIOTransform = null;
                    uiItemNameFeedback.text = String.Empty;
                    uiItemNameFeedbackOutline.text = String.Empty;
                }
            }
        }


        IEnumerator UpdateSelectedNameFeedback()
        {
            while (true)
            {
                if (selectedIO && Random.value >= skipChance)
                    SetInteractionText(selectedIO.interactiveObjectName);
            
                yield return new WaitForSeconds(selectedNameUpdateTime);
            }
        }

        public void SetInteractionText(string text)
        {
            string newString = UppercaseRandomly(text);
            uiItemNameFeedback.text = newString;
            uiItemNameFeedbackOutline.text = newString;
        }
    
        string UppercaseRandomly(string s){
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            for (int i = 0; i < a.Length; i++)
            {
                if (Random.value >= 0.5f)
                    a[i] = char.ToUpper(a[i]);
                else
                    a[i] = char.ToLower(a[i]);
            }
            return new string(a);
        }
    }
}