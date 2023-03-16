using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourcePlayerUi : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Image resourceIcon;
    [SerializeField] private Text resourceAmountText;
    private bool isShowing = false;
    private static readonly int Update = Animator.StringToHash("Update");
    private static readonly int Active = Animator.StringToHash("Show");

    public void SetResourceIcon(Sprite sprite)
    {
        resourceIcon.sprite = sprite;
    }

    public void SetAmount(int amount)
    {
        resourceAmountText.text = amount.ToString();
    }
    
    public void Show()
    {
        isShowing = true;
        anim.SetBool(Active, true);
    }
    public void Hide()
    {
        isShowing = false;
        anim.SetBool(Active, false);
    }

    public void UpdateResource()
    {
        if (isShowing)
            return;
        anim.SetTrigger(Update);
    }
}
