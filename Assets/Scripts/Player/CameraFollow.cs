using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private float smoothSpeed = 8f;

    private float cameraZ;

    private void Awake()
    {
        if (target == null)
        {
            PlayerMovement player =
                FindAnyObjectByType<PlayerMovement>();

            if (player != null)
                target = player.transform;
        }

        // Only preserve the camera's depth.
        cameraZ = transform.position.z;
    }

    private void Start()
    {
        if (target == null)
            return;

        // Immediately center the camera on the player.
        transform.position = new Vector3(
            target.position.x,
            target.position.y,
            cameraZ
        );
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            cameraZ
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}