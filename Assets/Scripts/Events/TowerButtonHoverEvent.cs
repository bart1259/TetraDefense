using UnityEngine;

public class TowerButtonHoverEvent : IEvent
{
    public TowerSO Tower;
    public bool IsHovering;

    public TowerButtonHoverEvent(TowerSO tower, bool isHovering)
    {
        Tower = tower;
        IsHovering = isHovering;
    }
}