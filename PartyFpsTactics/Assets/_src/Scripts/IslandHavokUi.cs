using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class IslandHavokUi : MonoBehaviour
{
    public static IslandHavokUi Instance;
    [SerializeField] private bool showBar = false;
    [SerializeField] private Image havokBar;
    [SerializeField] private Animator uiAnim;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("IslandHavokUi destroyed - there was another instance");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        HideBar();
    }

    public void ShowBar()
    {
        if (showBar == false)
            return;
        uiAnim.gameObject.SetActive(true);
    }
    public void HideBar()
    {
        uiAnim.gameObject.SetActive(false);
    }
    
    public void SetHavokFill(float fill)
    {
        if (showBar == false)
            return;
        havokBar.fillAmount = fill;
    }
}