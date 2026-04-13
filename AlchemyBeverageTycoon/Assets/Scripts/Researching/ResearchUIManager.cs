using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class ResearchUIManager : MonoBehaviour
{
    [Header("Data")]
    public List<ResearchLevelData> levels;

    [Header("UI")]
    public TextMeshProUGUI levelTitle;
    public Transform slotParent;
    public GameObject slotPrefab;
    public TextMeshProUGUI goldText;
    public Button researchButton;

    private ResearchLevelData current;
    public InventoryManager InventoryManager;
    public UIAnimatorTool uIAnimatorTool;

    public UIAnimatorTool completeResearchPanel;
    public Transform content;
    public TextMeshProUGUI notification;
    public GameObject recipeResearchPrefabs;

    private void Start()
    {
        RefreshUI();
    }

    public void OnOpenResearch()
    {
        RefreshUI();
        uIAnimatorTool.Show(); 
    }


    private void RefreshUI()
    {
        int index = ResearchProgress.Instance.CurrentLevelIndex;
        if (index < levels.Count)
            current = levels[index];
        else
            current = levels[levels.Count - 1];

        levelTitle.text = current.levelName;
        goldText.text = current.goldRequired.ToString();

        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        foreach (var req in current.itemRequirements)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            var ui = slot.GetComponent<ResearchItemSlotUI>();
            ui.Setup(req);
        }

        CheckRequirementStatus();
    }

    private void CheckRequirementStatus()
    {
        bool enoughItems = true;

        foreach (var req in current.itemRequirements)
        {
            int have = InventoryManager.GetItemCount(req.item);

            if (have < req.requiredAmount)
            {
                enoughItems = false;
                break;
            }
        }

        bool enoughGold = GoldManager.Instance.Gold >= current.goldRequired;
        bool enoughRank = (int)ReputationManager.Instance.currentRank >= ResearchProgress.Instance.CurrentLevelIndex;

        researchButton.interactable = (enoughItems && enoughGold && enoughRank && !current.isMax);
    }

    public void OnClickResearch()
    {
        if (!researchButton.interactable) return;

        //Unlock Recipe
        foreach (var recipe in current.recipeToUnlock)
        {
            RecipeUnlockManager.Instance.Unlock(recipe);
        }

        //Show noitification
        completeResearchPanel.Show();

        foreach(Transform t in content)
        {
            Destroy(t.gameObject);
        }

        foreach(var recipe in current.recipeToUnlock)
        {
            GameObject recipeO = GameObject.Instantiate(recipeResearchPrefabs,content);
            recipeO.transform.Find("Icon").GetComponent<Image>().sprite = recipe.icon;
            recipeO.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = recipe.name;
        }
        notification.text = $"x{current.recipeToUnlock.Count} new recipes have been added to the recipe book";
        

        // Trừ item
        foreach (var req in current.itemRequirements)
        {
            InventoryManager.RemoveItem(req.item, req.requiredAmount);
        }

        // Trừ tiền
        GoldManager.Instance.SpendGold(current.goldRequired);

        // Lên cấp
        ResearchProgress.Instance.LevelUp();

        // Làm mới UI
        RefreshUI();
        uIAnimatorTool.HideIfShow();
    }
}
