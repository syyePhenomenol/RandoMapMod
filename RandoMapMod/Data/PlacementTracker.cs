namespace RandoMapMod.Data;

public static class PlacementTracker
{
    public static event Action Update;

    internal static void OnUpdate()
    {
        Update?.Invoke();
    }
}
