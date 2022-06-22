using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

public class Maze_Generator : MonoBehaviour
{
    [SerializeField] private Vector2Int mazeSize;
    [SerializeField] private float cellSize;

    private void Start()
    {
        MazeCell[] mazeCells = GenerateMaze(mazeSize, cellSize);
        DrawMaze(mazeCells);
    }

    public MazeCell[] GenerateMaze(Vector2Int mazeSize, float cellSize, int randomSeed = -1)
    {
        Random random = randomSeed == -1 ? new Random() : new Random((uint)randomSeed);

        NativeArray<MazeCell> cells = new NativeArray<MazeCell>(mazeSize.x * mazeSize.y, Allocator.TempJob);

        GenerateMazeJob generateMaze = new GenerateMazeJob()
        {
            cells = cells,
            mazeSize = new int2(mazeSize.x, mazeSize.y),
            random = random
        };

        generateMaze.Run();

        MazeCell[] mazeCells = generateMaze.cells.ToArray();

        // Dispose Native Arrays
        cells.Dispose();

        return mazeCells;
    }

    public void DrawMaze(MazeCell[] mazeCells)
    {
        foreach (var cell in mazeCells)
        {
            if (cell.wallTop) Debug.DrawLine(GetWorldPosition(cell.x, cell.y + 1), GetWorldPosition(cell.x + 1, cell.y + 1), Color.white, 100);
            if (cell.wallLeft) Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x, cell.y + 1), Color.white, 100);
            if (cell.wallBottom) Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x + 1, cell.y), Color.white, 100);
            if (cell.wallRight) Debug.DrawLine(GetWorldPosition(cell.x + 1, cell.y), GetWorldPosition(cell.x + 1, cell.y + 1), Color.white, 100);
        }
    }

    public struct GenerateMazeJob : IJob
    {
        public int2 mazeSize;
        public NativeArray<MazeCell> cells;
        public Random random;


        public void Execute()
        {
            for(int x = 0; x < mazeSize.x; x++)
            {
                for(int y = 0; y < mazeSize.y; y++)
                {
                    MazeCell cell = new MazeCell();

                    cell.x = x;
                    cell.y = y;
                    cell.index = CalculateIndex(x, y);

                    cell.wallTop    = true;
                    cell.wallLeft   = true;
                    cell.wallBottom = true;
                    cell.wallRight  = true;

                    cell.isVisited  = false;

                    cells[cell.index] = cell;
                }
            }

            int currentCellIndex = 0;

        }

        private int CheckNeighbours(int currentCellIndex)
        {
            MazeCell currentCell = cells[currentCellIndex];

            NativeArray<int> neighbourIndex = new NativeArray<int>(4, Allocator.Temp);
            neighbourIndex[0]   = IsNeighbourUsable(CalculateIndex(currentCell.x, currentCell.y + 1));
            neighbourIndex[1]   = IsNeighbourUsable(CalculateIndex(currentCell.x - 1, currentCell.y));
            neighbourIndex[2]   = IsNeighbourUsable(CalculateIndex(currentCell.x, currentCell.y - 1));
            neighbourIndex[3]   = IsNeighbourUsable(CalculateIndex(currentCell.x + 1, currentCell.y));

            NativeList<int> usableNeightbourIndexes = new NativeList<int>(Allocator.Temp);
            foreach (int cellIndex in neighbourIndex)
                if (cellIndex != -1) usableNeightbourIndexes.Add(cellIndex);

            // Dispose NativeArrays
            neighbourIndex.Dispose();

            if (usableNeightbourIndexes.Length > 0)
                return usableNeightbourIndexes[random.NextInt(usableNeightbourIndexes.Length)];
            return -1;            
        }

        private int IsNeighbourUsable(int index)
            => index > -1 && !cells[index].isVisited ? index : -1;

        public int CalculateIndex(int x, int y)
        {
            if (x < 0 || y < 0 || x > mazeSize.x || y > mazeSize.y) 
                return -1;
            return x + y * mazeSize.x;
        }
    }

    #region Helper functions
    public Vector2 GetWorldPosition(int gridPositionX, int gridPositionY)
    => new Vector2(-gridPositionX, -gridPositionY) * cellSize + CalculateOrigin();

    public Vector2 CalculateOrigin()
        => new Vector2(mazeSize.x, mazeSize.y) / 2f * cellSize;
    #endregion

    public struct MazeCell
    {
        public int x, y;
        public int index;

        public bool wallTop;
        public bool wallLeft;
        public bool wallBottom;
        public bool wallRight;

        public bool isVisited;
    }
}
