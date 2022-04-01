using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueWindowInterface : MonoBehaviour
{
    public static DialogueWindowInterface Instance;
    public GameObject phoneVisual;
    public GameObject logoEye;
    public GameObject playerAnswerButtons;
    
    public Text nameText;
    public Text messageText;
    public Text playerAnswerText;

    public Transform phoneInactiveTransform;
    public Transform phoneActiveTransform;

    public bool dialogueWindowActive = false;

    public AudioSource phoneAu;
    public AudioClip messageNotificationClip;
    public AudioClip playerAnswerClip;

    private void Start()
    {
        Instance = this;
        TogglePlayerAnswerButtons(false);
    }
    
    private void Update()
    {
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                ToggleDialogueWindow(!dialogueWindowActive);
        }
        if (!dialogueWindowActive)
            return;
        
        if (Input.GetKeyDown(KeyCode.E))
            PlayerAnswered(true);
        if (Input.GetKeyDown(KeyCode.Q))
            PlayerAnswered(false);
    }

    public void TogglePlayerAnswerButtons(bool active)
    {
        playerAnswerButtons.SetActive(active);
    }
    public void NewMessage(string _nameText, string _messageText, bool clearPlayerAnswer)
    {
        nameText.text = _nameText;
        messageText.text = _messageText;
        phoneAu.clip = messageNotificationClip;
        phoneAu.Play();
        
        if (clearPlayerAnswer)
        {
            playerAnswerText.text = "";
            playerAnswerText.color = Color.white;
        }
    }

    private void PlayerAnswered(bool positiveAnswer)
    {
        if (!ProceduralCutscenesManager.Instance.CanAnswer)
            return;
        
        if (positiveAnswer)
        {
            playerAnswerText.text = ":-)";
            playerAnswerText.color = new Color(0.86f, 0.37f, 0.57f);
        }
        else
        {
            playerAnswerText.text = ":-(";
            playerAnswerText.color = new Color(0.38f, 0.42f, 0.58f);
        }

        phoneAu.clip = playerAnswerClip;
        phoneAu.Play();
        ProceduralCutscenesManager.Instance.PlayerAnswered(positiveAnswer);
    }

    public void ToggleDialogueWindow(bool active)
    {
        if (toggleDialogueWIndowCoroutine != null)
            StopCoroutine(toggleDialogueWIndowCoroutine);
        
        toggleDialogueWIndowCoroutine = StartCoroutine(ToggleDialogueWindowIEnumerator(active));
    }

    private Coroutine toggleDialogueWIndowCoroutine;
    IEnumerator ToggleDialogueWindowIEnumerator(bool active)
    {
        Transform targetTransform = null;
        targetTransform = active ? phoneActiveTransform : phoneInactiveTransform;

        dialogueWindowActive = active;
        float t = 0;

        if (active)
        {
            logoEye.SetActive(false);
            phoneVisual.SetActive(true);
            yield return null;
            logoEye.SetActive(true);
        }
        
        while (t < 1)
        {
            yield return null;
            phoneVisual.transform.position = Vector3.Lerp(phoneVisual.transform.position, targetTransform.position, t / 1);
            phoneVisual.transform.rotation = Quaternion.Slerp(phoneVisual.transform.rotation, targetTransform.rotation, t / 1);
            t += Time.deltaTime;
        }

        if (!active)
            phoneVisual.SetActive(false);
        
        toggleDialogueWIndowCoroutine = null;
    }
}