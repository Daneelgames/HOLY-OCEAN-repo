using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public List<Room> spawnedRooms = new List<Room>();

    public Transform levelFolder;
    [Header("SCALE IS SCALED BY 2 IN CODE")]
    public Vector2Int roomsScaleMinMaxX = new Vector2Int(3, 10);
    public Vector2Int roomsScaleMinMaxY = new Vector2Int(2, 6);
    public Vector2Int roomsScaleMinMaxZ = new Vector2Int(3, 10);
    public Vector2Int roomsPositionsMinMaxX = new Vector2Int(-100, 100);
    public Vector2Int roomsPositionsMinMaxY = new Vector2Int(0, 100);
    public Vector2Int roomsPositionsMinMaxZ = new Vector2Int(-100, 100);
    
    public int roomsAmount = 10;
    IEnumerator Start()
    {
        for (int i = 0; i < roomsAmount; i++)
        {
            yield return StartCoroutine(SpawnNewRoom(i));
            yield return null;
        }

        yield return StartCoroutine(ConnectRooms());
        
    }

    IEnumerator SpawnNewRoom(int roomIndex)
    {
        while (true)
        {
            float roomY = roomsPositionsMinMaxY.x;
            
            if (roomIndex > 0)
            {
                roomY += roomIndex * 5;
            }
            
            Vector3 roomPosition = new Vector3(Random.Range(roomsPositionsMinMaxX.x, roomsPositionsMinMaxX.y),
                roomY, Random.Range(roomsPositionsMinMaxZ.x, roomsPositionsMinMaxZ.y));
            
            Vector3Int roomSize = new Vector3Int(Random.Range(roomsScaleMinMaxX.x, roomsScaleMinMaxX.y) * 2,
                Random.Range(roomsScaleMinMaxY.x, roomsScaleMinMaxY.y) * 2,Random.Range(roomsScaleMinMaxZ.x, roomsScaleMinMaxZ.y) * 2);
            Quaternion roomRotation = Quaternion.Euler(0, Random.Range(0,360), 0);

            if (!Physics.CheckBox(roomPosition, new Vector3(roomSize.x, roomSize.y, roomSize.z), roomRotation, 1 << 6))
            {
                yield return StartCoroutine(SpawnTilesInRoom(roomPosition, roomSize, roomRotation));
                if (roomIndex == 0)
                {
                    PlayerMovement.Instance.rb.MovePosition(roomPosition);
                }
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator SpawnTilesInRoom(Vector3 pos, Vector3Int size, Quaternion rot)
    {
        Room newRoom = new Room();
        newRoom.position = pos;
        newRoom.size = size;
        newRoom.rotation = rot;

        GameObject newGameObject = new GameObject("Room");
        newGameObject.transform.parent = levelFolder;
        newRoom.spawnedTransform = newGameObject.transform;
        newGameObject.transform.position = pos;
        newGameObject.transform.rotation = rot;
        
        spawnedRooms.Add(newRoom);
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                var newTile = Instantiate(tilePrefab, newRoom.spawnedTransform);
                newTile.transform.localRotation = Quaternion.identity;
                newTile.transform.localPosition = new Vector3(x - size.x / 2, 0, z - size.z/2);
                newRoom.tiles.Add(newTile);
            }
            yield return null;   
        }
    }

    IEnumerator ConnectRooms()
    {
        for (int i = 0; i < spawnedRooms.Count - 1; i++)
        {
            Room roomFrom = spawnedRooms[i];
            Room roomTo = spawnedRooms[i + 1];

            Transform roomFromClosestTile = roomFrom.tiles[0].transform;
            Transform roomToClosestTile = roomTo.tiles[0].transform;
            
            float distance = 10000;
            float newDistance = 0;
            
            for (int j = 0; j < roomFrom.tiles.Count; j++)
            {
                for (int k = 0; k < roomTo.tiles.Count; k++)
                {
                    if (j != 0 && k != 0 && j != roomFrom.tiles.Count-1 && k != roomTo.tiles.Count-1)
                        continue;
                    
                    var tile1 = roomFrom.tiles[j];
                    var tile2 = roomTo.tiles[k];
                    newDistance = Vector3.Distance(tile1.transform.position, tile2.transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        roomFromClosestTile = tile1.transform;
                        roomToClosestTile = tile2.transform;
                    }
                }
            }

            float bridgeTilesAmount = distance;
            for (int j = 0; j <= bridgeTilesAmount; j++)
            {
                Quaternion rot = Quaternion.Lerp(roomFromClosestTile.rotation, roomToClosestTile.rotation, j/bridgeTilesAmount);
                var newTile = Instantiate(tilePrefab, 
                    roomFromClosestTile.position + (roomToClosestTile.position - roomFromClosestTile.position).normalized * j, rot);
                newTile.transform.parent = levelFolder;
                yield return null;
            }
            yield return null;
        }
    }
}

[Serializable]
public class Room
{
    public List<GameObject> tiles = new List<GameObject>();
    public Transform spawnedTransform;
    public Vector3 position;
    public  Vector3Int size;
    public  Quaternion rotation;
}