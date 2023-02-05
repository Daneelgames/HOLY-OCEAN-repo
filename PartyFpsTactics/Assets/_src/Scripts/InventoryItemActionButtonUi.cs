using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemActionButtonUi : MonoBehaviour
{
    [SerializeField] private Text buttonName;
    [SerializeField] private Button _button;
    public Button GetButton => _button;

    public void SetButtonName(string nameStr)
    {
        buttonName.text = nameStr;
    }
}
