using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class IslandHavokUi : MonoBehaviour
{
    public static IslandHavokUi Instance;
    [SerializeField] private Image havokBar;
    [SerializeField] private Animator uiAnim;
    private static readonly int Active = Animator.StringToHash("Active");
    private static readonly int Update = Animator.StringToHash("Update");

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
        uiAnim.SetBool(Active, true);
    }
    public void HideBar()
    {
        uiAnim.SetBool(Active, false);
    }

    public void BlinkHavokBar()
    {
        uiAnim.SetTrigger(Update);
    }
    
    public void SetHavokFill(float fill)
    {
        havokBar.fillAmount = fill;
    }
}