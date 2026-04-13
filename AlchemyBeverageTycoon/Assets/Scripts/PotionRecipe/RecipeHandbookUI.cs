using UnityEngine;

public class RecipeHandbookUI : MonoBehaviour
{
    [Header("Data")]
    public PotionRecipeList recipeList;

    [Header("UI")]
    public Transform listParent;            // Content của ScrollView
    public GameObject recipeButtonPrefab;   // Prefab nút

    public RecipeDetailUI detailUI;         // Panel chi tiết

    private void Start()
    {
        PopulateList();
    }

    public void PopulateList()
    {
        foreach (Transform child in listParent)
            Destroy(child.gameObject);

        foreach (var recipe in recipeList.recipes)
        {
            var btn = Instantiate(recipeButtonPrefab, listParent);
            var ui = btn.GetComponent<RecipeButtonUI>();
            ui.Setup(recipe, OnRecipeSelected);
        }
    }

    private void OnRecipeSelected(PotionRecipe recipe)
    {
        if (RecipeUnlockManager.Instance.IsUnlocked(recipe.potion))
        {
           TutorialSignal.Emit("ChosseARecipe");
            detailUI.Show(recipe);
        }
        else
            detailUI.ShowLocked(recipe.potion);
    }
}
