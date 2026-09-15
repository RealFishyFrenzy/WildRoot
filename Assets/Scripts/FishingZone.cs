using UnityEngine;

public class FishingZone : MonoBehaviour
{
    [Header("Fishing")]
    [SerializeField] private FishPool fishPool;

    public FishPool FishPool => fishPool;

    public bool Contains(Vector3 worldPos)
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();

        if (zoneCollider == null)
            return false;

        return zoneCollider.OverlapPoint(worldPos);
    }
}
