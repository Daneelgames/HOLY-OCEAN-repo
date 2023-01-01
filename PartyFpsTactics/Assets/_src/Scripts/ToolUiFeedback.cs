using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Tools;
using UnityEngine;
using UnityEngine.UI;

public class ToolUiFeedback : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Image Icon;
    [SerializeField] private Image selectedFeedback;

    public void SetTool(ToolType tool, int amount, Sprite sprite)
    {
        if (tool != ToolType.Null && amount > 0)
        {
            if (nameText)
                nameText.text = tool + " X" + amount;
            Icon.sprite = sprite;
        }
        else
            SetEmpty();
    }
    
    public void SetEmpty()
    {
        if (nameText)
            nameText.text = String.Empty;
        Icon.sprite = null;
        //back.enabled = false;
    }
    public void SetSelected(bool selected)
    {
        selectedFeedback.enabled = selected;
    }
}
