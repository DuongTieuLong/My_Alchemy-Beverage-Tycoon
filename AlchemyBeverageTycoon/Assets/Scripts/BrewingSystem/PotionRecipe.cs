using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Potion Recipe")]
public class PotionRecipe : ScriptableObject
{
    public Rank recipeRank;
    public PotionData potion;                  // Potion tạo ra
    public List<ItemData> requiredIngredients; // 1-3 nguyên liệu chính
    public float timeToBrew;
    public float minTemp = 89f;                // Nhiệt tối thiểu
    public float maxTemp = 110f;               // Nhiệt tối đa
    public StirLevel optimalStir;              // Mức khuấy tối ưu
    public int autoUnlockThreshold;
    public int brewedCount;
}

public enum StirLevel
{
    None,
    Low,
    Medium,
    High
}

public enum BrewResult
{
    Success,
    Failed,
}
