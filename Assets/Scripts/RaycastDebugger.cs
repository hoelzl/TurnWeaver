using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastDebugger : MonoBehaviour
{
    [SerializeField] private float rayLength = 100f;
    [SerializeField] private Color rayColor = Color.yellow;
    [SerializeField] private float rayDuration = 1f;
    [SerializeField] private bool drawHitNormal = true;
    [SerializeField] private float hitNormalLength = 2f;
    [SerializeField] private Color hitNormalColor = Color.green;
    [SerializeField] private LayerMask layersToCheck;

    private Camera _mainCamera;
    private RPGInputActions _inputActions;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _inputActions = new RPGInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Gameplay.Designate.performed += OnClick;
        _inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Gameplay.Designate.performed -= OnClick;
        _inputActions.Gameplay.Disable();
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = _inputActions.Gameplay.Point.ReadValue<Vector2>();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        // Draw the ray in scene view
        Debug.DrawRay(ray.origin, ray.direction * rayLength, rayColor, rayDuration);

        // Log general ray information
        Debug.Log($"Ray Origin: {ray.origin}, Direction: {ray.direction}");

        // Try raycast against specified layers
        if (Physics.Raycast(ray, out RaycastHit hitInfo, rayLength, layersToCheck))
        {
            // Draw hit point and normal
            if (drawHitNormal)
            {
                Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * hitNormalLength,
                    hitNormalColor, rayDuration);
            }

            // Log hit information
            Debug.Log($"Hit: {hitInfo.collider.gameObject.name}, " +
                      $"Layer: {LayerMask.LayerToName(hitInfo.collider.gameObject.layer)}, " +
                      $"Point: {hitInfo.point}, Normal: {hitInfo.normal}, " +
                      $"Distance: {hitInfo.distance}");
        }
        else
        {
            Debug.Log($"No hit found against layers {layersToCheck}");

            // Try raycast against everything as a fallback
            if (Physics.Raycast(ray, out hitInfo, rayLength))
            {
                Debug.Log($"Hit against all layers: {hitInfo.collider.gameObject.name}, " +
                          $"Layer: {LayerMask.LayerToName(hitInfo.collider.gameObject.layer)}");
            }
            else
            {
                Debug.Log("Raycast didn't hit anything at all!");
            }
        }
    }
}
