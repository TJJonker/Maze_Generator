using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    [SerializeField] private InformationCollector informationCollector;
    private Vector2Int mazeSize;
    
    private const float CELL_SIZE = .5f;

    private Maze_Generator mazeGenerator;
    private Maze_Visualiser visualiser;
    private Maze_Gameplay gameplay;
    
    public MazeCell[] Maze { get; private set; }


    private void Start()
    {
        mazeGenerator = GetComponent<Maze_Generator>();
        visualiser = GetComponent<Maze_Visualiser>();
        gameplay = GetComponent<Maze_Gameplay>();   
    }

    public void CreateMaze()
    {
        informationCollector.GetInformation(out int width, out int height, out int randomSeed);
        mazeSize = new Vector2Int(width, height);

        // Generate the maze
        Maze = mazeGenerator.GenerateMaze(mazeSize, CELL_SIZE, randomSeed);

        // Visualise the maze
        visualiser.DrawMaze(Maze, CELL_SIZE, mazeSize);

        // Give information to the gameplay script
        gameplay.GameplaySetup(Maze, CELL_SIZE, mazeSize);
    }
}
