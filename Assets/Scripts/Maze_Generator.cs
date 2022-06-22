using UnityEngine;
using Unity.Burst;
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
        MazeCell[] mazeCells = GenerateMaze(mazeSize, cellSize, 42);
        DrawMaze(mazeCells);
    }

    public MazeCell[] GenerateMaze(Vector2Int mazeSize, float cellSize, int randomSeed)
    {
        Random random = new Random((uint)randomSeed);

        NativeArray<MazeCell> cells = new NativeArray<MazeCell>(mazeSize.x * mazeSize.y, Allocator.TempJob);

        GenerateMazeJob generateMaze = new GenerateMazeJob()
        {
            cells = cells,
            mazeSize = new int2(mazeSize.x, mazeSize.y),
            random = random
        };

        var starttime = Time.realtimeSinceStartup;
        generateMaze.Run();
        Debug.Log(Time.realtimeSinceStartup - starttime);
        MazeCell[] mazeCells = generateMaze.cells.ToArray();

        // Dispose Native Arrays
        cells.Dispose();

        return mazeCells;
    }

    public void DrawMaze(MazeCell[] mazeCells)
    {
        foreach (var cell in mazeCells)
        {
            //Debug.Log(cell.wallTop + " " + cell.wallLeft + " " + cell.wallBottom + " " + cell.wallRight);

            if (cell.wallTop) Debug.DrawLine(GetWorldPosition(cell.x, cell.y + 1), GetWorldPosition(cell.x + 1, cell.y + 1), Color.white, 100);
            if (cell.wallLeft) Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x, cell.y + 1), Color.white, 100);
            if (cell.wallBottom) Debug.DrawLine(GetWorldPosition(cell.x, cell.y), GetWorldPosition(cell.x + 1, cell.y), Color.white, 100);
            if (cell.wallRight) Debug.DrawLine(GetWorldPosition(cell.x + 1, cell.y), GetWorldPosition(cell.x + 1, cell.y + 1), Color.white, 100);
        }
    }

    [BurstCompile]
    public struct GenerateMazeJob : IJob
    {
        public int2 mazeSize;
        public NativeArray<MazeCell> cells;
        public Random random;


        public void Execute()
        {
            NativeList<int> stack = new NativeList<int>(1, Allocator.Temp);
            int currentCellIndex;

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

            stack.Add(0);
            while(stack.Length > 0)
            {
                currentCellIndex = stack[stack.Length - 1];
                stack.RemoveAt(stack.Length - 1);
                stack.Capacity--;

                SetIsVisited(true, currentCellIndex);

                var nextCellIndex = CheckNeighbours(currentCellIndex);
                
                while(nextCellIndex != -1)
                {
                    SetIsVisited(true, nextCellIndex);
                    stack.Add(currentCellIndex);

                    // Debug.Log("Current: " + currentCellIndex + ", Next: " + nextCellIndex);
                    RemoveWalls(currentCellIndex, nextCellIndex);
                    currentCellIndex = nextCellIndex;

                    nextCellIndex = CheckNeighbours(currentCellIndex);
                }                
            }

            stack.Dispose();
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

        private int CalculateIndex(int x, int y)
        {
            if (x < 0 || y < 0 || x > mazeSize.x - 1 || y > mazeSize.y - 1) 
                return -1;
            return x + y * mazeSize.x;
        }

        private void SetIsVisited(bool IsVisited, int cellIndex)
        {
            MazeCell cell = cells[cellIndex];
            cell.isVisited = IsVisited;
            cells[cellIndex] = cell;
        }

        private void RemoveWalls(int currentCellIndex, int nextCellIndex)
        {
            var next = cells[nextCellIndex];
            var current = cells[currentCellIndex];

            var x = next.x - current.x;
            if(x == 1)
            {
                current.wallRight = false;
                next.wallLeft = false;
            }
            else if(x == -1)
            {
                current.wallLeft = false;
                next.wallRight = false;
            }

            var y = next.y - current.y;
            if(y == 1)
            {
                next.wallBottom = false;
                current.wallTop = false;
            }
            else if(y == -1)
            {
                next.wallTop = false;
                current.wallBottom = false;
            }

            cells[nextCellIndex] = next;
            cells[currentCellIndex] = current;
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
