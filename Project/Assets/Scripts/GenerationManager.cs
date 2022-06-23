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
        DrawMaze(mazeCells);
    }

    public void DrawMaze(MazeCell[] mazeCells)
    {
        mesh = new Mesh();
        MazeCell[] WallCleanedMazeCells = WallCleanUp(mazeCells);
        foreach(var cell in WallCleanedMazeCells)
        {
            if (cell.wallTop)   CreateWall(GetWorldPosition(cell.x, cell.y + 1), GetWorldPosition(cell.x + 1, cell.y + 1));
            if (cell.wallLeft)  CreateWall(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x, cell.y + 1));
            if (cell.wallBottom)CreateWall(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x + 1, cell.y));
            if (cell.wallRight) CreateWall(GetWorldPosition(cell.x + 1, cell.y), GetWorldPosition(cell.x + 1, cell.y + 1));
            //break;
        }

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        
        foreach (var cell in WallCleanedMazeCells)
        {
            if (cell.wallTop)   Debug.DrawLine(GetWorldPosition(cell.x, cell.y + 1), GetWorldPosition(cell.x + 1, cell.y + 1), Color.black, 100);
            if (cell.wallLeft)  Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x, cell.y + 1), Color.black, 100);
            if (cell.wallBottom)Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x + 1, cell.y), Color.black, 100);
            if (cell.wallRight) Debug.DrawLine(GetWorldPosition(cell.x + 1, cell.y), GetWorldPosition(cell.x + 1, cell.y + 1), Color.black, 100);
            break;   
        }
        
    }



    #region Helper functions
    private void CreateWall(Vector2 pointA, Vector2 pointB)
    {
        var shapeDirection = pointB - pointA;

        pointA += new Vector2(.025f, .025f);
        pointB -= new Vector2(.025f, .025f);
        /*
        if(shapeDirection.x == 0)
        {
            pointA.x += .025f;
            pointB.x -= .025f;
        }
        if(shapeDirection.y == 0)
        {
            pointA.y += .025f;
            pointB.y -= .025f;
        }
        */
        MeshUtils.AddToMesh(ref mesh, pointA + shapeDirection * .5f, 0f, pointB - pointA, Vector2.zero, Vector2.one);


        //var shapeDirection = pointB - pointA;
        //if (shapeDirection.x > 0) pointA.y -= .5f;
        //if (shapeDirection.y > 0) pointA.x -= .5f;
        //MeshUtils.AddToMesh(ref mesh, pointA, 0f, pointB - pointA, Vector2.zero, Vector2.zero);
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
