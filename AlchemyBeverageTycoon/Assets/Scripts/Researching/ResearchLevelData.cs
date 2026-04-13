using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResearchLevelData", menuName = "Alchemy/Research Level")]
public class ResearchLevelData : ScriptableObject
{
    public List<PotionData> recipeToUnlock;
    public string levelName;                      // Apprentice – Adept – ...
    public int goldRequired;                      // tiền cần
    public List<ItemRequirement> itemRequirements;
    public bool isMax;
}

[System.Serializable]
public class ItemRequirement
{
    public ItemData item;
    public int requiredAmount;
}
