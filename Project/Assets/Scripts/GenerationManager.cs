using UnityEngine;
using Jonko.Utils;

public class GenerationManager : MonoBehaviour
{
    [SerializeField] private InformationCollector informationCollector;
    [SerializeField] private MeshFilter meshFilter;
    
    private const float CELL_SIZE = .5f;

    private Maze_Generator mazeGenerator;
    private Vector2Int mazeSize;

    private Mesh mesh;

    
    private void Awake()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }
    

    private void Start() 
        => mazeGenerator = GetComponent<Maze_Generator>();

    public void CreateMaze()
    {
        int width, height, randomSeed;
        informationCollector.GetInformation(out width, out height, out randomSeed);
        mazeSize = new Vector2Int(width, height);        

        MazeCell[] mazeCells = mazeGenerator.GenerateMaze(mazeSize, CELL_SIZE, randomSeed);
        var startTime = Time.realtimeSinceStartup;
        DrawMaze(mazeCells);
        Debug.Log("Time to draw maze: " + (Time.realtimeSinceStartup - startTime));
    }

    public void DrawMaze(MazeCell[] mazeCells)
    {
        MazeCell[] WallCleanedMazeCells = WallCleanUp(mazeCells);
        MeshUtils.CreateEmptyMeshArrays(100000, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles);

        //foreach(var cell in WallCleanedMazeCells)
        for (int i = 0; i < WallCleanedMazeCells.Length; i++)
        {
            var cell = WallCleanedMazeCells[i];
            int index = i * 4;

            if (cell.wallTop)   CreateWall(GetWorldPosition(cell.x, cell.y + 1), GetWorldPosition(cell.x + 1, cell.y + 1), index, ref vertices, ref uvs, ref triangles);
            if (cell.wallLeft)  CreateWall(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x, cell.y + 1), index + 1, ref vertices, ref uvs, ref triangles);
            if (cell.wallBottom)CreateWall(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x + 1, cell.y), index + 2, ref vertices, ref uvs, ref triangles);
            if (cell.wallRight) CreateWall(GetWorldPosition(cell.x + 1, cell.y), GetWorldPosition(cell.x + 1, cell.y + 1), index + 3, ref vertices, ref uvs, ref triangles);
            //break;
        }

        MeshUtils.ApplyToMesh(mesh, vertices, uvs, triangles);
        meshFilter.mesh = mesh;

        
        foreach (var cell in WallCleanedMazeCells)
        {
            if (cell.wallTop)   Debug.DrawLine(GetWorldPosition(cell.x, cell.y + 1), GetWorldPosition(cell.x + 1, cell.y + 1), Color.black, 100);
            if (cell.wallLeft)  Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x, cell.y + 1), Color.black, 100);
            if (cell.wallBottom)Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x + 1, cell.y), Color.black, 100);
            if (cell.wallRight) Debug.DrawLine(GetWorldPosition(cell.x + 1, cell.y), GetWorldPosition(cell.x + 1, cell.y + 1), Color.black, 100);
            //break;   
        }
        
    }



    #region Helper functions
    private void CreateWall(Vector2 pointA, Vector2 pointB, int index, ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles)
    {
        var shapeDirection = pointB - pointA;

        pointA += new Vector2(.025f, .025f);
        pointB -= new Vector2(.025f, .025f);

        MeshUtils.AddToMeshArray(ref vertices, ref uvs, ref triangles, index, pointA + shapeDirection * .5f, 0f, pointB - pointA, Vector2.zero, Vector2.one);
        //MeshUtils.AddToMesh(ref mesh, pointA + shapeDirection * .5f, 0f, pointB - pointA, Vector2.zero, Vector2.one);
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

    public Vector2 GetWorldPosition(int gridPositionX, int gridPositionY)
    {
        return new Vector2(-gridPositionX, -gridPositionY) * CELL_SIZE + CalculateOrigin();
    }

    public Vector2 CalculateOrigin()
        => new Vector2(mazeSize.x, mazeSize.y) / 2f * CELL_SIZE;
    #endregion
}
