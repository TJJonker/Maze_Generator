using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Jonko.Timers;
using Unity.Mathematics;
using UnityEngine.Events;

public class Maze_Gameplay : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] private CinemachineVirtualCamera cameraTarget;
    [SerializeField] private float movementTime;

    [SerializeField] private UnityEvent action;

    private Transform previousCameraTarget;

    private GameObject playerObject;
    private GameObject flagObject;

    private MazeCell[] maze;
    private float cellSize;
    private Vector2Int mazeSize;

    private bool canMove = true;
    private int currentCellIndex;
    private int finishIndex;

    public void Start()
    {
        InputManager.Instance.PlayerInputActionMap.Gameplay.Movement.performed += TryMove;
        InputManager.Instance.PlayerInputActionMap.Gameplay.Quit.performed += FinishLevel;
    }

    public void GameplaySetup(MazeCell[] maze, float cellSize, Vector2Int mazeSize)
    {
        this.maze = maze;
        this.cellSize = cellSize;
        this.mazeSize = mazeSize; 
    }

    public void TryStartGamePlay()
    {
        if (maze == null) return;

        InputManager.Instance.SwitchActionMap(InputManager.Instance.PlayerInputActionMap.Gameplay);

        currentCellIndex = mazeSize.x - 1;
        finishIndex = maze.Length - mazeSize.x;

        playerObject = playerObject ? playerObject : Instantiate(playerPrefab);
        playerObject.transform.position = GetPosition(currentCellIndex);
        previousCameraTarget = cameraTarget.Follow;
        cameraTarget.Follow = playerObject.transform;

        flagObject = flagObject ? flagObject : Instantiate(flagPrefab);
        flagObject.transform.position = GetPosition(finishIndex);
    }

    private void TryMove(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        var desiredCellIndex = GetMovementDirection(context);
        if(desiredCellIndex == -1) return;
        canMove = false;

        var progressionSteps = 1 / movementTime;
        var startTime = Time.realtimeSinceStartup;
        var desiredPosition = GetPosition(desiredCellIndex);
        var originalPlayerPosition = playerObject.transform.position;
        var t = 0f;

        FunctionUpdater.Create(() =>
        {
            t += Time.deltaTime * progressionSteps;
            playerObject.transform.position = Vector3.Slerp(originalPlayerPosition, desiredPosition, t);

            if(Time.realtimeSinceStartup - startTime > movementTime)
            {
                playerObject.transform.position = desiredPosition;
                canMove = true;
                currentCellIndex = desiredCellIndex;
                if (currentCellIndex == finishIndex) FinishLevel();
                return true;
            }
            return false;
        });
    }

    private void FinishLevel(InputAction.CallbackContext context = default(InputAction.CallbackContext))
    {
        InputManager.Instance.SwitchActionMap(InputManager.Instance.PlayerInputActionMap.Generating);
        Destroy(playerObject);
        Destroy(flagObject);
        previousCameraTarget.position = Vector3.zero;
        cameraTarget.Follow = previousCameraTarget;
        action.Invoke();    
    }

    private int GetMovementDirection(InputAction.CallbackContext context)
    {
        MazeCell currentCell = maze[currentCellIndex];
        var direction = context.ReadValue<Vector2>();
        var x = direction.x;
        if (x > 0 && !currentCell.wallLeft) return CalculateIndex(currentCell.neighbourLeft);
        if (x < 0 && !currentCell.wallRight) return CalculateIndex(currentCell.neighbourRight);

        var y = direction.y;
        if(y > 0 && !currentCell.wallBottom) return CalculateIndex(currentCell.neighbourBottom);
        if(y < 0 && !currentCell.wallTop) return CalculateIndex(currentCell.neighbourTop);

        return -1;
    }

    private Vector2 GetPosition(int cellIndex) 
        => GetWorldPosition(maze[cellIndex].x, maze[cellIndex].y) - Vector2.one * (cellSize / 2f - cellSize / 40);

    /// <summary>
    ///     Returns the world position of a given position on the grid
    /// </summary>
    /// <param name="gridPositionX"> X position on the grid </param>
    /// <param name="gridPositionY"> Y position on the grid </param>
    /// <returns> returns the world position </returns>
    public Vector2 GetWorldPosition(int gridPositionX, int gridPositionY)
        => new Vector2(-gridPositionX, -gridPositionY) * cellSize + CalculateOrigin();

    /// <summary>
    ///     Calculates the origin point of the grid
    /// </summary>
    /// <returns> Returns the origin of the grid </returns>
    public Vector2 CalculateOrigin()
        => new Vector2(mazeSize.x, mazeSize.y) / 2f * cellSize;


    /// <summary>
    ///     Calculates the index based on the x and y position
    /// </summary>
    /// <param name="x"> x position </param>
    /// <param name="y"> y position</param>
    /// <returns> returns the index </returns>
    private int CalculateIndex(int3 position)
    {
        var x = position.x;
        var y = position.y;
        if (x < 0 || y < 0 || x > mazeSize.x - 1 || y > mazeSize.y - 1)
            return -1;
        return x + y * mazeSize.x;
    }

}
