using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
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
        if (Player.Health.health <= 0)
            return;

        if (selectedIOTransform)
        {
            uiItemNameFeedbackOutline.transform.position = cam.WorldToScreenPoint(selectedIOTransform.position);
        }

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

            if (Player.Health.health <= 0)
            {
                if (selectedIO == null)
                    continue;
                selectedIO = null;
                selectedIOTransform = null;
                uiItemNameFeedback.text = String.Empty;
                uiItemNameFeedbackOutline.text = String.Empty;
            }
            
            if (ProceduralCutscenesManager.Instance.InCutScene)
            {
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
                if (hit.collider.gameObject.layer != 11)
                {
                    uiItemNameFeedback.text = String.Empty;
                    uiItemNameFeedbackOutline.text = String.Empty;
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