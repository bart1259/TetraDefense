using UnityEngine;

public class TowerPlaceEvent : IEvent
{
    public TowerSO Tower;
    public GameObject Visuals;

    public TowerPlaceEvent(TowerSO tower, GameObject visuals)
    {
        Tower = tower;
        Visuals = visuals;
    }
}