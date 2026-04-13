using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Potion Data")]
public class PotionData : ItemData
{
    public Rank potionRank;
    [Header("Potion Stats")]
    [Range(0f, 1f)] public float purity = 0.8f;      // Độ tinh khiết (0–1)
    [Range(0f, 1f)] public float strength;    // Dược lực (0–1)
    public List<ElementType> elementMixs;          // Phản ứng phép thuật (nguyên tố)
 

    [Header("Expedition Bonus")]
    [Tooltip("Tăng tỉ lệ thành công của thám hiểm khi mang theo.")]
    [Range(0f, 1f)] public float explorationBonus = 0f;

    [Tooltip("Chỉ có hiệu lực ở khu vực có nguyên tố tương thích.")]
    public List<ElementType> favoredRegionElement;

    public float GetExpeditionBoost(ElementType regionElement)
    {
        foreach (var elem in favoredRegionElement)
        {
            if (elem == regionElement)
            {
                return explorationBonus * 1.5f;
            }
        }
        return explorationBonus;
    }

    public float GetSellPriceMultiplier()
    {
        // Giá trị bán = baseValue * (Purity + Strength)
        return Mathf.Clamp01(purity + strength);
    }

    public void SetRarityBasedOnStats()
    {
        float averageStat = (purity + strength) / 2f;
        if (averageStat >= 0.9f)
        {
            itemRarity = ItemRarity.Legendary;
        }
        else if (averageStat >= 0.80f)
        {
            itemRarity = ItemRarity.Epic;
        }
        else if (averageStat >= 0.7f)
        {
            itemRarity = ItemRarity.Rare;
        }
        else if (averageStat >= 0.5f)
        {
            itemRarity = ItemRarity.Uncommon;
        }
        else
        {
            itemRarity = ItemRarity.Common;
        }
    }
}

public enum ElementType
{
    Nature,
    Fire,
    Water,
    Spirit,
    Ice,
    Light,
    Dark,
    Secret
}

