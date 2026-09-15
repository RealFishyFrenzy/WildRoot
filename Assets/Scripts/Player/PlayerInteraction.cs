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

    private void Start()
    {
        GameplayFeedbackUI view = GetComponent<GameplayFeedbackUI>();
        if (view == null)
            view = gameObject.AddComponent<GameplayFeedbackUI>();
        view.SetInteraction(this);
    }

    public void TryInteract()
    {
        IInteractable interactable = ResolveTarget();
        interactable?.Interact();
    }

    // Both the prompt and E use this exact query and control gate.
    public IInteractable ResolveTarget()
    {
        if (PlayerController.Instance == null || !PlayerController.Instance.ControlsEnabled ||
            playerMovement == null || !isActiveAndEnabled)
            return null;
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
            if (interactable is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                return null;
            return interactable;
        }
        return null;
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
