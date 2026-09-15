using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool movementLocked;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    public bool MovementLocked => movementLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
        movement = Vector2.zero;

        if (locked && rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log($"Player movement locked: {movementLocked}");
    }

    private void Update()
    {
        if (!PlayerController.Instance.ControlsEnabled || movementLocked)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;

        if (movement != Vector2.zero)
        {
            FacingDirection = movement;
        }
    }

    private void FixedUpdate()
    {
        if (!PlayerController.Instance.ControlsEnabled || movementLocked)
        {
            movement = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
