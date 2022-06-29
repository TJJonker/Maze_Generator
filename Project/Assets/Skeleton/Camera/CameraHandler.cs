using UnityEngine;
using Cinemachine;
using MazeGenerator.Input;

public class CameraHandler : MonoBehaviour
{

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private float minOrthographicSize, maxOrthographicSize;

    private PlayerInputActionMap playerInputActionMap;

    private float orthographicSize;
    private float targetOrthographicSize;

    private void Start()
    {
        playerInputActionMap = InputManager.Instance.PlayerInputActionMap;

        orthographicSize = cinemachineVirtualCamera.m_Lens.OrthographicSize;
        targetOrthographicSize = orthographicSize;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleZoom()
    {
        float zoomAmount = 2f / 120;
        targetOrthographicSize -= playerInputActionMap.Generating.CameraZoom.ReadValue<float>() * zoomAmount;
        targetOrthographicSize -= playerInputActionMap.Gameplay.CameraZoom.ReadValue<float>() * zoomAmount;

        targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minOrthographicSize, maxOrthographicSize);

        float zoomSpeed = 5f;
        orthographicSize = Mathf.Lerp(orthographicSize, targetOrthographicSize, Time.deltaTime * zoomSpeed);

        cinemachineVirtualCamera.m_Lens.OrthographicSize = orthographicSize;
    }

    private void HandleMovement()
    {
        Vector2 input = playerInputActionMap.Generating.CameraMovement.ReadValue<Vector2>();

        float moveSpeed = 10f;
        transform.position += (Vector3)input * moveSpeed * Time.deltaTime;
    }
}
