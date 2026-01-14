
public class EnemyWaveStartEvent : IEvent
{
    public int waveIndex;

    public EnemyWaveStartEvent(int waveIndex)
    {
        this.waveIndex = waveIndex;
    }
}