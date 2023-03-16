using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourcePlayerUi : MonoBehaviour
{
    [SerializeField] private Image resourceIcon;
    [SerializeField] private Text resourceAmountText;

    public void SetResourceIcon(Sprite sprite)
    {
        resourceIcon.sprite = sprite;
    }
}
