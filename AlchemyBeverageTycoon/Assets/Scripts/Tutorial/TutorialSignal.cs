using System;

public static class TutorialSignal
{
    public static event Action<string> OnSignal;

    public static void Emit(string signalID)
    {
        OnSignal?.Invoke(signalID);
    }
}
