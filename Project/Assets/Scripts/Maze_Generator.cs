using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

public class Maze_Generator : MonoBehaviour
{
    public MazeCell[] GenerateMaze(Vector2Int mazeSize, float cellSize, int randomSeed)
    {
        // Create new Random object with given seed
        Random random = new Random((uint)randomSeed);
        // Create array with given size
        NativeArray<MazeCell> cells = new NativeArray<MazeCell>(mazeSize.x * mazeSize.y, Allocator.TempJob);
        // Prepare the maze generation
        GenerateMazeJob generateMaze = new GenerateMazeJob()
        {
            mazeSize = new int2(mazeSize.x, mazeSize.y),
            cells = cells,
            random = random
        };
        // Run the maze generation
        generateMaze.Run();
        MazeCell[] mazeCells = generateMaze.cells.ToArray();

        // Dispose Native Arrays
        cells.Dispose();

        return mazeCells;
    }



    [BurstCompile]
    public struct GenerateMazeJob : IJob
    {
        public NativeArray<MazeCell> cells;
        public int2 mazeSize;
        public Random random;

        // Execute the maze generation
        public void Execute()
        {
            // Create an iteration stack
            NativeList<int> stack = new NativeList<int>(1, Allocator.Temp);
            int currentCellIndex;

            // Instantiate the Cells
            for(int x = 0; x < mazeSize.x; x++)
            {
                for(int y = 0; y < mazeSize.y; y++)
                {
                    MazeCell cell = new MazeCell(x, y, CalculateIndex(x, y));
                    cells[cell.index] = cell;
                }
            }

            // Add the starting cell to the iteration stack
            stack.Add(0);
            // Check whether or not there are cells to be iterated
            while(stack.Length > 0)
            {
                // Get the last item from the stack
                currentCellIndex = stack[stack.Length - 1];
                stack.RemoveAt(stack.Length - 1);
                stack.Capacity--;

                // Mark the cell as visited
                SetIsVisited(true, currentCellIndex);
                // Check if the cell has usable neighbours
                var nextCellIndex = CheckNeighbours(currentCellIndex);
                // If there are usable neighbours
                while(nextCellIndex != -1)
                {
                    // Mark the neighbour as visitied
                    SetIsVisited(true, nextCellIndex);
                    // Add the current cell to the stack for later iteration
                    stack.Add(currentCellIndex);

                    // Remove the walls between the current and next cells
                    RemoveWalls(currentCellIndex, nextCellIndex);
                    currentCellIndex = nextCellIndex;
                    // Check for new neighbours to iterate through
                    nextCellIndex = CheckNeighbours(currentCellIndex);
                }                
            }
            // Dispose native container
            stack.Dispose();
        }

        /// <summary>
        ///     Checks for usable neighbours and returns one random neighbour
        /// </summary>
        /// <param name="currentCellIndex"> Index of the current cell </param>
        /// <returns> Returns -1 if false, a random neighbour if true </returns>
        private int CheckNeighbours(int currentCellIndex)
        {
            // Cache current cell
            MazeCell currentCell = cells[currentCellIndex];

            // Checks if the neighbours are usable 
            currentCell.neighbourTop.z   = IsNeighbourUsable(CalculateIndex(currentCell.neighbourTop));
            currentCell.neighbourLeft.z = IsNeighbourUsable(CalculateIndex(currentCell.neighbourLeft));
            currentCell.neighbourBottom.z = IsNeighbourUsable(CalculateIndex(currentCell.neighbourBottom));
            currentCell.neighbourRight.z = IsNeighbourUsable(CalculateIndex(currentCell.neighbourRight));

            // Counts the amount of usable neighbours and add them to a list
            NativeList<int> usableNeightbourIndexes = new NativeList<int>(Allocator.Temp);
            if (currentCell.neighbourTop.z != -1) usableNeightbourIndexes.Add(currentCell.neighbourTop.z);
            if (currentCell.neighbourLeft.z != -1) usableNeightbourIndexes.Add(currentCell.neighbourLeft.z);
            if (currentCell.neighbourBottom.z != -1) usableNeightbourIndexes.Add(currentCell.neighbourBottom.z);
            if (currentCell.neighbourRight.z != -1) usableNeightbourIndexes.Add(currentCell.neighbourRight.z);

            // If there are usable neighbours, pick a random one from the list
            if (usableNeightbourIndexes.Length > 0)
                return usableNeightbourIndexes[random.NextInt(usableNeightbourIndexes.Length)];
            return -1;            
        }

