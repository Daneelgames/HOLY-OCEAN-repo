using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitualPageUi : MonoBehaviour
{
    [SerializeField] private Transform selectedFeedback;

    public void SetSelected(bool select)
    {
        selectedFeedback.gameObject.SetActive(select);
    }
}
