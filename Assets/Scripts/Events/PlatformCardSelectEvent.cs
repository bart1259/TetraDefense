
public class PlatformCardSelectEvent : IEvent
{
    public PlatformSO Platform;

    public PlatformCardSelectEvent(PlatformSO platform)
    {
        Platform = platform;
    }
}