using Interaction;
using UnityEngine;
using UnityEngine.AI;

namespace Player
{
    public class PlayerInteractionController : MonoBehaviour, IInteractionSource
    {
        [SerializeField] private float interactionRange = 2f;

        private InteractionManager _interactionManager;
        private PlayerMovement _movement;
        public bool IsInteractionBlocked { get; private set; }

        // IInteractionSource implementation
        public float InteractionRange => interactionRange;
        public Transform Transform => transform;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _interactionManager = FindAnyObjectByType<InteractionManager>();

            if (_interactionManager == null)
                Debug.LogError("InteractionManager not found in scene!");
        }

        public bool AttemptInteraction(RaycastHit hit)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null) return false;

            // Always register with the interaction manager
            _interactionManager?.SetInteractionSource(this.gameObject);
            float distanceToTarget = Vector3.Distance(transform.position, hit.point);

            // If already within range, interact immediately
            if (distanceToTarget <= interactionRange)
            {
                ShowInteractionOptions(interactable);
                return true;
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
                    _movement.MoveTo(navHit.position);
                }

                // Start coroutine to wait until in range
                StartCoroutine(_movement.MoveToInteractable(
                    targetPos,
                    interactionRange,
                    () => ShowInteractionOptions(interactable)
                ));

                return true;
            }
        }

        private void ShowInteractionOptions(IInteractable interactable)
        {
            IsInteractionBlocked = true;
            _interactionManager?.ShowInteractionOptions(interactable);
        }

        public void FinalizeInteraction(IInteractable interactable)
        {
            IsInteractionBlocked = false;
            Debug.Log("Interaction complete.");
        }
    }
}
