using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 1f;
    [SerializeField] private float interactionRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void TryInteract()
    {
        if (!PlayerController.Instance.ControlsEnabled)
            return;
        Vector2 interactionPoint =
            (Vector2)transform.position +
            playerMovement.FacingDirection * interactionDistance;

        Collider2D hit = Physics2D.OverlapCircle(
            interactionPoint,
            interactionRadius,
            interactableLayer
        );

        if (hit != null && hit.TryGetComponent(out IInteractable interactable))
        {
            interactable.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement == null)
            return;

        Vector2 interactionPoint =
            (Vector2)transform.position +
            movement.FacingDirection * interactionDistance;

        Gizmos.DrawWireSphere(interactionPoint, interactionRadius);
    }
}
