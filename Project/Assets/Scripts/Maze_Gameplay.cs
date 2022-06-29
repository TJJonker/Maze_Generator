using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class Maze_Gameplay : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CinemachineVirtualCamera cameraTarget;
    private GameObject playerObject;

    private MazeCell[] maze;
    private float cellSize;
    private Vector2Int mazeSize;

    private bool canMove = true;


    public void Start() 
        => InputManager.Instance.PlayerInputActionMap.Gameplay.Movement.performed += TryMove;

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

        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = GetPosition(0);
        cameraTarget.Follow = playerObject.transform;
    }

    private void TryMove(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        canMove = false;
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

}
