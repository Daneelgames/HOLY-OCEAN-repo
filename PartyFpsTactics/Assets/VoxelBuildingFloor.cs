using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen.Modify.Procedural;
using Sirenix.OdinInspector;
using UnityEngine;

public class VoxelBuildingFloor : MonoBehaviour
{
    [SerializeField] private ColliderToVoxel wallsColliders;
    [SerializeField] private ColliderToVoxel innerWallsColliders;
    [SerializeField] private ColliderToVoxel holesColliders;

    [Header("SETTINGS")] 
    [SerializeField] private float floorHeight = 7;
    [SerializeField] private float floorSizeX = 7;
    [SerializeField] private float floorSizeZ = 7;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountF = 1;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountR = 1;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountB = 1;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountL = 1;
    [BoxGroup("Inner Walls")] [Range(0,10)][SerializeField] private int innerWallsAmountX = 1;
    [BoxGroup("Inner Walls")] [Range(0,10)][SerializeField] private int innerWallsAmountZ = 1;

    [Header("RUNTIME")] 
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject ceiling;
    [SerializeField] private GameObject wallF;
    [SerializeField] private GameObject wallR;
    [SerializeField] private GameObject wallB;
    [SerializeField] private GameObject wallL;

    [SerializeField] private GameObject entranceHole;
    
    [SerializeField] private List<GameObject> holesF = new List<GameObject>();
    [SerializeField] private List<GameObject> holesR = new List<GameObject>();
    [SerializeField] private List<GameObject> holesB = new List<GameObject>();
    [SerializeField] private List<GameObject> holesL = new List<GameObject>();
    
    [SerializeField] private List<GameObject> innerWallsX = new List<GameObject>();
    [SerializeField] private List<GameObject> innerWallsZ = new List<GameObject>();
    public void CutVoxels()
    {
        wallsColliders.ApplyProceduralModifier();
        innerWallsColliders.ApplyProceduralModifier();
        
        
        
        holesColliders.ApplyProceduralModifier(true);
    }
 
    private void OnValidate()
    {
        ceiling.transform.localPosition = Vector3.up * floorHeight;
        floor.transform.localScale = new Vector3(floorSizeX, 1, floorSizeZ);
        ceiling.transform.localScale = new Vector3(floorSizeX, 1, floorSizeZ);
        
        wallF.transform.localPosition = new Vector3(0, floorHeight / 2, floorSizeZ/2);
        wallF.transform.localScale = new Vector3(floorSizeX, floorHeight, 1);
        wallR.transform.localPosition = new Vector3(floorSizeX/2, floorHeight / 2, 0);
        wallR.transform.localScale = new Vector3(1, floorHeight, floorSizeZ);
        wallB.transform.localPosition = new Vector3(0, floorHeight / 2, -floorSizeZ/2);
        wallB.transform.localScale = new Vector3(floorSizeX, floorHeight, 1);
        wallL.transform.localPosition = new Vector3(-floorSizeX/2, floorHeight / 2, 0);
        wallL.transform.localScale = new Vector3(1, floorHeight, floorSizeZ);

        
        ValidateInnerWallsAmount();
        ValidateInnerWalls(innerWallsX, 0);
        ValidateInnerWalls(innerWallsZ, 1);
        
        ValidateHolesAmount();
        ValidateHoles(holesF,0);
        ValidateHoles(holesR,1);
        ValidateHoles(holesB,2);
        ValidateHoles(holesL,3);
    }

    void ValidateInnerWalls(List<GameObject> innerWalls, int side) // 0f; 1r; 2f; 3f
    {
        float spaceBetweenWalls = 1;
        for (var i = 0; i < innerWalls.Count; i++)
        {
            var wall = innerWalls[i];
            
            float newScaleX = 1;
            float newScaleZ = 1;
            float newPosX = 0;
            float newPosZ = 0;
            
            if (side == 0)
            {
                newScaleX = floorSizeX;
                newPosX = 0;
                spaceBetweenWalls = floorSizeX / innerWalls.Count;
                newPosZ = -floorSizeZ / 2 +  spaceBetweenWalls * (i + 1);

            }
            else if (side == 1)
            {
                newScaleZ = floorSizeZ;
                newPosZ = 0;
                spaceBetweenWalls = floorSizeZ / innerWalls.Count;
                newPosX = -floorSizeX / 2 +  spaceBetweenWalls * (i + 1);
            }
            
            wall.transform.localScale = new Vector3(newScaleX, floorHeight, newScaleZ);
            wall.transform.localPosition = new Vector3(newPosX, floorHeight/2, newPosZ);
        }
    }
    void ValidateHoles(List<GameObject> holes, int side) // 0f; 1r; 2f; 3f
    {
        var targetWall = wallF;
        Vector3 spreadHolesDirection = Vector3.zero;
        
        float spaceBetweenWindows = 1;
        float directionSize = 20;
        
        switch (side)
        {
            case 1:
                targetWall = wallR;
                spreadHolesDirection = Vector3.forward;
                spaceBetweenWindows = floorSizeZ / holes.Count;
                directionSize = floorSizeZ;
                break;
            case 2:
                targetWall = wallB;
                spreadHolesDirection = Vector3.right;
                spaceBetweenWindows = floorSizeX / holes.Count;
                directionSize = floorSizeX;
                break;
            case 3:
                targetWall = wallL;
                spreadHolesDirection = Vector3.forward;
                spaceBetweenWindows = floorSizeZ / holes.Count;
                directionSize = floorSizeZ;
                break;
            default:
                targetWall = wallF;
                spreadHolesDirection = Vector3.right;
                spaceBetweenWindows = floorSizeX / holes.Count;
                directionSize = floorSizeX;
                break;
        }

        
        for (var i = 0; i < holes.Count; i++)
        {
            var hole = holes[i];
            if (i == 0)
            {
                hole.SetActive(false);
            }

            float newScaleX = hole.transform.localScale.x;
            float newScaleZ = hole.transform.localScale.z;
            
            if (side == 0 || side == 2)
                newScaleX = directionSize / (holes.Count + 2);
            else
                newScaleZ = directionSize / (holes.Count + 2);
            
            hole.transform.localScale = new Vector3(newScaleX, floorHeight / 3, newScaleZ);
            hole.transform.localPosition = targetWall.transform.localPosition - spreadHolesDirection * directionSize / 2 + spreadHolesDirection * spaceBetweenWindows * i;
        }
    }

