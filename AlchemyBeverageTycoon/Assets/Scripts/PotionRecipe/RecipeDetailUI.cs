using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeDetailUI : MonoBehaviour
{
    [Header("Main Info")]
    public Image icon;
    public TextMeshProUGUI nameText;

    [Header("Recipe Info")]
    public List<Image> ingrediants = new List<Image>();
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI stirText;

    [Header("Locked Screen")]
    public GameObject lockedPanel;
    public Sprite defaultSprite;

    public void Show(PotionRecipe recipe)
    {
        lockedPanel.SetActive(false);

        icon.sprite = recipe.potion.icon;
        nameText.text = recipe.potion.itemName;

        foreach (var img in ingrediants) 
        {
            img.sprite = defaultSprite;
        }

        for (int i = 0; i < recipe.requiredIngredients.Count; i++)
        {
            ingrediants[i].sprite = recipe.requiredIngredients[i].icon;
        }
        temperatureText.text = $"{recipe.minTemp}° - {recipe.maxTemp}°";
        stirText.text = recipe.optimalStir.ToString();
    }

    public void ShowLocked(PotionData potion)
    {
        lockedPanel.SetActive(true);

        nameText.text = potion.itemName;
        icon.sprite = potion.icon;
    }
}
