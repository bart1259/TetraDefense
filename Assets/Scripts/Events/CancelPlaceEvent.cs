using UnityEngine;

public class CancelPlaceEvent : IEvent
{
    public bool PlacingPlatform => Platform != null;
    public bool PlacingTower => Tower != null;
    public PlatformSO Platform;
    public TowerSO Tower;

    public CancelPlaceEvent(PlatformSO platform, TowerSO tower)
    {
        Platform = platform;
        Tower = tower;
    }
}