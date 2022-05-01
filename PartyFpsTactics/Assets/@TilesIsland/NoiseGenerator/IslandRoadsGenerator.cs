using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using Random = UnityEngine.Random;

public class IslandRoadsGenerator : MonoBehaviour
{
    private IslandGenerator ig;
    public Node[,] grid;
    public bool zeroPenaltyOnExistingRoad = false;

    [Header("Blur Weights")]
    public bool blurPenalties = false;
    public int blurPenaltiesSize = 3;
    public int obstaclesProximityPenalty = 10;
    
    [Header("Testing")]
    public bool toClosest = false;
    public bool obstaclesColorTest = false;

    public static IslandRoadsGenerator instance;

    void Awake()
    {
        instance = this;
    }
    
    public void ConnectPointsOfInterest()
    {
        ig = IslandGenerator.instance;
        StartCoroutine(CreateGrid());
    }

    IEnumerator CreateGrid()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        grid = new Node[ig.mapWidth,ig.mapHeight];
        var noiseMap = ig.GetNoiseMap();
        
        Vector2Int nodePos;
        bool walkable = false;
        int movementPenalty = 0;
        int t = 0;

        for (int y = 0; y < ig.mapHeight; y++)
        {
            for (int x = 0; x < ig.mapWidth; x++)
            {
                nodePos = new Vector2Int(x,y);

                if (ig.obstaclesCoordinates.Contains(nodePos))
                    walkable = false;
                else
                    walkable = true;

                for (int i = 0; i < ig.regions.Length; i++)
                {
                    if (noiseMap[x, y] <= ig.regions[i].height)
                    {
                        movementPenalty = ig.regions[i].movementPenalty;
                        if (!walkable)
                            movementPenalty += obstaclesProximityPenalty;
                        break;
                    }
                }
            
                grid[x, y] = new Node(walkable, x, y, movementPenalty);

                if (t == 10000)
                {
                    t = 0;
                    yield return null;   
                }

                t++;
            }
        }   
        
        
        sw.Stop();
        yield return null;

        print("NODES GRID CREATED! Time took: " + sw.ElapsedMilliseconds + " ms");
        
