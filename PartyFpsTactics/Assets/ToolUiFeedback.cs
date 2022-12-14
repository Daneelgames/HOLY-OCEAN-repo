using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Tools;
using UnityEngine;
using UnityEngine.UI;

public class ToolUiFeedback : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Image back;
    [SerializeField] private Image selectedFeedback;

    public void SetTool(ToolType tool, int amount)
    {
        if (tool != ToolType.Null && amount > 0)
            nameText.text = tool + " X" + amount;
        else
            SetEmpty();
    }
    
    public void SetEmpty()
    {
        nameText.text = String.Empty;
        //back.enabled = false;
    }
    public void SetSelected(bool selected)
    {
        selectedFeedback.enabled = selected;
    }
}
