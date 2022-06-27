using UnityEngine;
using Jonko.Utils;
using UnityEngine.Pool;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour
{
    [SerializeField] private InformationCollector informationCollector;
    [SerializeField] private MeshFilter meshFilter;
    
    private const float CELL_SIZE = .5f;
    private const int MAX_QUAD_AMOUNT_PER_MESH = 4100;

    private Maze_Generator mazeGenerator;
    private Vector2Int mazeSize;

    private Mesh mesh;

    private ObjectPool<GameObject> pool;
    private List<GameObject> UsedMeshGameObjects = new List<GameObject>();

    
    private void Awake()
    {
        mesh = new Mesh();

        pool = new ObjectPool<GameObject>(
            () => new GameObject("MeshObject", typeof(MeshFilter), typeof(MeshRenderer)),
            (shape) => shape.gameObject.SetActive(true),
            (shape) => {
                shape.gameObject.SetActive(false);
                shape.gameObject.GetComponent<MeshFilter>().mesh = null;
                },
            (shape) => Destroy(shape.gameObject),
            false, 5, 20
        );
    }
    

    private void Start() 
        => mazeGenerator = GetComponent<Maze_Generator>();

    public void CreateMaze()
    {
        int width, height, randomSeed;
        informationCollector.GetInformation(out width, out height, out randomSeed);
        mazeSize = new Vector2Int(width, height);

        var startTimer = Time.realtimeSinceStartup;
        MazeCell[] mazeCells = mazeGenerator.GenerateMaze(mazeSize, CELL_SIZE, randomSeed);
        Debug.Log("Time to Generate maze: " + (Time.realtimeSinceStartup - startTimer));

        var startTime = Time.realtimeSinceStartup;
        DrawMaze(mazeCells);
        Debug.Log("Time to draw maze: " + (Time.realtimeSinceStartup - startTime));
    }

    public void DrawMaze(MazeCell[] mazeCells)
    {
        foreach(var obj in UsedMeshGameObjects) pool.Release(obj);
        UsedMeshGameObjects.Clear();

        MazeCell[] WallCleanedMazeCells = WallCleanUp(mazeCells);

        //Debug.Log(WallCleanedMazeCells.Length);
        //Debug.Log(GetAmountOfWalls(WallCleanedMazeCells));

        pool.Get(out GameObject poolObject);
        UsedMeshGameObjects.Add(poolObject);

        int quadCount = 0;
        int nextMeshIndex = 0;
        MeshUtils.CreateEmptyMeshArrays(100000, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles);

        //foreach(var cell in WallCleanedMazeCells)
        for (int i = 0; i < WallCleanedMazeCells.Length; i++)
        {
            if(quadCount > MAX_QUAD_AMOUNT_PER_MESH - 3)
            {
                MeshUtils.ApplyToMesh(mesh, vertices, uvs, triangles);
                quadCount = 0;
                poolObject.GetComponent<MeshFilter>().mesh = mesh;
                mesh = new Mesh();
                pool.Get(out poolObject);
                UsedMeshGameObjects.Add(poolObject);
                nextMeshIndex = i * 4;
                MeshUtils.CreateEmptyMeshArrays(100000, out vertices, out uvs, out triangles);
            }

            int index = i * 4 - nextMeshIndex;
            var cell = WallCleanedMazeCells[i];

            if (cell.wallTop)
            {
                CreateWall(GetWorldPosition(cell.x, cell.y + 1), GetWorldPosition(cell.x + 1, cell.y + 1), index, ref vertices, ref uvs, ref triangles);
                quadCount++;
            }

            if (cell.wallLeft)
            {
                CreateWall(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x, cell.y + 1), index + 1, ref vertices, ref uvs, ref triangles);
                quadCount++;
            }

            if (cell.wallBottom)
            {
                CreateWall(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x + 1, cell.y), index + 2, ref vertices, ref uvs, ref triangles);
                quadCount++;
            }

            if (cell.wallRight)
            {
                CreateWall(GetWorldPosition(cell.x + 1, cell.y), GetWorldPosition(cell.x + 1, cell.y + 1), index + 3, ref vertices, ref uvs, ref triangles);
                quadCount++;
            }
            //break;
        }
        

        MeshUtils.ApplyToMesh(mesh, vertices, uvs, triangles);
        poolObject.GetComponent<MeshFilter>().mesh = mesh;

        #region Debug
        
        foreach (var cell in WallCleanedMazeCells)
        {
            if (cell.wallTop)   Debug.DrawLine(GetWorldPosition(cell.x, cell.y + 1),GetWorldPosition(cell.x + 1, cell.y + 1), Color.black, 100);
            if (cell.wallLeft)  Debug.DrawLine(GetWorldPosition(cell.x, cell.y),    GetWorldPosition(cell.x, cell.y + 1), Color.black, 100);
            if (cell.wallBottom)Debug.DrawLine(GetWorldPosition(cell.x, cell.y),    GetWorldPosition(cell.x + 1, cell.y), Color.black, 100);
            if (cell.wallRight) Debug.DrawLine(GetWorldPosition(cell.x + 1, cell.y),GetWorldPosition(cell.x + 1, cell.y + 1), Color.black, 100);
            //break;   
        }
        
        #endregion
    }



    #region Helper functions
    private void CreateWall(Vector2 pointA, Vector2 pointB, int index, ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles)
    {
        var shapeDirection = pointB - pointA;

        pointA += new Vector2(.025f, .025f);
        pointB -= new Vector2(.025f, .025f);

        MeshUtils.AddToMeshArray(ref vertices, ref uvs, ref triangles, index, pointA + shapeDirection * .5f, 0f, pointB - pointA, Vector2.zero, Vector2.one);
    }

    private MazeCell[] WallCleanUp(MazeCell[] mazeCells)
    {
        for (int i = 0; i < mazeCells.Length; i++)
        {
            MazeCell cell = mazeCells[i];
            if (cell.wallLeft && cell.neighbourLeft != -1) cell.wallLeft = false;
            if (cell.wallTop && cell.neighbourTop != -1) cell.wallTop = false;
            mazeCells[i] = cell;
        }
        return mazeCells;
    }

    private int GetAmountOfWalls(MazeCell[] mazeCells)
    {
        int amountOfWalls = 0;  
        foreach(MazeCell cell in mazeCells)
        {
            if(cell.wallTop) amountOfWalls++;
            if(cell.wallLeft) amountOfWalls++;
            if(cell.wallBottom) amountOfWalls++;
            if(cell.wallRight) amountOfWalls++;
        }
        return amountOfWalls;
    }

    public Vector2 GetWorldPosition(int gridPositionX, int gridPositionY)
    {
        return new Vector2(-gridPositionX, -gridPositionY) * CELL_SIZE + CalculateOrigin();
    }

    public Vector2 CalculateOrigin()
        => new Vector2(mazeSize.x, mazeSize.y) / 2f * CELL_SIZE;
    #endregion
}
