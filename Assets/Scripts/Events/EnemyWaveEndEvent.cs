
public class EnemyWaveEndEvent : IEvent
{
    public int waveIndex;
    public bool gameWon;

    public EnemyWaveEndEvent(int waveIndex, bool gameWon)
    {
        this.waveIndex = waveIndex;
        this.gameWon = gameWon;
    }
}