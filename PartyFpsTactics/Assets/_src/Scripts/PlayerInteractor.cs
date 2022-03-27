using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerInteractor : MonoBehaviour
{
    public Camera cam;
    public LayerMask raycastMask;
    public float raycastDistance = 3;
    private Transform selectedIOTransform;
    private InteractiveObject selectedIO;

    public Text uiItemNameFeedback;
    
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
        if (selectedIOTransform)
            uiItemNameFeedback.transform.position = cam.WorldToScreenPoint(selectedIOTransform.position);

        if (Input.GetKeyDown(KeyCode.E) && selectedIO != null)
        {
            InteractableManager.Instance.InteractWithIO(selectedIO);
        }
    }

    IEnumerator RaycastInteractables()
    {
        while (true)
        {
            yield return null;
            
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, raycastDistance, raycastMask))
            {
                if (hit.collider.gameObject.layer != 11)
                {
                    uiItemNameFeedback.text = String.Empty;
                    selectedIO = null;
                    selectedIOTransform = null;
                    continue;
                }
                
                
                if (hit.collider.transform == selectedIOTransform)
                {
                    continue;
                }
                
                var newIO = hit.collider.gameObject.GetComponent<InteractiveObject>();
                if (newIO)
                {
                    selectedIO = newIO;
                    selectedIOTransform = newIO.transform;
                }
            }
            else
            {
                selectedIO = null;
                selectedIOTransform = null;
                uiItemNameFeedback.text = String.Empty;
            }
        }
    }


    IEnumerator UpdateSelectedNameFeedback()
    {
        while (true)
        {
            yield return new WaitForSeconds(selectedNameUpdateTime);
            if (selectedIO == null)
            {
                continue;
            }
            if (Random.value < skipChance)
                continue;
            
            string newString = UppercaseRandomly(selectedIO.interactiveObjectName);
            uiItemNameFeedback.text = newString;
        }
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