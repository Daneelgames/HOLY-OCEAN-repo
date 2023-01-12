using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleRuntimeInspector : MonoBehaviour
{
  [SerializeField] private Transform inspectorParent;

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Home))
      inspectorParent.gameObject.SetActive(!inspectorParent.gameObject.activeInHierarchy);
  }
}