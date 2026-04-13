using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cauldron : MonoBehaviour
{
    public static Cauldron Instance;

    [Header("References")]
    public InventoryManager inventoryManager;

    public UIAnimatorTool brewingPanel;
    public UIAnimatorTool potionInfoPenel;

    [Header("Current Brew")]
    public List<Button> selectedIngredientsButton = new List<Button>();
    public List<ItemData> selectedIngredients = new List<ItemData>();
    public Dictionary<ItemData, Button> ingredientButtonMap = new Dictionary<ItemData, Button>();
    public Button selectedCatalystButton;

    public ItemData selectedCatalyst;
    public ItemType currentTypeNeed;
    public Sprite lockedIngredientIcon;
    public Sprite defaultIngredientIcon;


    private int indexOfSelectButton = -1;
    private float temperature;
    private StirLevel stirLevel;
    private int stirLevelInt = 0;

    [Header("Brew Control")]
    public Slider temperatureSlider;
    public TextMeshProUGUI temperatureText;
    public Slider stirSlider;
    public Button stirButton;
    public Image brewIcon;
    public Button brewButton;
    public Button temperaturePlusButton;
    public Button temperatureMinusButton;
    [Header("Effects")]
    public AudioSource brewSFXSource;
    public AudioClip brewSFX;

    [Header("Recipes")]
    public List<PotionRecipe> recipes;

    [Header("Auto Brew")]
    public Toggle autoBrew;
    public bool autoBrewUnlocked = false; // managed automatically now
    public Slider amountSlider;
    public TextMeshProUGUI amountText;

    [Header("Upgrade")]
    public int maxSelectedIngredients = 2;
    public int maxTemperature = 120;

    [Header("Other control")]
    public Button upgradeButton;
    public Button recipeBookButton;

    //public List<PotionData> resultPotions = new List<PotionData>();
    public PotionData resultPotion;
    public PotionData failedPotion;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // UI visibility will be controlled by autoBrewUnlocked state
        SetAutoBrewUnlocked(autoBrewUnlocked);

        InitIngredientButtons();
        InitCatalystButton();
        InitCauldronSettings();
        OnToggleAutoBrew();

        // Ensure evaluated at start (in case selection/persistence exists)
        EvaluateAutoBrewAvailability();

        GameSaveManager.Instance.onLoadGame += UpdateSelectIngredientSlots;
        GameSaveManager.Instance.onNewGame += UpdateSelectIngredientSlots;
    }

    #region --- Initialization ---

    public void OnUpgrade(int item, int heat)
    {
        ResetCauldron();
        maxSelectedIngredients += item;
        maxTemperature += heat;

        for (int i = selectedIngredients.Count; i < maxSelectedIngredients; i++)
            selectedIngredients.Add(null);

        OpenBrewPanel();
    }
    public void UpdateSelectIngredientSlots()
    {
        for (int i = selectedIngredients.Count; i < maxSelectedIngredients; i++)
            selectedIngredients.Add(null);

        OpenBrewPanel();
    }

    public void OpenBrewPanel()
    {
        if(isOnBrewing) return;

        for (int i = 0; i < maxSelectedIngredients; i++)
        {
            var btn = selectedIngredientsButton[i].transform.Find("Icon").GetComponent<Image>();
            btn.sprite = defaultIngredientIcon;
            selectedIngredientsButton[i].interactable = true;
        }

        temperatureSlider.maxValue = maxTemperature;

        if(selectedIngredientsButton.Count > maxSelectedIngredients)
        {
            for (int i = maxSelectedIngredients; i < selectedIngredientsButton.Count; i++)
            {
                var btn = selectedIngredientsButton[i].transform.Find("Icon").GetComponent<Image>();
                btn.sprite = lockedIngredientIcon;
                selectedIngredientsButton[i].interactable = false;
            }
        }
        
    }

    private void InitIngredientButtons()
    {
        for (int i = 0; i < selectedIngredientsButton.Count; i++)
        {
            int iCopy = i;
            selectedIngredientsButton[i].onClick.AddListener(() =>
            {
                indexOfSelectButton = iCopy;
                currentTypeNeed = ItemType.Ingredient;
            });
        }

        // Ensure list length equals maxSelectedIngredients
        for (int i = selectedIngredients.Count; i < maxSelectedIngredients; i++)
            selectedIngredients.Add(null);

        // Trim if necessary
        if (selectedIngredients.Count > maxSelectedIngredients)
            selectedIngredients = selectedIngredients.Take(maxSelectedIngredients).ToList();
    }

    private void InitCatalystButton()
    {
        if (selectedCatalystButton == null) return;
        selectedCatalystButton.onClick.AddListener(() =>
        {
            indexOfSelectButton = -1;
            currentTypeNeed = ItemType.Catalyst;
        });
    }

    private void InitCauldronSettings()
    {
        if (temperatureSlider != null)
        {
            temperatureSlider.maxValue = maxTemperature;
            temperatureSlider.value = 0;
        }
        temperature = 0;
        if (temperatureText != null) temperatureText.text = $"{temperature} °C";
        if (stirSlider != null) stirSlider.value = 0;
        if (stirButton != null) stirButton.interactable = false;
        if (brewButton != null) brewButton.interactable = false;
    }

    #endregion

    #region --- Ingredient & Catalyst Management ---

    // Note: selecting ingredients/catalyst does not mutate inventory.
    // Inventory is consumed when brewing actually runs.
    public void AddIngredient(ItemData ingredient)
    {
        if (ingredient == null) return;


        if (ingredient.itemType == ItemType.Catalyst)
        {
            AddCatalyst(ingredient);
        }
        else
        {
            AddToIngredientSlot(ingredient);
        }

        CheckingToEnableStirButton();
        CheckingToEnableBrewButton();

        // Re-evaluate auto-brew availability whenever selection changes
        EvaluateAutoBrewAvailability();
    }

    private void AddCatalyst(ItemData catalyst)
    {
        if (catalyst == null || selectedCatalystButton == null) return;

        selectedCatalyst = catalyst;

        var btn = selectedCatalystButton.transform;
        var iconImg = btn.Find("Icon")?.GetComponent<Image>();
        if (iconImg != null) iconImg.sprite = catalyst.icon;
        var countText = selectedCatalystButton.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
        if (countText != null)
        {
            int available = inventoryManager != null ? inventoryManager.GetItemCount(catalyst) : 0;
            countText.text = available.ToString();
        }

        var removeBtn = btn.Find("Remove")?.GetComponent<Button>();
        if (removeBtn != null)
        {
            removeBtn.interactable = true;
            removeBtn.gameObject.SetActive(true);
            removeBtn.onClick.RemoveAllListeners();
            removeBtn.onClick.AddListener(() =>
            {
                RemoveCatalyst();
            });
        }

        // update displayed counts and auto-brew availability
        AmountSlideChange();
        UpdateIngredientsShow(Mathf.Clamp(Mathf.RoundToInt(amountSlider != null ? amountSlider.value : 1), 1, int.MaxValue));
        EvaluateAutoBrewAvailability();
    }

    private void RemoveCatalyst()
    {
        selectedCatalyst = null;
        if (selectedCatalystButton != null)
            ResetButton(selectedCatalystButton.transform);

        CheckingToEnableBrewButton();
        EvaluateAutoBrewAvailability();
    }

    private void AddToIngredientSlot(ItemData ingredient)
    {
        if (indexOfSelectButton < 0)
        {
            return;
        }

        if (ingredient == null) return;

        // If slot had previous item -> clear mapping (no inventory returned because none removed)
        var previous = selectedIngredients[indexOfSelectButton];
        if (previous != null)
        {
            ingredientButtonMap.Remove(previous);
        }

        selectedIngredients[indexOfSelectButton] = ingredient;
        var countText = selectedIngredientsButton[indexOfSelectButton].transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
        if (countText != null) countText.text = "1";

        // Update mapping (replace if exists)
        ingredientButtonMap[ingredient] = selectedIngredientsButton[indexOfSelectButton];

        UpdateIngredientSlotUI(indexOfSelectButton, ingredient);

        // update displayed counts (based on inventory) and auto-brew availability
        AmountSlideChange();
        UpdateIngredientsShow(Mathf.Clamp(Mathf.RoundToInt(amountSlider != null ? amountSlider.value : 1), 1, int.MaxValue));
        EvaluateAutoBrewAvailability();
    }

    private void UpdateIngredientSlotUI(int index, ItemData ingredient)
    {
        var btn = selectedIngredientsButton[index].transform;
        btn.Find("Icon").GetComponent<Image>().sprite = ingredient.icon;

        var removeBtn = btn.Find("Remove")?.GetComponent<Button>();
        if (removeBtn != null)
        {
            removeBtn.interactable = true;
            removeBtn.gameObject.SetActive(true);
            removeBtn.onClick.RemoveAllListeners();
            removeBtn.onClick.AddListener(() =>
            {
                RemoveIngredient(index);
            });
        }
    }

    public void DisableRemoveIngredients()
    {
        foreach (var btn in selectedIngredientsButton)
        {
            var removeBtn = btn.transform.Find("Remove")?.GetComponent<Button>();
            if (removeBtn != null)
            {
                removeBtn.interactable = false;
            }
        }
    }

    private void RemoveIngredient(int index)
    {
        var ingredient = selectedIngredients[index];
        if (ingredient == null) return;

        // simply clear selection — inventory was not changed on selection
        selectedIngredients[index] = null;

        ingredientButtonMap.Remove(ingredient);
        ResetButton(selectedIngredientsButton[index].transform);
        CheckingToEnableStirButton();
        CheckingToEnableBrewButton();

        // reevaluate auto-brew availability
        EvaluateAutoBrewAvailability();
    }

    public void ResetButton(Transform button)
    {
        if (button == null) return;
        var icon = button.Find("Icon")?.GetComponent<Image>();
        if (icon != null) icon.sprite = defaultIngredientIcon;
        var remove = button.Find("Remove")?.gameObject;
        if (remove != null) remove.SetActive(false);
        var count = button.Find("Count")?.GetComponent<TextMeshProUGUI>();
        if (count != null) count.text = "";
    }

    #endregion

    #region --- Temperature & Stirring ---

    public void SlideTemperature()
    {
        if (temperatureSlider == null) return;
        temperature = temperatureSlider.value;
        if (temperatureText != null) temperatureText.text = $"{temperature} °C";
    }

    public void UpdateTemperature(int tempIncrease)
    {
        temperature = Mathf.Clamp(temperature + tempIncrease, 0, maxTemperature);
        if (temperatureSlider != null) temperatureSlider.value = temperature;
        if (temperatureText != null) temperatureText.text = $"{temperature} °C";
    }

    public void Stir()
    {
        stirLevelInt++;

        stirLevel = stirLevelInt switch
        {
            1 => StirLevel.Low,
            2 => StirLevel.Medium,
            _ => StirLevel.High,
        };

        if (stirSlider != null) stirSlider.value = stirLevelInt;
    }

    public void CheckingToEnableStirButton()
    {
        if (stirButton == null) return;

        int count = 0;

        foreach (var item in selectedIngredients)
        {
            if (item != null && ++count >= 2)
            {
                stirButton.interactable = true;
                return;
            }
        }

        stirButton.interactable = false;
    }

    public void CheckingToEnableBrewButton()
    {
        if (brewButton == null) return;
        int ingredientCount = selectedIngredients.Count(i => i != null);

        if (ingredientCount >= 2)
        {
            if (autoBrewUnlocked && autoBrew != null && autoBrew.isOn)
            {
                if (amountSlider != null && amountSlider.value > 0)
                    brewButton.interactable = true;
                else
                    brewButton.interactable = false;
            }
            else
                brewButton.interactable = true;
        }
        else
            brewButton.interactable = false;
    }

    #endregion

    #region --- Brewing System ---

    public bool completeBrew = false;
    public bool isOnBrewing = false;

    public void StartBrew()
    {
        if (isOnBrewing) return; // prevent double press

        // Before starting a single brew, ensure enough inventory exists (1 batch)
        if (!CheckHasEnoughIngredients(1))
        {
            return;
        }
        if (selectedCatalyst != null && inventoryManager != null && !inventoryManager.HasItem(selectedCatalyst, 1))
        {
            return;
        }

        PlaySFX();

        isOnBrewing = true;
        completeBrew = false;
        if (brewButton != null) brewButton.interactable = false;
        DisableControlButton();


        if (autoBrew != null && autoBrew.isOn && autoBrewUnlocked)
        {
            AutoBrew(Mathf.RoundToInt(amountSlider.value));
        }
        else
        {
            StartCoroutine(ProcessBrew());
        }
    }

    private IEnumerator ProcessBrew()
    {
        // Re-check before actually consuming
        if (!CheckHasEnoughIngredients(1))
        {
            isOnBrewing = false;
            EnableControlButton();
            yield break;
        }
        if (selectedCatalyst != null && inventoryManager != null && !inventoryManager.HasItem(selectedCatalyst, 1))
        {
            isOnBrewing = false;
            EnableControlButton();
            yield break;
        }

        BrewResult result = CheckRecipe(out PotionRecipe validRecipe);

        float brewTime = (result == BrewResult.Success && validRecipe != null)
            ? validRecipe.timeToBrew
            : 3f; // default fail time

        // Fill once
        yield return StartCoroutine(FillBrewIcon(brewTime));

        // Consume ingredients and catalyst now (single batch)
        RemoveIngredients(); // removes 1 unit per selected slot
        if (selectedCatalyst != null && inventoryManager != null)
            inventoryManager.RemoveItem(selectedCatalyst, 1);

        // When brew completes:
        if (result == BrewResult.Success && validRecipe != null)
        {
            PotionData potion = ScriptableObject.Instantiate(validRecipe.potion);

            int stirLV = Mathf.Abs((int)stirLevel - (int)validRecipe.optimalStir);


            float purity = stirLevel == StirLevel.None
                ? UnityEngine.Random.Range(0.00f, 0.20f)    // Không khuấy → tệ
                : stirLV switch
                {
                    0 => UnityEngine.Random.Range(0.75f, 1.00f), // Chuẩn
                    1 => UnityEngine.Random.Range(0.50f, 0.74f), // Lệch nhẹ
                    _ => UnityEngine.Random.Range(0.20f, 0.49f), // Lệch nặng
                };

            potion.purity = purity;

            potion.strength = CatalystStrength(selectedCatalyst);
            potion.SetRarityBasedOnStats();
            potion.baseValue = Mathf.RoundToInt(GetValue(potion));

            resultPotion = potion;

            if (inventoryManager != null) inventoryManager.AddItem(resultPotion, 1);

            // increment recipe brewed count and re-evaluate auto-brew availability
            validRecipe.brewedCount++;
            EvaluateAutoBrewAvailability();

            var completePotionInfo = potion;
            ShowBrewingComplete(completePotionInfo);
            TutorialSignal.Emit("PotionDone");
        }
        else
        {
            if (inventoryManager != null && failedPotion != null)
            {
                PotionData failedPotion = ScriptableObject.Instantiate(this.failedPotion);

                failedPotion.purity = UnityEngine.Random.Range(0.1f, 0.9f);
                failedPotion.strength = CatalystStrength(selectedCatalyst);
                failedPotion.SetRarityBasedOnStats();
                failedPotion.baseValue = Mathf.RoundToInt(GetValue(failedPotion) * 0.3f);
                inventoryManager.AddItem(failedPotion, 1);

                ShowBrewingComplete(failedPotion);
            }
        }

        EnableControlButton();
        ResetCauldron();
        isOnBrewing = false;
        completeBrew = true;
        StopSFX();
        ResetSliderAmount();
    }

    // ------------------------------
    // ⚙️ AUTO BREW
    // ------------------------------

    public void OnToggleAutoBrew()
    {
        if (autoBrew == null || amountSlider == null || amountText == null) return;

        amountSlider.interactable = autoBrew.isOn && autoBrewUnlocked;
        if (!autoBrew.isOn)
            ResetSliderAmount();

        // ensure UI displays values correctly
        ResetSliderAmount();
    }

    public void ResetSliderAmount()
    {
        if (amountSlider == null) return;
        amountSlider.value = 1;
        if (amountText != null) amountText.text = amountSlider.value.ToString();
        UpdateIngredientsShow(Mathf.RoundToInt(amountSlider.value));
    }

    public void AmountSlideChange()
    {
        if (amountSlider == null) return;

        int value = Mathf.Clamp(Mathf.RoundToInt(amountSlider.value), 1, int.MaxValue);

        // Check if we can brew 'value' batches with current inventory
        if (CheckHasEnoughIngredients(value))
        {
            amountSlider.value = value;
            if (amountText != null) amountText.text = amountSlider.value.ToString();
            UpdateIngredientsShow(value);
        }
        else
        {
            int maxCan = GetMaxIngredientsCanBrew();
            amountSlider.value = Mathf.Max(1, maxCan);
            if (amountText != null) amountText.text = amountSlider.value.ToString();
            UpdateIngredientsShow(Mathf.RoundToInt(amountSlider.value));
        }

        CheckingToEnableBrewButton();
    }

    // Updated: supports duplicates in selectedIngredients (requires multiple units)
    private bool CheckHasEnoughIngredients(int count)
    {
        if (count <= 0) return false;
        if (inventoryManager == null) return false;

        // Count occurrences per distinct ingredient
        var grouped = selectedIngredients.Where(x => x != null).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        if (grouped.Count == 0) return false;

        foreach (var kv in grouped)
        {
            var item = kv.Key;
            int neededPerBatch = kv.Value;
            int neededTotal = neededPerBatch * count;
            if (!inventoryManager.HasItem(item, neededTotal))
                return false;
        }
        return true;
    }

    // Updated: consider duplicates when computing max batches
    public int GetMaxIngredientsCanBrew()
    {
        if (inventoryManager == null) return 0;

        var grouped = selectedIngredients.Where(x => x != null).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        if (grouped.Count == 0) return 0;

        int minBatches = int.MaxValue;
        foreach (var kv in grouped)
        {
            var item = kv.Key;
            int perBatch = kv.Value;
            int available = inventoryManager.GetItemCount(item);
            int batches = available / Math.Max(1, perBatch);
            if (batches < minBatches) minBatches = batches;
        }

        if (minBatches == int.MaxValue) return 0;
        return minBatches;
    }

    public void UpdateIngredientsShow(int count)
    {
        foreach (var ing in selectedIngredients)
        {
            if (ing == null) continue;
            ingredientButtonMap.TryGetValue(ing, out var btn);
            if (btn != null)
            {
                var t = btn.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
                if (t != null) t.text = count.ToString();
            }
        }

        if (selectedCatalyst != null && selectedCatalystButton != null && inventoryManager != null)
        {
            int maxCatalyst = inventoryManager.GetItemCount(selectedCatalyst);
            var t = selectedCatalystButton.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
            if (t != null) t.text = (count > maxCatalyst) ? maxCatalyst.ToString() : count.ToString();
        }
    }


    public void DisableAutoBrewControl()
    {
        if (autoBrew != null) autoBrew.interactable = false;
        if (amountSlider != null) amountSlider.interactable = false;
    }
    public void EnableAutoBrewControl()
    {
        if (autoBrew != null) autoBrew.interactable = true;
        if (amountSlider != null) amountSlider.interactable = true;
    }

    public void AutoBrew(int amount)
    {
        if (amount <= 0) return;
        DisableAutoBrewControl();
        StartCoroutine(AutoBrewRoutine(amount));
    }

    private IEnumerator AutoBrewRoutine(int amount)
    {
        int currentAmount = amount;

        for (int i = 0; i < amount; i++)
        {
            UpdateIngredientsShow(currentAmount);
            currentAmount--;

            // Verify enough ingredients and catalyst for this batch (accounting duplicates)
            if (!CheckHasEnoughIngredients(1))
            {
            
                break;
            }
            if (selectedCatalyst != null && inventoryManager != null)
            {
                if (!inventoryManager.HasItem(selectedCatalyst, 1))
                {
                    RemoveCatalyst();
                }
            }

     
            BrewResult result = CheckRecipe(out PotionRecipe validRecipe);
            float brewTime = (result == BrewResult.Success && validRecipe != null) ? validRecipe.timeToBrew : 3f;

            yield return StartCoroutine(FillBrewIcon(brewTime));

            // Consume ingredients and catalyst now for this batch
            RemoveIngredients(); // removes 1 per selected slot
            if (selectedCatalyst != null && inventoryManager != null)
                inventoryManager.RemoveItem(selectedCatalyst, 1);

            if (result == BrewResult.Success && validRecipe != null)
            {
                PotionData potion = ScriptableObject.Instantiate(validRecipe.potion);
                int stirLV = Mathf.Abs((int)stirLevel - (int)validRecipe.optimalStir);
            

                float purity = stirLevel == StirLevel.None
                    ? UnityEngine.Random.Range(0.00f, 0.20f)    // Không khuấy → tệ
                    : stirLV switch
                    {
                        0 => UnityEngine.Random.Range(0.75f, 1.00f), // Chuẩn
                        1 => UnityEngine.Random.Range(0.50f, 0.74f), // Lệch nhẹ
                        _ => UnityEngine.Random.Range(0.20f, 0.49f), // Lệch nặng
                    };

                potion.purity = purity;
                potion.strength = CatalystStrength(selectedCatalyst);
                potion.SetRarityBasedOnStats();
                potion.baseValue = Mathf.RoundToInt(GetValue(potion));

                resultPotion = potion;

                if (inventoryManager != null) inventoryManager.AddItem(resultPotion, 1);

                // increment recipe brewed count and re-evaluate auto-brew availability
                validRecipe.brewedCount++;
                EvaluateAutoBrewAvailability();

            }
            else
            {
                if (inventoryManager != null && failedPotion != null)
                {
                    PotionData failedPotion = ScriptableObject.Instantiate(this.failedPotion);
                    failedPotion.purity = UnityEngine.Random.Range(0.1f, 0.9f);
                    failedPotion.strength = CatalystStrength(selectedCatalyst);
                    failedPotion.SetRarityBasedOnStats();
                    failedPotion.baseValue = Mathf.RoundToInt(GetValue(failedPotion) * 0.3f);
                    inventoryManager.AddItem(failedPotion, 1);
                }
            }

            // Small delay between batches
            yield return new WaitForSeconds(0.5f);
        }

        isOnBrewing = false;
        completeBrew = true;
        ResetCauldron();
        EnableControlButton();
        EnableAutoBrewControl();
        StopSFX();
        ResetSliderAmount();
    }

    private IEnumerator FillBrewIcon(float duration)
    {
        float timer = 0f;
        if (brewIcon == null) yield break;
        brewIcon.fillAmount = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            brewIcon.fillAmount = Mathf.Clamp01(timer / duration);
            yield return null;
        }

        brewIcon.fillAmount = 1f;
    }

    public void DisableControlButton()
    {
        foreach (var btn in selectedIngredientsButton)
        {
            if (btn != null) btn.interactable = false;
        }
        if (selectedCatalystButton != null) selectedCatalystButton.interactable = false;
        if (stirButton != null) stirButton.interactable = false;
        if (temperaturePlusButton != null) temperaturePlusButton.interactable = false;
        if (temperatureMinusButton != null) temperatureMinusButton.interactable = false;

        selectedCatalystButton.transform.Find("Remove").GetComponent<Button>().interactable = false;
        DisableRemoveIngredients();

        upgradeButton.interactable = false ;
        recipeBookButton.interactable = false ;
    }

    public void EnableControlButton()
    {
        foreach (var btn in selectedIngredientsButton)
        {
            if (btn != null) btn.interactable = true;
        }
        if (selectedCatalystButton != null) selectedCatalystButton.interactable = true;
        if (stirButton != null) stirButton.interactable = true;
        if (temperaturePlusButton != null) temperaturePlusButton.interactable = true;
        if (temperatureMinusButton != null) temperatureMinusButton.interactable = true;

        upgradeButton.interactable = true;
        recipeBookButton.interactable = true;
    }

    #endregion

    #region --- Recipe matching / Auto-brew unlock logic ---

    /// <summary>
    /// Find a recipe that matches currently selected ingredients (multiset match).
    /// Ignores temperature/stir — only matches ingredient list and counts.
    /// </summary>
    private PotionRecipe FindMatchingRecipeByIngredients()
    {
        var provided = selectedIngredients.Where(x => x != null).ToList();
        if (provided.Count == 0) return null;

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;
            var required = recipe.requiredIngredients ?? new List<ItemData>();
            if (required.Count != provided.Count) continue;
            if (IngredientsMatchMultiset(required, provided))
                return recipe;
        }
        return null;
    }

    /// <summary>
    /// Evaluate whether auto-brew UI should be unlocked/shown based on selected ingredients.
    /// Condition: there is a matching recipe AND its brewedCount >= autoUnlockThreshold.
    /// </summary>
    public void EvaluateAutoBrewAvailability()
    {
        var match = FindMatchingRecipeByIngredients();
        bool shouldUnlock = match != null && match.brewedCount >= Mathf.Max(1, match.autoUnlockThreshold);

        SetAutoBrewUnlocked(shouldUnlock);
    }

    /// <summary>
    /// Centralizes enabling/disabling the auto-brew UI and state.
    /// </summary>
    private void SetAutoBrewUnlocked(bool unlocked)
    {
        autoBrewUnlocked = unlocked;

        if (autoBrew != null)
            autoBrew.gameObject.SetActive(unlocked);

        if (amountSlider != null)
            amountSlider.gameObject.SetActive(unlocked);

        if (amountText != null)
            amountText.gameObject.SetActive(unlocked);

        // If unlocking, ensure toggle state is off by default
        if (!unlocked)
        {
            if (autoBrew != null) autoBrew.isOn = false;
            if (amountSlider != null) amountSlider.value = 1;
            if (amountText != null) amountText.text = "1";
        }
    }

    #endregion

    private BrewResult CheckRecipe(out PotionRecipe validRecipe)
    {
        // Build list of provided ingredients (exclude nulls)
        var provided = selectedIngredients.Where(x => x != null).ToList();

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;

            var required = recipe.requiredIngredients ?? new List<ItemData>();

            if (required.Count != provided.Count)
                continue;

            if (!IngredientsMatchMultiset(required, provided))
                continue;

            // Temperature check
            if (temperature > recipe.maxTemp || temperature < recipe.minTemp)
            {
                validRecipe = recipe;
                return BrewResult.Failed;
            }

            validRecipe = recipe;
            return BrewResult.Success;
        }

        validRecipe = null;
        return BrewResult.Failed;
    }

    private bool IngredientsMatchMultiset(List<ItemData> required, List<ItemData> provided)
    {
        if (required == null || provided == null) return false;
        var dict = new Dictionary<ItemData, int>();
        foreach (var r in required)
        {
            if (r == null) return false;
            if (!dict.ContainsKey(r)) dict[r] = 0;
            dict[r]++;
        }
        foreach (var p in provided)
        {
            if (p == null) return false;
            if (!dict.ContainsKey(p)) return false;
            dict[p]--;
            if (dict[p] < 0) return false;
        }
        // All counts zero?
        return dict.Values.All(v => v == 0);
    }

    public void ShowBrewingComplete(PotionData potion)
    {
        if (!brewingPanel.isVisible) return;
        brewingPanel.Hide();
        potionInfoPenel.Show();

        var panel = potionInfoPenel.transform;

        var icon = panel.Find("Icon").GetComponent<Image>();
        var nameText = panel.Find("Name").GetComponent<TextMeshProUGUI>();
        var purityText = panel.Find("Purity").GetComponent<TextMeshProUGUI>();
        var strengthText = panel.Find("Strength").GetComponent<TextMeshProUGUI>();
        var valueText = panel.Find("Value").GetComponent<TextMeshProUGUI>();
        var rarityText = panel.Find("Rarity").GetComponent<TextMeshProUGUI>();
        var elementMixText = panel.Find("ElementMix").GetComponent<TextMeshProUGUI>();
        var descText = panel.Find("Description").GetComponent<TextMeshProUGUI>();

        icon.sprite = potion.icon;
        nameText.text = potion.itemName;
        purityText.text = $"Purity: {potion.purity:F2}";
        strengthText.text = $"Strength: {potion.strength:F2}";
        valueText.text = $"Value: <color=#FFFF00>{potion.baseValue}</color>";
        elementMixText.text = "Element Mix: " + string.Join(", ", potion.elementMixs);
        descText.text = potion.description;

        ColoredRarityText(rarityText, potion.itemRarity);
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

        textComponent.text = $"<color={colorHex}>{rarity}</color>";
    }

    private void RemoveIngredients()
    {
        if (inventoryManager == null) return;
        foreach (var ing in selectedIngredients)
        {
            if (ing == null) continue;
            // Remove one unit per selected slot
            inventoryManager.RemoveItem(ing, 1);
        }
    }


    public float GetValue(PotionData potion)
    {
        float baseValue = 0f;

        foreach (var item in selectedIngredients)
        {
            if (item == null) continue;
            baseValue += item.baseValue;
        }

        float strengthValue = 0f;
        strengthValue += (baseValue * potion.strength);
        float purityValue = 0f;
        purityValue += (baseValue * potion.purity);

        float total = baseValue + strengthValue + purityValue;

        float multiplier = potion.itemRarity switch
        {
            ItemRarity.Uncommon => 1.15f,
            ItemRarity.Rare => 1.30f,
            ItemRarity.Epic => 1.45f,
            ItemRarity.Legendary => 1.75f,
            _ => 1f
        };

        total *= multiplier;

        return total;
    }

    private float CatalystStrength(ItemData catalyst)
    {
        if (catalyst == null) return UnityEngine.Random.Range(0f, 0.3f);

        return catalyst.itemRarity switch
        {
            ItemRarity.Common => UnityEngine.Random.Range(0.3f, 0.5f),
            ItemRarity.Uncommon => UnityEngine.Random.Range(0.51f, 0.6f),
            ItemRarity.Rare => UnityEngine.Random.Range(0.61f, 0.7f),
            ItemRarity.Epic => UnityEngine.Random.Range(0.71f, 0.8f),
            ItemRarity.Legendary => UnityEngine.Random.Range(0.81f, 1f),
            _ => UnityEngine.Random.Range(0f, 0.3f)
        };
    }

    #region --- Reset & Menu Control ---

    public void OpenCauldronMenu()
    {
        // Reset temporary state when opening
        ResetCauldron();
        gameObject.SetActive(true);
    }

    public void CloseCauldronMenu()
    {
        if (isOnBrewing) return;
        ResetCauldron();
    }

    public void ResetCauldron()
    {
        // Clear data
        for (int i = 0; i < selectedIngredients.Count; i++)
            selectedIngredients[i] = null;
        selectedCatalyst = null;
        stirLevelInt = 0;
        stirLevel = StirLevel.Low;
        temperature = 0;

        // Reset UI buttons
        for (int i = 0; i < maxSelectedIngredients && i < selectedIngredientsButton.Count; i++)
        {
            ResetButton(selectedIngredientsButton[i].transform);
        }

        if (selectedCatalystButton != null)
            ResetButton(selectedCatalystButton.transform);

        // Clear maps
        ingredientButtonMap.Clear();

        resultPotion = null;

        if (temperatureSlider != null) temperatureSlider.value = 0;
        if (temperatureText != null) temperatureText.text = "0 °C";
        if (stirSlider != null) stirSlider.value = 0;
        if (stirButton != null) stirButton.interactable = false;
        if (brewButton != null) brewButton.interactable = false;

        if (brewIcon != null) brewIcon.fillAmount = 0f;
        completeBrew = false;

        // After reset, auto-brew availability should be re-evaluated (likely locked)
        EvaluateAutoBrewAvailability();
    }

    #endregion

    public void PlaySFX()
    {
        if (brewSFXSource == null || brewSFX == null) return;
        brewSFXSource.clip = brewSFX;
        brewSFXSource.Play();
    }
    public void StopSFX()
    {
        if (brewSFXSource == null) return;
        brewSFXSource.Stop();
    }
}