    void ValidateInnerWallsAmount()
    {
        while (innerWallsAmountX > innerWallsX.Count)
        {
            // add wall
            var newWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newWall.transform.parent = innerWallsColliders.transform;
            newWall.name = "Inner Wall";
            newWall.layer = 2;
            newWall.GetComponent<Collider>().isTrigger = true;
            
            innerWallsX.Add(newWall);
        }
        while (innerWallsAmountZ > innerWallsZ.Count)
        {
            // add wall
            var newWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newWall.transform.parent = innerWallsColliders.transform;
            newWall.name = "Inner Wall";
            newWall.layer = 2;
            newWall.GetComponent<Collider>().isTrigger = true;
            
            innerWallsZ.Add(newWall);
        }

        while (innerWallsAmountX < innerWallsX.Count)
        {
            var last = innerWallsX[^1];
            innerWallsX.Remove(last);
            if (Application.isEditor && Application.isPlaying == false)
                DestroyImmediate(last);
            else
                Destroy(last);
        }
        while (innerWallsAmountZ < innerWallsZ.Count)
        {
            var last = innerWallsZ[^1];
            innerWallsZ.Remove(last);
            if (Application.isEditor && Application.isPlaying == false)
                DestroyImmediate(last);
            else
                Destroy(last);
        }
    }
    void ValidateHolesAmount()
    {
        while (holesAmountF > holesF.Count)
        {
            // add hole
            var newHole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newHole.transform.parent = holesColliders.transform;
            newHole.name = "HOLE";
            newHole.layer = 2;
            newHole.GetComponent<Collider>().isTrigger = true;
            
            holesF.Add(newHole);
        }

        while (holesAmountF < holesF.Count)
        {
            var last = holesF[^1];
            holesF.Remove(last);
            
            if (Application.isEditor && Application.isPlaying == false)
                DestroyImmediate(last);
            else
                Destroy(last);
        }
        while (holesAmountR > holesR.Count)
        {
            // add hole
            var newHole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newHole.transform.parent = holesColliders.transform;
            newHole.name = "HOLE";
            newHole.layer = 2;
            newHole.GetComponent<Collider>().isTrigger = true;
            
            holesR.Add(newHole);
        }

        while (holesAmountR < holesR.Count)
        {
            var last = holesR[^1];
            holesR.Remove(last);
            
            if (Application.isEditor && Application.isPlaying == false)
                DestroyImmediate(last);
            else
                Destroy(last);
        }
        while (holesAmountB > holesB.Count)
        {
            // add hole
            var newHole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newHole.transform.parent = holesColliders.transform;
            newHole.name = "HOLE";
            newHole.layer = 2;
            newHole.GetComponent<Collider>().isTrigger = true;
            
            holesB.Add(newHole);
        }

        while (holesAmountB < holesB.Count)
        {
            var last = holesB[^1];
            holesB.Remove(last);
            
            if (Application.isEditor && Application.isPlaying == false)
                DestroyImmediate(last);
            else
                Destroy(last);
        }
        while (holesAmountL > holesL.Count)
        {
            // add hole
            var newHole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newHole.transform.parent = holesColliders.transform;
            newHole.name = "HOLE";
            newHole.layer = 2;
            newHole.GetComponent<Collider>().isTrigger = true;
            
            holesL.Add(newHole);
        }

        while (holesAmountL < holesL.Count)
        {
            var last = holesL[^1];
            holesL.Remove(last);
            
            if (Application.isEditor && Application.isPlaying == false)
                DestroyImmediate(last);
            else
                Destroy(last);
        }
    }
}
