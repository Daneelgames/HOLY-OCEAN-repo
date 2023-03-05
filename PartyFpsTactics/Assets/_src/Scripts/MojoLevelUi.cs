using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using UnityEngine.UI;

public class MojoLevelUi : MonoBehaviour
{
    [SerializeField] private Text levelName;
    [SerializeField] private Text leftHandSlot;
    [SerializeField] private Text rightHandSlot;

    private ScoringSystem.MojoLevel savedMojoLevel;
    public void SetMojoLevel(ScoringSystem.MojoLevel mojoLevel, int levelIndex)
    {
        savedMojoLevel = mojoLevel;
        leftHandSlot.text = mojoLevel.HandLTool?.tool.ToString().ToUpper();
        rightHandSlot.text = mojoLevel.HandRTool?.tool.ToString().ToUpper();
        levelName.text = mojoLevel.minDamage.ToString();
    }

    public void ClickedHandL()
    {
        // tool selected
        // show selected tool info
        // highlight suitable items in shop
        
        //savedMojoLevel.HandLTool
    }
    public void ClickedHandR()
    {
        
    }

    public void ClickedShopItem()
    {
        
    }
}