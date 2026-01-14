
public class TowerSelectEvent : IEvent
{
    public TowerSO Tower;

    public TowerSelectEvent(TowerSO tower)
    {
        Tower = tower;
    }
}