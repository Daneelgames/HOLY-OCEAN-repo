using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInStepUi : MonoBehaviour
{
    [SerializeField] private Transform selectedFeedback;
    [SerializeField] private Text _text;
    public void SetBuilding(BuildingData buildingData)
    {
        _text.text = buildingData.BuildingName;
    }

    public void SetSelected(bool selected)
    {
        selectedFeedback.gameObject.SetActive(selected);
    }
}
