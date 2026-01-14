using UnityEngine;

public class PlatformPlaceEvent : IEvent
{
    public PlatformSO Platform;
    public GameObject Visuals;

    public PlatformPlaceEvent(PlatformSO platform, GameObject visuals)
    {
        Platform = platform;
        Visuals = visuals;
    }
}