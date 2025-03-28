using System.Collections;
using Interaction;
using UI.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour, IInteractionSource
{
    [Header("Movement")]
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float moveSpeed = 10f;

    [Header("Navigation Settings")]
    [SerializeField] private float stoppingDistance = 0.1f;
    [SerializeField] private bool debugPath;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private GameObject selectionMarkerPrefab;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debugging")]
    [SerializeField] private bool debugRaycasts;

    private NavMeshAgent _agent;
    private Animator _animator;
    private GameObject _selectionMarker;
    private Camera _camera;
    private Vector3 _targetDestination;
    private bool _hasPendingDestination;
    private InteractionManager _interactionManager;

    // Animation parameter hashes
    private readonly int _moveSpeedParam = Animator.StringToHash("MoveSpeed");
    private readonly int _isMovingParam = Animator.StringToHash("IsMoving");

    // Input system
    private RPGInputActions _inputActions;
    private InputAction _clickAction;
    private InputAction _pointAction;

    private void Awake()
    {
        _inputActions ??= new RPGInputActions();

        _interactionManager = FindAnyObjectByType<InteractionManager>();
        if (_interactionManager == null)
            Debug.LogError("InteractionManager not found in scene!");
    }

    private void OnEnable()
    {
        _inputActions ??= new RPGInputActions();

        // Subscribe to input events
        _clickAction = _inputActions.Gameplay.Click;
        _pointAction = _inputActions.Gameplay.Point;

        _clickAction.performed += OnClick;

        // Enable the action map
        _inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from input events to prevent memory leaks
        _clickAction.performed -= OnClick;

        // Disable the action map
        _inputActions.Gameplay.Disable();
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _camera = Camera.main;

        // Set agent properties
        _agent.speed = moveSpeed;
        _agent.angularSpeed = turnSpeed;
        _agent.updateRotation = true;
        _agent.stoppingDistance = stoppingDistance;
        _agent.autoBraking = true;

        // Create selection marker
        if (selectionMarkerPrefab != null)
        {
            _selectionMarker = Instantiate(selectionMarkerPrefab);
            _selectionMarker.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateMovement();

        // Debug path visualization
        if (debugPath && _agent.hasPath)
        {
            Debug.DrawLine(transform.position, _agent.destination, Color.blue);
            if (_agent.path != null && _agent.path.corners.Length > 1)
            {
                for (int i = 0; i < _agent.path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(_agent.path.corners[i], _agent.path.corners[i + 1], Color.red);
                }
            }
        }
    }

    private bool _isInteractionBlocked;
    private bool IsInteractionBlocked => _isInteractionBlocked || UILayerManager.ActiveLayerCount > 0;

    private void OnClick(InputAction.CallbackContext context)
    {
        // Only handle clicks when the interaction is not blocked
        if (IsInteractionBlocked) return;

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

    private void HandleInteractableHit(RaycastHit hit)
    {
        // Get interactable component
        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        if (interactable == null) return;

        // Always register with the interaction manager
        _interactionManager?.SetInteractionSource(this.gameObject);

        float distanceToTarget = Vector3.Distance(transform.position, hit.point);

        // If already within range, interact immediately
        if (distanceToTarget <= interactionRange)
        {
            _isInteractionBlocked = true;

            _interactionManager?.ShowInteractionOptions(interactable, hit.point);
            // NOTE: InteractionManager will call our FinalizeInteraction when done
            _isInteractionBlocked = false;

        }
        else
        {
            // Move to interactable first
            Vector3 targetPos = hit.collider.ClosestPoint(transform.position);
            Vector3 direction = (targetPos - transform.position).normalized;
            Vector3 destinationPoint = targetPos - (direction * (interactionRange * 0.8f));

            // Find nearest point on navmesh
            if (NavMesh.SamplePosition(destinationPoint, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                _targetDestination = navHit.position;
                _agent.SetDestination(navHit.position);
                _agent.isStopped = false;
            }

            // Show selection marker
            if (_selectionMarker != null)
            {
                _selectionMarker.transform.position = _targetDestination;
                _selectionMarker.SetActive(true);
            }

            // Start coroutine to wait until in range
            StartCoroutine(MoveToInteractable(interactable, targetPos));
        }
    }

    private void HandleGroundHit(RaycastHit hit)
    {
        // Update the target destination
        _targetDestination = hit.point;
        _hasPendingDestination = true;

        // Set destination for NavMeshAgent
        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
        {
            _agent.SetDestination(navHit.position);
            _agent.isStopped = false;

            // Show selection marker
            if (_selectionMarker != null)
            {
                _selectionMarker.transform.position = navHit.position;
                _selectionMarker.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Clicked position is not on NavMesh!");
        }
    }

    private void UpdateMovement()
    {
        // Check if we're still moving
        bool isMoving = _agent.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            float speed = _agent.velocity.magnitude / moveSpeed;
            if (_animator != null)
            {
                _animator.SetFloat(_moveSpeedParam, speed);
                _animator.SetBool(_isMovingParam, true);
            }
        }
        else
        {
            // We've stopped moving
            if (_animator != null)
            {
                _animator.SetFloat(_moveSpeedParam, 0);
                _animator.SetBool(_isMovingParam, false);
            }

            // If we have reached our destination...
            if (Vector3.Distance(transform.position, _targetDestination) <= _agent.stoppingDistance)
            {
                // ...hide the marker
                if (_selectionMarker != null)
                {
                    _selectionMarker.SetActive(false);
                }
                // and notify the turn manager
                TurnManager.Instance?.EndPlayerTurn();
            }
        }
    }

    private IEnumerator MoveToInteractable(IInteractable interactable, Vector3 targetPos)
    {
        // Keep checking if we're close enough while path is still active
        while (_agent.pathPending ||
               (_agent.hasPath && _agent.remainingDistance > interactionRange))
        {
            // Check if a new destination was set during this coroutine
            if (_hasPendingDestination && _targetDestination != _agent.destination)
            {
                yield break; // Exit the coroutine if a new destination was set
            }

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

            // Now we can interact
            _isInteractionBlocked = true;
            _interactionManager?.ShowInteractionOptions(interactable, targetPos);
        }
    }

    // IInteractionSource implementation
    public float InteractionRange => interactionRange;

    public Transform Transform => transform;

    public void FinalizeInteraction(IInteractable interactable)
    {
        Debug.Log("Interaction complete.");
    }

    public void StopMoving()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
        _hasPendingDestination = false;

        if (_animator == null) return;
        _animator.SetBool(_isMovingParam, false);
        _animator.SetFloat(_moveSpeedParam, 0);
    }

    public void StartTurn()
    {
    }

    public void EndTurn()
    {
    }

    public bool IsMoving() => _agent.velocity.magnitude > 0.1f;
}
