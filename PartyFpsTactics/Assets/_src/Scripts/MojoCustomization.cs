using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCharacterController;
using MrPink;
using UnityEngine;

public class MojoCustomization : MonoBehaviour
{
    public static MojoCustomization Instance;
    [SerializeField] private MojoLevelUi _mojoLevelUiPrefab;
    [SerializeField] private Transform mojoLevelUiIconsParent;
    [SerializeField] private Transform mojoCustomizationUiParent;
    private List<MojoLevelUi> spawnedMojoLevelsUi = new List<MojoLevelUi>();
    private bool isShowing = false;
    public bool IsShowing => isShowing;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenWindow()
    {
        isShowing = true;
        mojoCustomizationUiParent.gameObject.SetActive(true);
        var mojoLevelsCurrent = ScoringSystem.Instance.MojoLevels;
        for (var index = 0; index < mojoLevelsCurrent.Count; index++)
        {
            var mojoLevel = mojoLevelsCurrent[index];
            var newMojoLevelUi = Instantiate(_mojoLevelUiPrefab, mojoLevelUiIconsParent);
            spawnedMojoLevelsUi.Add(newMojoLevelUi);
            newMojoLevelUi.SetMojoLevel(mojoLevel, index);
        }
    }

    public void CloseWindow()
    {
        isShowing = false;
        mojoCustomizationUiParent.gameObject.SetActive(false);
        for (int i = 0; i < spawnedMojoLevelsUi.Count; i++)
        {
            Destroy(spawnedMojoLevelsUi[i].gameObject);
        }
        spawnedMojoLevelsUi.Clear();
    }
}
