using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen;
using Fraktalia.VoxelGen.Modify.Procedural;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelBuildingFloor : MonoBehaviour
{
    [SerializeField] private ColliderToVoxel wallsColliders;
    [SerializeField] private ColliderToVoxel innerWallsColliders;
    [SerializeField] private ColliderToVoxel holesColliders;
    [Header("SETTINGS")] 
    [BoxGroup("Size")][Range(6,15)][SerializeField] private int floorHeight = 7;
    public int GetHeight => floorHeight;
    [BoxGroup("Size")][Range(10,50)][SerializeField] private int floorSizeX = 7;
    public Vector3Int LevelSize => new Vector3Int(floorSizeX, floorHeight, floorSizeZ);
    [BoxGroup("Size")][Range(10,50)][SerializeField] private int floorSizeZ = 7;
    [BoxGroup("Inner Walls")] [Range(0,3)][SerializeField] private int innerWallsAmountX = 1;
    [BoxGroup("Inner Walls")] [Range(0,3)][SerializeField] private int innerWallsAmountZ = 1;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountF = 1;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountR = 1;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountB = 1;
    [BoxGroup("Holes")] [Range(1,10)][SerializeField] private int holesAmountL = 1;
    

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
    [SerializeField] private List<GameObject> innerHolesX = new List<GameObject>();
    [SerializeField] private List<GameObject> innerWallsZ = new List<GameObject>();
    [SerializeField] private List<GameObject> innerHolesZ = new List<GameObject>();
    
    
    [Serializable] 
    public class VoxelFloorRandomSettings
    {
        public int floorHeight;
        public int floorSizeX;
        public int floorSizeZ;
        public int innerWallsAmountX;
        public int innerWallsAmountZ;
        public int holesAmountF;
        public int holesAmountR;
        public int holesAmountB;
        public int holesAmountL;
    }

    public void SetSettings(VoxelFloorRandomSettings voxelFloorRandomSettings)
    {
        floorHeight = voxelFloorRandomSettings.floorHeight;
        floorSizeX = voxelFloorRandomSettings.floorSizeX;
        floorSizeZ = voxelFloorRandomSettings.floorSizeZ;
        innerWallsAmountX = voxelFloorRandomSettings.innerWallsAmountX;
        innerWallsAmountZ = voxelFloorRandomSettings.innerWallsAmountZ;
        holesAmountF = voxelFloorRandomSettings.holesAmountF;
        holesAmountR = voxelFloorRandomSettings.holesAmountR;
        holesAmountB = voxelFloorRandomSettings.holesAmountB;
        holesAmountL = voxelFloorRandomSettings.holesAmountL;
        
        ConstructFloor();
    }
    public void CutVoxels(VoxelGenerator voxelGenerator)
    {
        Debug.Log("CutVoxels building floor");
        wallsColliders.TargetGenerator = voxelGenerator;
        innerWallsColliders.TargetGenerator = voxelGenerator;
        holesColliders.TargetGenerator = voxelGenerator;
        
        wallsColliders.ApplyProceduralModifier();
        innerWallsColliders.ApplyProceduralModifier();
        
        holesColliders.ApplyProceduralModifier(true);
    }

    
    private void ConstructFloor()
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
        ValidateInnerWalls(innerWallsX, innerHolesX, 0);
        ValidateInnerWalls(innerWallsZ, innerHolesZ,1);
        
        ValidateHolesAmount();
        ValidateHoles(holesF,0);
        ValidateHoles(holesR,1);
        ValidateHoles(holesB,2);
        ValidateHoles(holesL,3);
    }

    void ValidateInnerWalls(List<GameObject> innerWalls, List<GameObject> innerHoles , int side) // 0f; 1r; 2f; 3f
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
                
                newScaleX /= i+1;
                
                newPosX = 0;
                spaceBetweenWalls = (floorSizeZ - 2) / innerWalls.Count;
                if (innerWalls.Count > 1)
                    newPosZ = -floorSizeZ / 2 + spaceBetweenWalls * (i + 0.5f);
                else
                    newPosZ = 0;

            }
            else if (side == 1)
            {
                newScaleZ = floorSizeZ;
                
                newScaleZ /= i+1;
                
                newPosZ = 0;
                spaceBetweenWalls = (floorSizeX - 2) / innerWalls.Count;
                if (innerWalls.Count > 1)
                    newPosX = -floorSizeX / 2 +  spaceBetweenWalls * (i + 0.5f);
                else
                    newPosX = 0;
            }
            
            wall.transform.localScale = new Vector3(newScaleX, floorHeight, newScaleZ);
            wall.transform.localRotation = Quaternion.identity;
            wall.transform.localPosition = new Vector3(newPosX, floorHeight/2, newPosZ);
            var hole = innerHoles[i];
            int iii = i; /*if (iii == 0) iii = 1;*/
            
            if (side == 0)
            {
                hole.transform.localScale = new Vector3(3, wall.transform.localScale.y/2, wall.transform.localScale.z);
                hole.transform.localPosition = wall.transform.localPosition + new Vector3(-wall.transform.localScale.x/3, 0, 0) + Vector3.right * 1.2f * (iii);
            }
            else if (side == 1)
            {
                hole.transform.localScale = new Vector3(wall.transform.localScale.x, wall.transform.localScale.y/2, 3);
                hole.transform.localPosition = wall.transform.localPosition + new Vector3(0, 0, -wall.transform.localScale.z/3) + Vector3.forward * 1.2f * (iii);
            }
            hole.transform.localRotation = Quaternion.identity;
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
            
            hole.transform.localRotation = Quaternion.identity;
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
            
            var newHole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newHole.transform.parent = holesColliders.transform;
            newHole.name = "Inner Hole";
            newHole.layer = 2;
            newHole.GetComponent<Collider>().isTrigger = true;

            innerHolesX.Add(newHole);
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
            
            var newHole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newHole.transform.parent = holesColliders.transform;
            newHole.name = "Inner Hole";
            newHole.layer = 2;
            newHole.GetComponent<Collider>().isTrigger = true;

            innerHolesZ.Add(newHole);
        }

        while (innerWallsAmountX < innerWallsX.Count)
        {
            var last = innerWallsX[^1];
            var lastHole = innerHolesX[^1];
            innerHolesX.Remove(lastHole);
            innerWallsX.Remove(last);
            if (Application.isEditor && Application.isPlaying == false)
            {
                DestroyImmediate(last);
                DestroyImmediate(lastHole);
            }
            else
            {
                Destroy(last);
                Destroy(lastHole);
            }
        }
        while (innerWallsAmountZ < innerWallsZ.Count)
        {
            var last = innerWallsZ[^1];
            var lastHole = innerHolesZ[^1];
            innerHolesZ.Remove(lastHole);
            innerWallsZ.Remove(last);
            if (Application.isEditor && Application.isPlaying == false)
            {
                DestroyImmediate(last);
                DestroyImmediate(lastHole);
            }
            else
            {
                Destroy(last);
                Destroy(lastHole);
            }
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

    public Vector3 GetRandomWorldPosOnFloor()
    {
        Vector3 localPos = new Vector3(Random.Range(-floorSizeX/2, floorSizeX/2), 1, Random.Range(-floorSizeZ/2, floorSizeZ/2));
        Vector3 worldPos = transform.TransformPoint(localPos);
        return worldPos;
    }
}