        if (blurPenalties)
            BlurPenaltyMap(blurPenaltiesSize);
        StartCoroutine(ConnectPOIsCoroutine());
    }

    IEnumerator ConnectPOIsCoroutine()
    {
        Color[] colourRoads = new Color[ig.mapWidth * ig.mapHeight];
        
        // first road from playerSpawn to bossSpawn
        // obstacles are obstacles

        if (toClosest)
        {
            Vector2Int closestPoint = ig.playerSpawnPoint.coordinates[0];
            int distance = 1000;
            for (int i = 1; i < ig.poisCoordinates.Count; i++)
            {
                Node newNode0;
                Node newNode1;

                newNode0 = grid[ig.playerSpawnPoint.coordinates[0].x, ig.playerSpawnPoint.coordinates[0].y];
                newNode1 = grid[ig.poisCoordinates[i].x, ig.poisCoordinates[i].y];   
                
                int newDistance = GetDistance(new Vector2Int(newNode0.gridX, newNode0.gridY), new Vector2Int(newNode1.gridX, newNode1.gridY));
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestPoint = new Vector2Int(newNode1.gridX, newNode1.gridY);
                }
            }
            PathRequestManager.RequestPath(new PathRequest(ig.playerSpawnPoint.coordinates[0], closestPoint, null), colourRoads);
        }
        else
        {
            // FIND PATH TO BOSS yield return StartCoroutine(FindPath(ig.playerSpawnPoint.coordinates[0], ig.bossSpawnPoint.coordinates[0], colourRoads));
            
            // CONNECT TO RANDOM POINTS
            var poisToConnect = new List<PointOfInterest>(ig.pointsOfInterest);
            for (int i = poisToConnect.Count - 1; i >= 0; i--)
            {
                if (poisToConnect[i].regionIndex == 0 || poisToConnect[i].regionIndex == 1 || poisToConnect[i].regionIndex == ig.regions.Length - 1) // dont connect deeps, water and snows
                {
                    poisToConnect.RemoveAt(i);
                }
            }

            var poi0 = ig.playerSpawnPoint;
            var poi1 = poisToConnect[0];
            while (poisToConnect.Count > 2)
            {
                poisToConnect.Remove(poi0);
                poi1 = poisToConnect[Random.Range(0, poisToConnect.Count)];
                
                PathRequestManager.RequestPath(new PathRequest(poi0.coordinates[0], poi1.coordinates[0], null), colourRoads);
                
                poi0 = poisToConnect[Random.Range(0, poisToConnect.Count)];
            }
        }

        if (obstaclesColorTest)
        {
            // COLOR NONWALKABLE NODES FOR TESTING
            for (int y = 0; y < ig.mapHeight; y++)
            {
                for (int x = 0; x < ig.mapWidth; x++)
                {
                    if (!grid[x,y].walkable)
                        colourRoads[y * ig.mapWidth + x] = Color.black;
                }
                yield return null;
            }
        }
        
        ig.PointsConnected(colourRoads);
    }

    public void FindPath(PathRequest request, Action<PathResult> callback, Color[] colourRoads)
    {
        //print("Try to find path between " + startPoint + " and " + targetPoint);
        List<Vector2Int> waypoints = new List<Vector2Int>();
        bool pathFound = false;
        
        Node startNode;
        Node targetNode;

        startNode = grid[request.pathStart.x, request.pathStart.y];
        targetNode = grid[request.pathEnd.x, request.pathEnd.y];
        
        if (colourRoads == null || (startNode.walkable && targetNode.walkable))
        {
            Heap<Node> openSet = new Heap<Node>(ig.mapWidth * ig.mapHeight);
            HashSet<Node> closedSet = new HashSet<Node>();
        
            openSet.Add(startNode);
            Node currentNode;
            int t = 0;
            while (openSet.Count > 0)
            {
                currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathFound = true;
                    break;
                }

                var nodeNeighbours = GetNeighbours(currentNode);
                for (int i = 0; i < nodeNeighbours.Count; i++)
                {
                    if ((colourRoads != null && !nodeNeighbours[i].walkable) || closedSet.Contains(nodeNeighbours[i]))
                        continue;

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(new Vector2Int(currentNode.gridX, currentNode.gridY), new Vector2Int(nodeNeighbours[i].gridX, nodeNeighbours[i].gridY)) + nodeNeighbours[i].movementPenalty;
                    if (newMovementCostToNeighbour < nodeNeighbours[i].gCost || !openSet.Contains(nodeNeighbours[i]))
                    {
                        nodeNeighbours[i].gCost = newMovementCostToNeighbour;
                        nodeNeighbours[i].hCost = GetDistance(new Vector2Int(nodeNeighbours[i].gridX, nodeNeighbours[i].gridY), new Vector2Int(targetNode.gridX, targetNode.gridY));
                        nodeNeighbours[i].parent = currentNode;

                        if (!openSet.Contains(nodeNeighbours[i]))
                        {
                            openSet.Add(nodeNeighbours[i]);
                        }
                        else
                        {
                            openSet.UpdateItem(nodeNeighbours[i]);
                        }
                    }
                }
            }
        }
        if (pathFound)
        {
            waypoints = RetracePath(startNode, targetNode, colourRoads);
            pathFound = waypoints.Count > 0;
        }

        if (colourRoads == null)
            callback(new PathResult(waypoints, pathFound, request.callback));
    }

    List<Vector2Int> RetracePath(Node startNode, Node endNode, Color[] colourRoads)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;

            if (colourRoads != null)
            {
                colourRoads[currentNode.gridY * ig.mapWidth + currentNode.gridX] = new Color(0.81f, 0f, 0.84f);
                if (zeroPenaltyOnExistingRoad)
                    grid[currentNode.gridX, currentNode.gridY].movementPenalty = 0;
            }
            
            ig.pathsCoordinates.Add(new Vector2Int(currentNode.gridX, currentNode.gridY));
        }
        path.Reverse();

        List<Vector2Int> waypoints = new List<Vector2Int>();
        for (var index = 0; index < path.Count; index++)
        {
            var p = path[index];
            waypoints.Add(new Vector2Int(p.gridX, p.gridY));
        }

        return waypoints;
    }
    
    public int GetDistance(Vector2Int coordsA, Vector2Int coordsB)
    {
        int dstX = Mathf.Abs(coordsA.x - coordsB.x);
        int dstY = Mathf.Abs(coordsA.y - coordsB.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtens = (kernelSize - 1) / 2;
        
        int [,] penaltiesHorizontalPass = new int[ig.mapWidth, ig.mapHeight];
        int [,] penaltiesVerticalPass = new int[ig.mapWidth, ig.mapHeight];

        for (int y = 0; y < ig.mapHeight; y++)
        {
            for (int x = -kernelExtens; x <= kernelExtens; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtens);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < ig.mapWidth; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtens - 1,0,ig.mapWidth);
                int addIndex = Mathf.Clamp(x + kernelExtens, 0, ig.mapWidth -1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] -
                                                grid[removeIndex, y].movementPenalty +
                                                grid[addIndex, y].movementPenalty;
            }
        }
        for (int x = 0; x < ig.mapWidth; x++)
        {
            for (int y = -kernelExtens; y <= kernelExtens; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtens);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            for (int y = 1; y < ig.mapHeight; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtens - 1,0,ig.mapHeight);
                int addIndex = Mathf.Clamp(y + kernelExtens, 0, ig.mapHeight -1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y-1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;
            }
        }
    }
    
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < ig.mapWidth && checkY >= 0 && checkY < ig.mapHeight)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
}

[Serializable]
public class Node : IHeapItem<Node>
{
    public bool walkable;
    public int gCost;
    public int hCost;

    public int gridX;
    public int gridY;
    public int movementPenalty;
    public Node parent;
    private int heapIndex;    
    public Node(bool _walkable, int _gridX, int _gridY, int _movementPenalty)
    {
        walkable = _walkable;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _movementPenalty;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}