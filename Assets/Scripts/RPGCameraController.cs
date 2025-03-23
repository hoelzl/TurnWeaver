using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class RPGCameraController : MonoBehaviour
{
    [Header("Cinemachine Setup")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform cameraTarget;

    [Header("Camera Controls")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoomDistance = 3f;
    [SerializeField] private float maxZoomDistance = 15f;
    [SerializeField] private Vector3 initialOffset = new Vector3(0, 10, -8);

    private CinemachineFollow _follow;
    private float _currentZoom;

    // Input system
    private RPGInputActions _inputActions;
    private InputAction _rightClickAction;
    private InputAction _pointAction;
    private InputAction _zoomAction;
    private Vector2 _lastMousePosition;
    private bool _isRotating = false;

    private void Awake()
    {
        // Initialize input actions
        _inputActions = new RPGInputActions();
    }

    private void OnEnable()
    {
        // Subscribe to input events
        _rightClickAction = _inputActions.Player.RightClick;
        _pointAction = _inputActions.Player.Point;
        _zoomAction = _inputActions.Player.Zoom;

        _rightClickAction.started += OnRotateStart;
        _rightClickAction.canceled += OnRotateEnd;
        _zoomAction.performed += OnZoom;

        // Enable the action map
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        _rightClickAction.started -= OnRotateStart;
        _rightClickAction.canceled -= OnRotateEnd;
        _zoomAction.performed -= OnZoom;

        // Disable the action map
        _inputActions.Player.Disable();
    }

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera not assigned to RPGCameraController!");
            return;
        }

        // Get the transposer component for zoom control
        _follow = virtualCamera.GetComponent<CinemachineFollow>();
        if (_follow == null) return;

        // Set initial position
        _follow.FollowOffset = initialOffset;
        _currentZoom = initialOffset.magnitude;
    }

    private void Update()
    {
        // Handle camera rotation only when right mouse button is held
        if (_isRotating)
        {
            Vector2 currentMousePosition = _pointAction.ReadValue<Vector2>();
            Vector2 mouseDelta = currentMousePosition - _lastMousePosition;

            float horizontal = mouseDelta.x * rotationSpeed * Time.deltaTime;
            cameraTarget.Rotate(0, horizontal, 0);

            _lastMousePosition = currentMousePosition;
        }
    }

    private void OnRotateStart(InputAction.CallbackContext context)
    {
        _isRotating = true;
        _lastMousePosition = _pointAction.ReadValue<Vector2>();
    }

    private void OnRotateEnd(InputAction.CallbackContext context)
    {
        _isRotating = false;
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        if (_follow == null) return;

        float scrollValue = context.ReadValue<float>();

        // Calculate new zoom level
        _currentZoom = Mathf.Clamp(_currentZoom - scrollValue * zoomSpeed, minZoomDistance, maxZoomDistance);

        // Apply zoom while maintaining direction
        Vector3 zoomDirection = _follow.FollowOffset.normalized;
        _follow.FollowOffset = zoomDirection * _currentZoom;
    }
}
