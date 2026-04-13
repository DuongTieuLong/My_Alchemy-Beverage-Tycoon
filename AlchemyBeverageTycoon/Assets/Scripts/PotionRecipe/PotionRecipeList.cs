using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PotionRecipeList", menuName = "Alchemy/Recipe List")]
public class PotionRecipeList : ScriptableObject
{
    public List<PotionRecipe> recipes;
}
