using UnityEngine;

public class AnimalPlacementZone : MonoBehaviour
{
    [SerializeField] private PlacementZoneType zoneType;

    public PlacementZoneType ZoneType => zoneType;
}