        /// <summary>
        ///     Checks whether the given cell exists and is not visited
        /// </summary>
        /// <param name="index"> The index of the cell to check </param>
        /// <returns> returns -1 for false, its index for true</returns>
        private int IsNeighbourUsable(int index)
            => index > -1 && !cells[index].isVisited ? index : -1;

        /// <summary>
        ///     Calculates the index based on the x and y position
        /// </summary>
        /// <param name="x"> x position </param>
        /// <param name="y"> y position</param>
        /// <returns> returns the index </returns>
        private int CalculateIndex(int x, int y)
        {
            if (x < 0 || y < 0 || x > mazeSize.x - 1 || y > mazeSize.y - 1) 
                return -1;
            return x + y * mazeSize.x;
        }

        /// <summary>
        ///     Calculates the index based on the x and y position
        /// </summary>
        /// <param name="pos"> position </param>
        /// <returns> returns the index </returns>
        private int CalculateIndex(int2 pos)
            => CalculateIndex(pos.x, pos.y);

        /// <summary>
        ///     Calculates the index based on the x and y position
        /// </summary>
        /// <param name="pos"> position </param>
        /// <returns> returns the index </returns>
        private int CalculateIndex(int3 pos)
            => CalculateIndex(pos.x, pos.y);

        /// <summary>
        ///     Sets the Visited variable of the given cell
        /// </summary>
        /// <param name="IsVisited"> Whether or not the cell is visited </param>
        /// <param name="cellIndex"> The index of the cell </param>
        private void SetIsVisited(bool IsVisited, int cellIndex)
        {
            MazeCell cell = cells[cellIndex];
            cell.isVisited = IsVisited;
            cells[cellIndex] = cell;
        }

        /// <summary>
        ///     Removes the walls between the two given cells
        /// </summary>
        /// <param name="currentCellIndex"> Index of the first cell </param>
        /// <param name="nextCellIndex"> Index of the second cell </param>
        private void RemoveWalls(int currentCellIndex, int nextCellIndex)
        {
            // Cache cells
            var current = cells[currentCellIndex];
            var next = cells[nextCellIndex];
            // Remove their walls
            current.RemoveWalls(new int2(next.x, next.y));
            next.RemoveWalls(new int2(current.x, current.y));
            // confirm the changes
            cells[currentCellIndex] = current;
            cells[nextCellIndex] = next;
        }
    }
}

public struct MazeCell
{
    public int x, y;
    public int index;

    public bool wallTop;
    public bool wallLeft;
    public bool wallBottom;
    public bool wallRight;

    public int3 neighbourTop;
    public int3 neighbourLeft;
    public int3 neighbourBottom;
    public int3 neighbourRight;

    public bool isVisited;

    public MazeCell(int x, int y, int index)
    {
        this.x = x;
        this.y = y;
        this.index = index;

        wallTop     = true;
        wallLeft    = true;
        wallBottom  = true;
        wallRight   = true;

        neighbourTop    = new int3(x, y + 1, -1);
        neighbourLeft   = new int3(x - 1, y, -1);
        neighbourBottom = new int3(x, y - 1, -1);
        neighbourRight  = new int3(x + 1, y, -1);

        isVisited = false;
    }

    /// <summary>
    ///     Removes the wall between this and the given cell
    /// </summary>
    /// <param name="otherCellPosition"> Position of the other cell </param>
    public void RemoveWalls(int2 otherCellPosition)
    {
        // Check position difference (x)
        var x = otherCellPosition.x - this.x;
        // Remove walls accordingly
        if (x == 1) wallRight = false;
        else if(x == -1) wallLeft = false;

        // check position difference (y)
        var y = otherCellPosition.y - this.y;
        // Remove walls accordingly
        if(y == 1) wallTop = false;
        else if(y == -1) wallBottom = false;
    }

    public void CleanUpWalls()
    {
        if (wallTop && neighbourTop.z != -1) wallTop = false;
        if (wallLeft && neighbourLeft.z != -1) wallLeft = false;
    }
}
