using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Tools;
using UnityEngine;
using UnityEngine.UI;

public class ToolUiFeedback : MonoBehaviour
{
    [SerializeField] private Text amountText;
    [SerializeField] private Image Icon;
    [SerializeField] private Image selectedFeedback;
    [SerializeField] private GameObject wholeVisual;

    public void SetTool(ToolType tool, int amount, Sprite sprite)
    {
        if (tool != ToolType.Null && amount > 0)
        {
            if (amount > 1)
                amountText.text = "X" + amount;
            else
                amountText.text = String.Empty;
            Icon.sprite = sprite;
            wholeVisual.SetActive(true);
        }
        else
            SetEmpty();
    }
    
    void SetEmpty()
    {
        wholeVisual.SetActive(false);
        amountText.text = String.Empty;
        Icon.sprite = null;
        //back.enabled = false;
    }
    public void SetSelected(bool selected)
    {
        selectedFeedback.enabled = selected;
    }
}
