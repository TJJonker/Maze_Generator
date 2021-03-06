using System;
using UnityEngine;

namespace Jonko.Grids {
    public class Grid<TGridObject>
    {
        public event EventHandler<EAOnGridValueChanged> OnGridValueChanged;
        public class EAOnGridValueChanged : EventArgs
        {
            public int x, y;
        }

        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;
        private TGridObject[,] gridArray;
        private TextMesh[,] debugTextArray;

        /// <summary>
        ///     Creates a grid
        /// </summary>
        /// <param name="width"> Width of the grid </param>
        /// <param name="height"> Height of the grid </param>
        /// <param name="cellSize"> Size of the cells in the grid </param>
        /// <param name="originPosition"> Origin position of the grid </param>
        public Grid(int width, int height, float cellSize, Vector2 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;


            gridArray = new TGridObject[width, height];

            for(int x = 0; x < gridArray.GetLength(0); x++)
            {
                for(int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }

            var showDebug = true;
            debugTextArray = new TextMesh[width, height];
            if (showDebug)
            {
                for (int x = 0; x < gridArray.GetLength(0); x++)
                {
                    for (int y = 0; y < gridArray.GetLength(1); y++)
                    {
                        debugTextArray[x, y] = Visualisation.Visualisation.CreateWorldText(gridArray[x, y]?.ToString(), Color.white, 
                            null, GetWorldPosition(x, y) + Vector3.one * cellSize * .5f, 3, TextAnchor.MiddleCenter, TextAlignment.Center);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100);
                    }
                }
                Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100);
                Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100);
            }
        }

        /// <summary>
        ///     Converts grid position to world position
        /// </summary>
        /// <param name="position"> Vector2 grid position </param>
        /// <returns></returns>
        public Vector3 GetWorldPosition(Vector3 position)
            => position * cellSize + originPosition;

        /// <summary>
        ///     Converts grid position to world position
        /// </summary>
        /// <param name="x"> X position on the grid </param>
        /// <param name="y"> Y position on the grid </param>
        /// <returns></returns>
        public Vector3 GetWorldPosition(int x, int y) 
            => GetWorldPosition(new Vector3(x, y));

        /// <summary>
        ///     Converts world position to grid position
        /// </summary>
        /// <param name="position"> World position </param>
        /// <param name="x"> X position on the grid </param>
        /// <param name="y"> Y position on the grid </param>
        public void GetGridPosition(Vector2 position, out int x, out int y)
        {
            x = Mathf.FloorToInt((position.x - originPosition.x) / cellSize);
            y = Mathf.FloorToInt((position.y - originPosition.y) / cellSize);
        }

        /// <summary>
        ///     Converts world position to grid position
        /// </summary>
        /// <param name="posX"> X position in the world </param>
        /// <param name="posY"> Y position in the world </param>
        /// <param name="x"> X position on the grid </param>
        /// <param name="y"> Y position on the grid </param>
        public void GetGridPosition(int posX, int posY, out int x, out int y)
            => GetGridPosition(new Vector2(posX, posY), out x, out y);

        /// <summary>
        ///     Sets the value a specific place in the grid
        /// </summary>
        /// <param name="position"> Position on the grid </param>
        /// <param name="value"> Value to put in the grid spot </param>
        public void SetGridObject(Vector3 position, TGridObject value)
        {
            int x, y;
            GetGridPosition(position, out x, out y);
            SetGridObject(x, y, value);
        }

        /// <summary>
        ///     Sets the value a specific place in the grid
        /// </summary>
        /// <param name="x"> X position on the grid </param>
        /// <param name="y"> Y position on the grid </param>
        /// <param name="value"> Value to put in the grid spot </param>
        public void SetGridObject(int x, int y, TGridObject value)
        {
            if(x >= 0 && y >= 0 && x < width && y < height)
            {
                gridArray[x, y] = value;
                TriggerGridObjectChanged(x, y);
            }
        }

        /// <summary>
        ///     Triggers the grid to change its value on a specific grid cell
        /// </summary>
        /// <param name="x"> X position of the grid cell </param>
        /// <param name="y"> Y position of the grid cell </param>
        public void TriggerGridObjectChanged(int x, int y)
        {
            OnGridValueChanged?.Invoke(this, new EAOnGridValueChanged { x = x, y = y });
            debugTextArray[x, y].text = gridArray[x, y].ToString();
        }

        /// <summary>
        ///     Returns the value of a specific place in the grid
        /// </summary>
        /// <param name="position"> The position on the grid </param>
        /// <returns> The value of the given place in the grid </returns>
        public TGridObject GetGridObject(Vector3 position)
        {
            GetGridPosition(position, out int x, out int y);
            return GetGridObject(x, y);
        }

        /// <summary>
        ///     Returns the value of a specific place in the grid
        /// </summary>
        /// <param name="x"> X position on the grid </param>
        /// <param name="y"> Y position on the grid </param>
        /// <returns> The value of the given place in the grid </returns>
        public TGridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
                return gridArray[x, y];
            else return default(TGridObject);
        }

        /// <summary>
        ///     Returns the width of the grid
        /// </summary>
        /// <returns> Returns the width of the grid </returns>
        public int GetWidth() => width;

        /// <summary>
        ///     Returns the height of the grid
        /// </summary>
        /// <returns> Returns the height of the grid </returns>
        public int GetHeight() => height;

        /// <summary>
        ///     Returns The size of the cells
        /// </summary>
        /// <returns> Returns The size of the cells </returns>
        public float GetCellSize() => cellSize;
    }
}
