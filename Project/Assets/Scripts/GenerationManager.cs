using UnityEngine;
using Jonko.Utils;
using UnityEngine.Pool;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour
{
    [SerializeField] private InformationCollector informationCollector;
    [SerializeField] private MeshFilter meshFilter;
    
    private const float CELL_SIZE = .5f;
    private const float WALL_WIDTH = 0.05f;
    private const int MAX_QUAD_AMOUNT_PER_MESH = 4000;

    private Maze_Generator mazeGenerator;
    private Vector2Int mazeSize;

    private Mesh mesh;

    private ObjectPool<GameObject> pool;
    private List<GameObject> UsedMeshGameObjects = new List<GameObject>();

    
    private void Awake()
    {
        mesh = new Mesh();
        // Create the pool
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

        //var startTimer = Time.realtimeSinceStartup;
        MazeCell[] mazeCells = mazeGenerator.GenerateMaze(mazeSize, CELL_SIZE, randomSeed);
        //Debug.Log("Time to Generate maze: " + (Time.realtimeSinceStartup - startTimer));

        //var startTime = Time.realtimeSinceStartup;
        DrawMaze(mazeCells);
        //Debug.Log("Time to draw maze: " + (Time.realtimeSinceStartup - startTime));
    }

    public void DrawMaze(MazeCell[] mazeCells)
    {
        // Empty the used object list and release all pool items
        foreach (var obj in UsedMeshGameObjects) pool.Release(obj);
        UsedMeshGameObjects.Clear();

        // Clean up the maze (In case of double walls)
        MazeCell[] WallCleanedMazeCells = WallCleanUp(mazeCells);

        // Get the first object from the pool
        pool.Get(out GameObject poolObject);
        UsedMeshGameObjects.Add(poolObject);

        int quadCount = 0;
        int nextMeshIndex = 0;
        
        MeshUtils.CreateEmptyMeshArrays(100000, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles);

        //var startTime = Time.realtimeSinceStartup;
        //foreach(var cell in WallCleanedMazeCells)
        for (int i = 0; i < WallCleanedMazeCells.Length; i++)
        {

            if(quadCount > MAX_QUAD_AMOUNT_PER_MESH - 3)
            {
                // Apply current arrays to the mesh
                MeshUtils.ApplyToMesh(mesh, vertices, uvs, triangles);
                // Apply mesh to meshObject
                poolObject.GetComponent<MeshFilter>().mesh = mesh;
                // Create new mesh
                mesh = new Mesh();
                // Get new object from the pool
                pool.Get(out poolObject);
                UsedMeshGameObjects.Add(poolObject);
                

                quadCount = 0;
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
    /// <summary>
    ///     Creates a wall between to points in the given mesh arrays 
    /// </summary>
    /// <param name="pointA"> First wall point </param>
    /// <param name="pointB"> Second wall point</param>
    /// <param name="index"> Place in the array </param>
    /// <param name="vertices"> Array of vertices to add the information to </param>
    /// <param name="uvs"> Array of uvs to add the information to </param>
    /// <param name="triangles"> Array of triangles to add the information to </param>
    private void CreateWall(Vector2 pointA, Vector2 pointB, int index, ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles)
    {
        // Determine the direction vector between the points
        var shapeDirection = pointB - pointA;
        // Adding a little width to the wall
        pointA += new Vector2(WALL_WIDTH / 2f, WALL_WIDTH / 2f);
        pointB -= new Vector2(WALL_WIDTH / 2f, WALL_WIDTH / 2f);
        // Adding the information to the arrays
        MeshUtils.AddToMeshArray(ref vertices, ref uvs, ref triangles, index, pointA + shapeDirection * .5f, 0f, pointB - pointA, Vector2.zero, Vector2.one);
    }

    /// <summary>
    ///     Calls the CleanUpWalls() function in all the MazeCell cells
    /// </summary>
    /// <param name="mazeCells"> Array of cells </param>
    /// <returns> Returns a cleaned up version of the array </returns>
    private MazeCell[] WallCleanUp(MazeCell[] mazeCells)
    {
        for (int i = 0; i < mazeCells.Length; i++)
            mazeCells[i].CleanUpWalls();
        return mazeCells;
    }

    /// <summary>
    ///     Returns the world position of a given position on the grid
    /// </summary>
    /// <param name="gridPositionX"> X position on the grid </param>
    /// <param name="gridPositionY"> Y position on the grid </param>
    /// <returns> returns the world position </returns>
    public Vector2 GetWorldPosition(int gridPositionX, int gridPositionY)
        => new Vector2(-gridPositionX, -gridPositionY) * CELL_SIZE + CalculateOrigin();

    /// <summary>
    ///     Calculates the origin point of the grid
    /// </summary>
    /// <returns> Returns the origin of the grid </returns>
    public Vector2 CalculateOrigin()
        => new Vector2(mazeSize.x, mazeSize.y) / 2f * CELL_SIZE;
    #endregion
}
