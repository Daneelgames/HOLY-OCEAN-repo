using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using MrPink;
using UnityEngine;

public class SettingsGameWrapper : MonoBehaviour
{
    public static SettingsGameWrapper Instance;
    [SerializeField] private Transform menuTransform;
    [SerializeField] private Transform startGameButton;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnStartSinglePlayerClicked()
    {
        NetworkHudCanvasesMrp.Instance.StartLocalSinglePlayerGame();
        CloseMenu();
    }
    public void OnQuitGameClicked()
    {
        Application.Quit();
    }

    public void OpenMenu()
    {
        if (Game._instance && Game.LocalPlayer != null)
            startGameButton.gameObject.SetActive(false);
        else
            startGameButton.gameObject.SetActive(true);
        
        menuTransform.gameObject.SetActive(true);
    }
    public void CloseMenu()
    {
        menuTransform.gameObject.SetActive(false);
    }
}
