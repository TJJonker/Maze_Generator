using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class Maze_Generator : MonoBehaviour
{
    [SerializeField] private int2 mazeSize;
    [SerializeField] private float cellSize;

    private void Start()
    {
        GenerateMaze generateMaze = new GenerateMaze()
        {
            mazeCellSize = cellSize,
            mazeSize = mazeSize,
        };
        generateMaze.Run();
    }

    public struct GenerateMaze : IJob
    {
        public int2 startPosiiton;
        public int2 endPosition;

        public float mazeCellSize;
        public int2 mazeSize;


        public void Execute()
        {
            NativeArray<MazeCell> mazeCellArray = new NativeArray<MazeCell>(mazeSize.x * mazeSize.y, Allocator.Temp);

            for(int x = 0; x < mazeSize.x; x++)
            {
                for(int y = 0; y < mazeSize.y; y++)
                {
                    MazeCell cell = new MazeCell();

                    cell.x = x;
                    cell.y = y;
                    cell.index = CalculateIndex(x, y);
                    cell.cellSize = mazeCellSize;

                    cell.wallTop = true;
                    cell.wallLeft = true;
                    cell.wallBottom = true;
                    cell.wallRight = true;

                    mazeCellArray[cell.index] = cell;
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100);
                }
                Debug.DrawLine(GetWorldPosition(0, mazeSize.y), GetWorldPosition(mazeSize.x, mazeSize.y), Color.white, 100);
                Debug.DrawLine(GetWorldPosition(mazeSize.x, 0), GetWorldPosition(mazeSize.x, mazeSize.y), Color.white, 100);
            }

            // Disposing the NativeArrays
            mazeCellArray.Dispose();
        }

        public int CalculateIndex(int x, int y)
            => x + y * mazeSize.x;

        public Vector2 GetWorldPosition(int gridPositionX, int gridPositionY)
            => new Vector2(gridPositionX, gridPositionY) * mazeCellSize + CalculateOrigin();

        public Vector2 CalculateOrigin()
            => new Vector2(mazeSize.x, mazeSize.y) / 2f * mazeCellSize;
    }


    public struct MazeCell
    {
        public int x, y;
        public int index;
        public float cellSize;

        public bool wallTop;
        public bool wallLeft;
        public bool wallBottom;
        public bool wallRight;
    }
}
