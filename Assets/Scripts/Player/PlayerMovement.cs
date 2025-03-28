using System.Collections;
using TurnHandling;
using UnityEngine;
using UnityEngine.AI;

namespace Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float turnSpeed = 10f;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float stoppingDistance = 0.1f;
        [SerializeField] private GameObject selectionMarkerPrefab;
        [SerializeField] private bool debugPath;

        private NavMeshAgent _agent;
        private Animator _animator;
        private GameObject _selectionMarker;
        private Vector3 _targetDestination;
        private bool _hasPendingDestination;

        // Animation hashes
        private readonly int _moveSpeedParam = Animator.StringToHash("MoveSpeed");
        private readonly int _isMovingParam = Animator.StringToHash("IsMoving");

        public bool IsMoving => _agent != null && _agent.velocity.magnitude > 0.1f;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

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

        public void MoveTo(Vector3 destination)
        {
            _targetDestination = destination;
            _hasPendingDestination = true;

            if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
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
                Debug.LogWarning("Destination is not on NavMesh!");
                _hasPendingDestination = false;
            }
        }

        public void StopMoving()
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _hasPendingDestination = false;

            if (_animator != null)
            {
                _animator.SetBool(_isMovingParam, false);
                _animator.SetFloat(_moveSpeedParam, 0);
            }

            HideSelectionMarker();
        }

        public void HideSelectionMarker()
        {
            if (_selectionMarker != null)
            {
                _selectionMarker.SetActive(false);
            }
        }

        public IEnumerator MoveToInteractable(Vector3 targetPos, float interactionRange, System.Action onReachedTarget)
        {
            // First, wait one frame to ensure NavMeshAgent has started processing the path
            yield return null;

            // Wait until either:
            // 1. We're close enough to the target
            // 2. The agent stopped moving (path completed or canceled)
            // 3. A new destination was set
            while (true)
            {
                // Check if a new destination was set during this coroutine
                if (_hasPendingDestination && _targetDestination != _agent.destination)
                {
                    yield break; // Exit if player clicked elsewhere
                }

                // Check if we've reached the interaction range
                float currentDistance = Vector3.Distance(transform.position, targetPos);
                if (currentDistance <= interactionRange)
                {
                    break; // Exit the loop if we're in range
                }

                // Check if we've stopped moving but didn't reach the target
                if (!_agent.pathPending && !_agent.hasPath && _agent.velocity.sqrMagnitude < 0.1f)
                {
                    yield break; // Exit if we can't reach the target
                }

                yield return null;
            }

            HideSelectionMarker();

            // Look at the interactable
            Vector3 lookDirection = targetPos - transform.position;
            lookDirection.y = 0;
            if (lookDirection.sqrMagnitude > 0.001f) // Prevent errors with zero vectors
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            // Execute callback when in range
            onReachedTarget?.Invoke();
        }

        private void UpdateMovement()
        {
            // Check if we're still moving
            if (IsMoving)
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

                // If we're not moving anymore and had a pending destination
                if (_hasPendingDestination && !_agent.hasPath)
                {
                    // Check if we've reached our destination (within stopping distance)
                    if (Vector3.Distance(transform.position, _targetDestination) <= _agent.stoppingDistance + 0.01f)
                    {
                        _hasPendingDestination = false;
                        HideSelectionMarker();

                        // Notify the turn manager
                        TurnManager.Instance?.EndPlayerTurn();
                    }
                    // If path is complete but we're not at the destination, something prevented us from reaching it
                    else if (!_agent.pathPending)
                    {
                        _hasPendingDestination = false;
                        HideSelectionMarker();
                    }
                }
            }
        }
    }
}
