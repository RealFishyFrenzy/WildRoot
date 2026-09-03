using UnityEngine;

public class AreaTransition : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private Transform destination;

    private bool transitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transitioning)
            return;

        PlayerMovement player =
            other.GetComponent<PlayerMovement>();

        if (player == null || destination == null)
            return;

        transitioning = true;

        TeleportPlayer(player);
    }

    private void TeleportPlayer(PlayerMovement player)
    {
        player.transform.position = destination.position;

        transitioning = false;
    }
}