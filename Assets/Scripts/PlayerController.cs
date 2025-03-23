using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float moveDistancePerTurn = 5f;

    [Header("Interaction")] [SerializeField]
    private float interactionRange = 2f;

    [SerializeField] private GameObject selectionMarkerPrefab;
    [SerializeField] private GameObject interactionMenuPrefab;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debugging")] [SerializeField] private bool debugRaycasts = false;

    private NavMeshAgent _agent;
    private Animator _animator;
    private GameObject _selectionMarker;
    private Camera _camera;
    private bool _isTurnActive;
    private float _distanceMovedInTurn;
    private Vector3 _lastPosition;
    private GameObject _currentInteractionMenu;
    private bool _isInteracting;

    // Animation parameter hashes
    private readonly int _moveSpeedParam = Animator.StringToHash("MoveSpeed");
    private readonly int _isMovingParam = Animator.StringToHash("IsMoving");

    // Input system
    private RPGInputActions _inputActions;
    private InputAction _clickAction;
    private InputAction _pointAction;

    private void Awake()
    {
        // Initialize input actions
        _inputActions = new RPGInputActions();
    }

    private void OnEnable()
    {
        // Subscribe to input events
        _clickAction = _inputActions.Player.Click;
        _pointAction = _inputActions.Player.Point;

        _clickAction.performed += OnClick;

        // Enable the action map
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from input events to prevent memory leaks
        _clickAction.performed -= OnClick;

        // Disable the action map
        _inputActions.Player.Disable();
    }

    private void OnDrawGizmos()
    {
        if (!debugRaycasts || !Application.isPlaying || _pointAction == null) return;

        // Get current mouse position
        Vector2 mousePosition = _pointAction.ReadValue<Vector2>();

        // Only draw if we have a camera
        if (_camera == null) return;

        // Create and visualize the ray
        Ray ray = _camera.ScreenPointToRay(mousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * 100f);

        // Try to hit something and draw a sphere at the hit point
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.point, 0.2f);
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);
        }
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _camera = Camera.main;

        _agent.speed = moveSpeed;
        _agent.angularSpeed = turnSpeed;
        _agent.updateRotation = true;

        // Create selection marker
        if (selectionMarkerPrefab != null)
        {
            _selectionMarker = Instantiate(selectionMarkerPrefab);
            _selectionMarker.SetActive(false);
        }

        _lastPosition = transform.position;
    }

    private void Update()
    {
        // Update movement animations and distance tracking
        UpdateMovement();
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Debug.Log("OnClick");

        // Only handle clicks when not already in an interaction
        if (_isInteracting) return;

        // Get mouse position from the Point action
        Vector2 mousePosition = _pointAction.ReadValue<Vector2>();

        // Create a ray from the camera through the mouse position
        Ray ray = _camera.ScreenPointToRay(mousePosition);

        // First, check for interactable objects
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
        {
            HandleInteractableHit(hit);
        }
        // Then check for ground to move
        else if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            HandleGroundHit(hit);
        }
    }

    private void UpdateMovement()
    {
        // Check if we're moving
        if (_agent.velocity.magnitude > 0.1f)
        {
            float speed = _agent.velocity.magnitude / moveSpeed;
            if (_animator != null)
            {
                _animator.SetFloat(_moveSpeedParam, speed);
                _animator.SetBool(_isMovingParam, true);
            }

            // Track distance moved for turn-based gameplay
            if (_isTurnActive)
            {
                _distanceMovedInTurn += Vector3.Distance(transform.position, _lastPosition);

                // Check if we've moved our allowed distance
                if (_distanceMovedInTurn >= moveDistancePerTurn)
                {
                    StopMoving();
                    EndTurn();
                }
            }
        }
        else if (_animator != null)
        {
            _animator.SetFloat(_moveSpeedParam, 0);
            _animator.SetBool(_isMovingParam, false);
        }

        _lastPosition = transform.position;
    }

    private void HandleInteractableHit(RaycastHit hit)
    {
        // Get interactable component
        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        if (interactable == null) return;

        // Calculate distance to interactable
        float distanceToTarget = Vector3.Distance(transform.position, hit.point);

        // If already within range, interact immediately
        if (distanceToTarget <= interactionRange)
        {
            ShowInteractionMenu(interactable, hit.point);
        }
        else
        {
            // Move to interactable first
            Vector3 targetPos = hit.collider.ClosestPoint(transform.position);
            Vector3 direction = (targetPos - transform.position).normalized;
            Vector3 destinationPoint = targetPos - (direction * (interactionRange * 0.8f));

            // Show selection marker
            if (_selectionMarker != null)
            {
                _selectionMarker.transform.position = destinationPoint;
                _selectionMarker.SetActive(true);
            }

            // Move to the destination
            _agent.SetDestination(destinationPoint);

            // Start coroutine to wait until in range
            StartCoroutine(MoveToInteractable(interactable, targetPos));
        }
    }

    private void HandleGroundHit(RaycastHit hit)
    {
        // Just move to clicked ground position
        _agent.SetDestination(hit.point);

        // Show selection marker
        if (_selectionMarker != null)
        {
            _selectionMarker.transform.position = hit.point;
            _selectionMarker.SetActive(true);

            // Hide marker after delay
            StartCoroutine(HideMarkerAfterDelay(1.5f));
        }
    }

    private IEnumerator MoveToInteractable(IInteractable interactable, Vector3 targetPos)
    {
        // Wait until we're close enough or stopped moving
        while (_agent.pathPending ||
               (_agent.remainingDistance > interactionRange && _agent.remainingDistance > _agent.stoppingDistance))
        {
            yield return null;
        }

        // Hide the selection marker
        if (_selectionMarker != null)
        {
            _selectionMarker.SetActive(false);
        }

        // Check if we're actually within range
        if (Vector3.Distance(transform.position, targetPos) <= interactionRange)
        {
            // Look at the interactable
            Vector3 lookDirection = targetPos - transform.position;
            lookDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDirection);

            // Show interaction menu
            ShowInteractionMenu(interactable, targetPos);
        }
    }

    private void ShowInteractionMenu(IInteractable interactable, Vector3 position)
    {
        _isInteracting = true;

        // Get interaction options
        string[] options = interactable.GetInteractionOptions();

        // If there's only one option, execute it directly
        if (options.Length <= 1)
        {
            interactable.Interact(this);
            _isInteracting = false;
            return;
        }

        // Create and position menu
        if (interactionMenuPrefab != null)
        {
            _currentInteractionMenu = Instantiate(interactionMenuPrefab, transform.position, Quaternion.identity);
            InteractionMenu menu = _currentInteractionMenu.GetComponent<InteractionMenu>();

            if (menu != null)
            {
                menu.Initialize(options, (option) => { ExecuteInteraction(interactable, option); });

                // Position the menu above the interactable in screen space
                Vector3 screenPos = _camera.WorldToScreenPoint(position + Vector3.up * 1.5f);
                menu.SetPosition(screenPos);
            }
        }
    }

    private void ExecuteInteraction(IInteractable interactable, string option)
    {
        _isInteracting = false;

        // Destroy menu
        if (_currentInteractionMenu != null)
        {
            Destroy(_currentInteractionMenu);
            _currentInteractionMenu = null;
        }

        // Execute the interaction
        interactable.InteractWithOption(this, option);
    }

    public void StopMoving()
    {
        _agent.ResetPath();

        if (_animator == null) return;
        _animator.SetBool(_isMovingParam, false);
        _animator.SetFloat(_moveSpeedParam, 0);
    }

    public void StartTurn()
    {
        _isTurnActive = true;
        _distanceMovedInTurn = 0f;
    }

    public void EndTurn()
    {
        _isTurnActive = false;

        // Notify turn manager (you'll implement this later)
        TurnManager.Instance?.EndPlayerTurn();
    }

    private IEnumerator HideMarkerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_selectionMarker != null)
        {
            _selectionMarker.SetActive(false);
        }
    }

    // Public methods for external systems to query state
    public bool IsMoving() => _agent.velocity.magnitude > 0.1f;
    public bool IsTurnActive() => _isTurnActive;
    public float GetRemainingMoveDistance() => moveDistancePerTurn - _distanceMovedInTurn;
}
