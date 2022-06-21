using MazeGenerator.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///     Manager responsible for changing, enabling and disabling actionMaps.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public PlayerInputActionMap PlayerInputActionMap { get; private set; }

    private List<InputActionMap> activeActionMaps;
    private List<InputActionMap> previousActiveActionMaps;

    private void Awake()
    {
        Instance = this;    
        PlayerInputActionMap = new PlayerInputActionMap();
        activeActionMaps = new List<InputActionMap>();
        previousActiveActionMaps = new List<InputActionMap>();  
    }

    private void Start()
    {
        SwitchActionMap(PlayerInputActionMap.Generating);
    }

    /// <summary>
    ///     Disables all the currently enabled action maps and enables the given action map
    /// </summary>
    /// <param name="actionMap"> Action map to enable </param>
    public void SwitchActionMap(InputActionMap actionMap)
    {
        previousActiveActionMaps = new List<InputActionMap>(activeActionMaps);
        DisableAllActionMaps();        
        EnableActionMap(actionMap);
    }

    /// <summary>
    ///     Enables the given action map
    /// </summary>
    /// <param name="actionMap"> Action map to enable </param>
    public void EnableActionMap(InputActionMap actionMap)
    {
        actionMap.Enable();
        activeActionMaps.Add(actionMap);
    }

    /// <summary>
    ///     Disables the given action map.
    /// </summary>
    /// <param name="actionMap"> Action map to disable </param>
    public void DisableActionMap(InputActionMap actionMap)
    {
        if (!activeActionMaps.Contains(actionMap)) return;
        actionMap.Disable();    
        activeActionMaps.Remove(actionMap);
    }

    /// <summary>
    ///     Disables all the action maps.
    /// </summary>
    public void DisableAllActionMaps()
    {
        PlayerInputActionMap.Disable();
        activeActionMaps.Clear();
    }

    /// <summary>
    ///     Switches to the previously enabled action map
    /// </summary>
    public void SwitchToPreviousActionMap()
    {
        DisableAllActionMaps();
        
        foreach (InputActionMap actionMap in previousActiveActionMaps)
          EnableActionMap(actionMap);
    }

    /// <summary>
    ///     On call, prints all the currently enabled action maps in the debug console
    /// </summary>
    public void PrintCurrentActionMaps()
    {
        if (activeActionMaps.Count == 0) Debug.Log("Empty");
        foreach (InputActionMap inputMap in activeActionMaps)
            Debug.Log(inputMap);
    }
}
