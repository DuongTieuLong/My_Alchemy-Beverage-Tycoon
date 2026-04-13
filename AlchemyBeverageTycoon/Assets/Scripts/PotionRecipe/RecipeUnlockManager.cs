using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeUnlockManager : MonoBehaviour
{
    public static RecipeUnlockManager Instance;
    [SerializeField] private HashSet<PotionData> unlocked = new HashSet<PotionData>();

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public bool IsUnlocked(PotionData potion)
        => unlocked.Contains(potion);

    public void Unlock(PotionData potion)
    {
        if (potion == null) return;
        unlocked.Add(potion);
        // Future: Save data vào file
    }

    // New: get unlocked list as names for save system
    public List<string> GetUnlockedNames()
    {
        return unlocked.Where(p => p != null).Select(p => p.itemName).ToList();
    }

    // New: set unlocked from saved names (best-effort resolution)
    public void SetUnlockedFromNames(List<string> names)
    {
        if (names == null || names.Count == 0) return;

        // Attempt to resolve via Resources first, then fall back to loaded assets
        var allPotions = Resources.LoadAll<PotionData>("Potions/");
        var map = allPotions.Where(p => p != null).ToDictionary(p => p.itemName, p => p);

        foreach (var name in names)
        {
            if (string.IsNullOrEmpty(name)) continue;
            if (map.TryGetValue(name, out var potion))
                unlocked.Add(potion);
            else
            {
                // not found in loaded assets; try Resources.Load by path (Potions/<name>)
                var loaded = Resources.Load<PotionData>($"Potions/{name}");
                if (loaded != null)
                    unlocked.Add(loaded);

            }
        }
    }
}