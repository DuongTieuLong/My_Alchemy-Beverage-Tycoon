using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeButtonUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI title;
    public GameObject lockOverlay;
    public TextMeshProUGUI brewedCount;
    private PotionRecipe recipe;


    public void Setup(PotionRecipe recipe, System.Action<PotionRecipe> onClick)
    {
        this.recipe = recipe;

        icon.sprite = recipe.potion.icon;
        title.text = recipe.potion.itemName;
        if (recipe.brewedCount >= recipe.autoUnlockThreshold)
            brewedCount.text = $"{recipe.autoUnlockThreshold}/{recipe.autoUnlockThreshold}";
        else
            brewedCount.text = $"{recipe.brewedCount}/{recipe.autoUnlockThreshold}"
;

        bool unlocked = RecipeUnlockManager.Instance.IsUnlocked(recipe.potion);
        lockOverlay.SetActive(!unlocked);

        GetComponent<Button>().onClick.AddListener(() =>onClick(recipe));
    }
}
