using UnityEngine;
using TMPro;
public class ShowPotionInfo : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI elementMixs;
    public TextMeshProUGUI purityText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI itemRarityText;
    public TextMeshProUGUI explorationBonus;
    public TextMeshProUGUI favoredRegionElement;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI potionValueText;
    public TextMeshProUGUI rarityText;

    public GameObject panelRoot;

    public UIAnimatorTool uiAnimatorTool;

    public void Show(PotionData potionData)
    {
        if (potionData == null) return;

        uiAnimatorTool.Show();
        if (itemNameText != null)
            itemNameText.text = potionData.itemName;
        if (elementMixs != null)
            elementMixs.text = "Element Mixs: " + string.Join(", ", potionData.elementMixs);
        if (purityText != null)
            purityText.text = $"Purity: {potionData.purity * 100f:F1}%";
        if (strengthText != null)
            strengthText.text = $"Strength: {potionData.strength * 100f:F1}%";
        if (itemRarityText != null)
            ColoredRarityText(itemRarityText, potionData.itemRarity);
        if (explorationBonus != null)
            explorationBonus.text = $"Exploration Bonus: {potionData.explorationBonus * 100f:F1}%";
        if (favoredRegionElement != null)
            favoredRegionElement.text = "Counter Elements: " + string.Join(", ", potionData.favoredRegionElement);
        if(descriptionText != null)
            descriptionText.text = potionData.description;
        if(potionValueText != null)
            potionValueText.text = $"Value: <color=#FFFF00>{potionData.baseValue} Gold</color>";
        if (rarityText != null)
            ColoredRarityText( rarityText,potionData.itemRarity);
    }
    public void ColoredRarityText(TextMeshProUGUI textComponent, ItemRarity rarity)
    {
        if (textComponent == null)
            return;

        string colorHex = rarity switch
        {
            ItemRarity.Common => "#FFFFFF",
            ItemRarity.Uncommon => "#1EFF00",
            ItemRarity.Rare => "#0070FF",
            ItemRarity.Epic => "#A335EE",
            ItemRarity.Legendary => "#FF8000",
            _ => "#FFFFFF"
        };

        textComponent.text = $"Rarity: <color={colorHex}>{rarity}</color>";
    }
    public void Hide()
    {
        uiAnimatorTool.HideIfShow();
    }
}
