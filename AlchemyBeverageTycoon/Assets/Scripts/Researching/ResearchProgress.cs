using UnityEngine;

public class ResearchProgress : MonoBehaviour
{
    public static ResearchProgress Instance;

    public int CurrentLevelIndex { get; private set; } = 0; // 0 = Apprentice

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public void LevelUp() => CurrentLevelIndex  ++;

    // New: allow restoring saved level index
    public void SetLevel(int index)
    {
        if (index < 0) index = 0;
        CurrentLevelIndex = index;
    }
}