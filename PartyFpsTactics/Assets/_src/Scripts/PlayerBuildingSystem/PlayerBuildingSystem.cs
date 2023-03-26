using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerBuildingSystem : NetworkBehaviour
{
    public static PlayerBuildingSystem Instance;

    [SerializeField] [ReadOnly] private int selectedRitualStepIndex;
    [SerializeField] [ReadOnly] private int selectedBuildingIndex;
    [SerializeField] private List<RitualStep> ritualSteps;
    public GameObject testBuildingPrefab;
    public GameObject testTilePrefab;

    [BoxGroup("PLACING OPTIONS")][SerializeField] private float buildingDistanceMax = 100;
    [BoxGroup("PLACING OPTIONS")][SerializeField] private float noRaycastBuildingMediumDistance = 50;
    [BoxGroup("PLACING OPTIONS")][SerializeField] private float buildingDistanceMin = 20;
    [BoxGroup("PLACING OPTIONS")][SerializeField] private float buildingRotationSpeed = 10;
    [BoxGroup("PLACING OPTIONS")][SerializeField] private float tileMaxSpawnDistance = 10;
    
    private BuildingData currentPlacingBuildingData;
    private GameObject currentPlacingBuildingGo;
    private bool inBuildingMode = false;
    
    [Header("UI")] 
    [SerializeField] private Transform buildingSystemCanvas;

    [SerializeField] private List<RitualPageUi> _ritualPageUis;
    [SerializeField] private List<BuildingInStepUi> buildingInStepUis;
    [SerializeField] private List<ResourcePlayerUi> resourceCostIcons;
    // есть список всех построек
    // в каждой постройке указано какие постройки она анлочит
    // анлок == получить "рецепт" постройки
    // игрок видит анлокнутые постройки в окошке
    // в том же окошке особо отмечены постройки, которые игрок еще не возвел 

    [Serializable]
    class RitualStep
    {
        public List<BuildingData> BuildingsRecipe;
    }

    [SerializeField]
    [ReadOnly] private List<RitualStepBuiltIndexes> ritualStepBuiltIndexesList;


    [Serializable]
    class RitualStepBuiltIndexes
    {
        public List<int> BuiltIndexes = new List<int>();
        public List<Vector3> BuiltPositions = new List<Vector3>();
    }

    public bool InBuildingMode => inBuildingMode;
    public override void OnStartClient()
    {
        base.OnStartClient();

        // TODO: REPLACE WITH SAVING SYSTEM
        ritualStepBuiltIndexesList = new List<RitualStepBuiltIndexes>(ritualSteps.Count);
        foreach (var builtData in ritualStepBuiltIndexesList)
        {
            builtData.BuiltIndexes = new List<int>();
            builtData.BuiltPositions = new List<Vector3>();
        }
        
        Instance = this;
        UpdateBuildingWindow();
    }

    private void Update()
    {
        if (Game._instance == null || Game.LocalPlayer == null || Game.LocalPlayer.Health.health < 1 || SettingsGameWrapper.Instance.IsOpened)
            return;
        
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleBuildingMode();
        
        if (inBuildingMode == false)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            SelectPreviousRitualStep();
        if (Input.GetKeyDown(KeyCode.E))
            SelectNextRitualStep();
        if (Input.mouseScrollDelta.y > 0)
            SelectPreviousBuildingInRitualStep();
        if (Input.mouseScrollDelta.y < 0)
            SelectNextBuildingInRitualStep();

        if (Input.GetMouseButtonDown(0))
        {
            PlaceBuilding();
        }

    }
    public void ToggleBuildingMode()
    {
        inBuildingMode = !inBuildingMode;
        UpdateBuildingWindow();   
    }

    void UpdateBuildingWindow()
    {
        buildingSystemCanvas.gameObject.SetActive(inBuildingMode);
        if (inBuildingMode)
        {
            BuildingResources.Instance.EnterBuildingMode();
            UpdateBuildingWindowContent();
        }
        else
            BuildingResources.Instance.ExitBuildingMode();
    }

    void SelectPreviousRitualStep()
    {
        selectedRitualStepIndex--;
        if (selectedRitualStepIndex < 0)
            selectedRitualStepIndex = 0;

        selectedBuildingIndex = 0;
        
        UpdateBuildingWindowContent();
    }
    void SelectNextRitualStep()
    {
        selectedRitualStepIndex++;
        if (selectedRitualStepIndex >= ritualSteps.Count)
            selectedRitualStepIndex = ritualSteps.Count - 1;
         
        selectedBuildingIndex = 0;
        
        UpdateBuildingWindowContent();
    }

    void SelectPreviousBuildingInRitualStep()
    {
        selectedBuildingIndex--;

        if (selectedBuildingIndex < 0)
            selectedBuildingIndex = 0;
        
        UpdateBuildingWindowContent();
    }
    void SelectNextBuildingInRitualStep()
    {
        selectedBuildingIndex++;

        if (selectedBuildingIndex >= ritualSteps[selectedRitualStepIndex].BuildingsRecipe.Count)
            selectedBuildingIndex = ritualSteps[selectedRitualStepIndex].BuildingsRecipe.Count - 1;

        UpdateBuildingWindowContent();
    }

    void UpdateBuildingWindowContent()
    {
        for (int i = 0; i < _ritualPageUis.Count; i++)
        {
            _ritualPageUis[i].SetSelected(i == selectedRitualStepIndex);
        }

        for (int i = 0; i < buildingInStepUis.Count; i++)
        {
            if (selectedRitualStepIndex > ProgressionManager.Instance.CurrentActiveRitualStep || i >= ritualSteps[selectedRitualStepIndex].BuildingsRecipe.Count)
            {
                buildingInStepUis[i].gameObject.SetActive(false);
                continue;
            }

            buildingInStepUis[i].gameObject.SetActive(true);
            buildingInStepUis[i].SetBuilding(ritualSteps[selectedRitualStepIndex].BuildingsRecipe[i]);
            buildingInStepUis[i].SetSelected(i == selectedBuildingIndex);
        }

        var buildingData = ritualSteps[selectedRitualStepIndex].BuildingsRecipe[selectedBuildingIndex];
        
        for (int i = 0; i < resourceCostIcons.Count; i++)
        {
            if (i >= buildingData.Recipe.Count)
            {
                resourceCostIcons[i].gameObject.SetActive(false);
                continue;
            }
            
            resourceCostIcons[i].gameObject.SetActive(true);
            resourceCostIcons[i].SetResourceIcon(BuildingResources.GetResourceSprite(buildingData.Recipe[i].Resource));
            resourceCostIcons[i].SetAmount(buildingData.Recipe[i].Amount);
            resourceCostIcons[i].Show();
        }
    }

    public void CancelInput()
    {
        if (currentPlacingBuildingGo != null)
        {
            StopCoroutine(placingBuildingCoroutine);
        }

        if (currentPlacingBuildingGo != null)
        {
            Destroy(currentPlacingBuildingGo);
        }
    }

    /*
    void SpawnTile()
    {
        Transform camTransform = Game.LocalPlayer.MainCamera.transform;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out var hit, tileMaxSpawnDistance,
            GameManager.Instance.AllSolidsMask) == false)
            return;
        

        var newTile = Instantiate(testTilePrefab);
        var spawnPos = new Vector3(Mathf.RoundToInt(hit.point.x),Mathf.RoundToInt(hit.point.y), Mathf.RoundToInt(hit.point.z));
        newTile.transform.position = spawnPos;
        newTile.transform.Rotate(new Vector3(90 * Random.Range(0,4),90 * Random.Range(0,4),90 * Random.Range(0,4)));
        
        IslandSpawner.Instance.GetClosestTileBuilding(transform.position)?.AddToDisconnectedTilesFolder(newTile.transform);
    }
    */

    void PlaceBuilding()
    {
        if (placingBuildingCoroutine != null && currentPlacingBuildingGo != null)
        {
            // place building;
            StopCoroutine(placingBuildingCoroutine);
            GameManager.Instance.SetLayerRecursively(currentPlacingBuildingGo.transform, 6); // solids
            
            Debug.LogError("При постройке нужно сохранять инфу о том индекс какого здания на каком шаге ритуала построен на какой позиции");
            
            ritualStepBuiltIndexesList[selectedRitualStepIndex].BuiltIndexes.Add(selectedBuildingIndex);
            ritualStepBuiltIndexesList[selectedRitualStepIndex].BuiltPositions.Add(currentPlacingBuildingGo.transform.position);
            // ^^^ NEED TO SAVE THIS LIST ^^^
            // SaveBuilt();
            currentPlacingBuildingGo.SetActive(true);
            currentPlacingBuildingGo = null;
            return;
        }
        
        // spawn new building
        currentPlacingBuildingData = ritualSteps[selectedRitualStepIndex].BuildingsRecipe[selectedBuildingIndex];
        currentPlacingBuildingGo = Instantiate(testBuildingPrefab);
        GameManager.Instance.SetLayerRecursively(currentPlacingBuildingGo.transform, 31); // ignore collision
        
        placingBuildingCoroutine = StartCoroutine(PlacingBuilding());
    }

    private Coroutine placingBuildingCoroutine;

    IEnumerator PlacingBuilding()
    {
        Transform camTransform = Game.LocalPlayer.MainCamera.transform;
        float t = 0.3f;
        while (true)
        {
            yield return null;
            
            t -= Time.deltaTime;
            if (t < 0)
            {
                t = 0.3f;
                currentPlacingBuildingGo.SetActive(!currentPlacingBuildingGo.activeInHierarchy);
            }
            
            if (Input.GetKey(KeyCode.R))
                currentPlacingBuildingGo.transform.Rotate(0,buildingRotationSpeed * Time.deltaTime, 0);
            
            if (Physics.Raycast(camTransform.position, camTransform.forward, out var hit, buildingDistanceMax,
                GameManager.Instance.AllSolidsMask) == false)
            {
                currentPlacingBuildingGo.transform.position = camTransform.position + camTransform.forward * noRaycastBuildingMediumDistance;
                continue;
            }
            
            if (Vector3.Distance(hit.point, camTransform.position) < buildingDistanceMin)
                continue;
            
            currentPlacingBuildingGo.transform.position = hit.point + Vector3.up;
            
            
        }
    }
}
