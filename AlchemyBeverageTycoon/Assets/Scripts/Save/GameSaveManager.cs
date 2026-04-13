using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;
    public InventoryManager inventoryManager;
    public PotionData potionStart;
    public TextMeshProUGUI saveNoityfication;
    public bool isTest;


    public Action onNewGame;
    public Action onLoadGame;

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        // auto save on period change
        if (TimeCycleManager.Instance != null)
            TimeCycleManager.Instance.OnPeriodChanged += OnPeriodChanged;

        inventoryManager = InventoryManagerInstance();
    }

    public bool CheckHasFileLoad()
    {
        if (File.Exists(SavePath))
        {
            return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        if (TimeCycleManager.Instance != null)
            TimeCycleManager.Instance.OnPeriodChanged -= OnPeriodChanged;
    }

    private void OnPeriodChanged(TimePeriod period)
    {
        SaveGame();
    }

    [ContextMenu("DeleteFileSave")]
    public void DeleteFileSave()
    {
        try
        {
            File.Delete(SavePath);
        }
        catch (Exception)
        {
        }
    }

    public void NewGame()
    {
        DeleteFileSave();

        if (isTest)
        {
            inventoryManager.ClearInventory();
            inventoryManager.maxCapacity = 10000;
            inventoryManager.LoadTest();
            inventoryManager.AddItem(potionStart);
        }
        else
        {
            inventoryManager.ClearInventory();
            inventoryManager.maxCapacity = 100;
            inventoryManager.AddItem(potionStart);
            ItemData baseItem1 = Resources.Load<ItemData>($"Items/Dewdrop");
            if (baseItem1 != null)
                inventoryManager.AddItem(baseItem1, 2);
            ItemData baseItem2 = Resources.Load<ItemData>($"Items/Herb Leaf");
            if (baseItem1 != null)
                inventoryManager.AddItem(baseItem2, 2);
            ItemData baseItem3 = Resources.Load<ItemData>($"Items/Sun Berry");
            if (baseItem1 != null)
                inventoryManager.AddItem(baseItem3, 1);

        }

        var allTeams = Resources.FindObjectsOfTypeAll<ExpeditionMilitary>();
        foreach (var team in allTeams)
        {
            team.isUnlocked = false;
            team.isInExpedition = false;
            team.troopCount = 0;
        }
        var dataAsset = Resources.LoadAll<ExpeditionData>($"Expeditions/");

        foreach (var data in dataAsset)
        {
            data.isOnExpedition = false;
        }

        foreach (var rp in Cauldron.Instance.recipes)
        {
            rp.brewedCount = 0;
        }

        Cauldron.Instance.maxSelectedIngredients = 2;

        foreach (var data in UpgradeManager.Instance.database.upgrades)
        {
            data.purchased = false;
        }
        ResearchProgress.Instance.SetLevel(0);

        GoldManager.Instance.StartGameGold();
        ReputationManager.Instance.StartGameReputation();

        UpgradeManager.Instance.NewGame();

        ExpeditionUI.Instance.ClearMarkersIfNoExpedition();

        onNewGame.Invoke();
    }

    // PUBLIC API
    public void SaveGame()
    {
        try
        {
            GameSaveData data = CollectGameState();
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            saveNoityfication.text = "Game Saved";
            saveNoityfication.gameObject.SetActive(true);
            saveNoityfication.GetComponent<UIAnimatorTool>().Show();
            StopAllCoroutines();
            StartCoroutine(ResetText());

        }
        catch (Exception )
        {
        }
    }

    public IEnumerator ResetText()
    {
        yield return new WaitForSeconds(3);
        saveNoityfication.gameObject.SetActive(false);
    }

    public void LoadGame()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                return;
            }

            string json = File.ReadAllText(SavePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            RestoreGameState(data);
            onLoadGame.Invoke();
        }
        catch (Exception)
        {
        }
    }

    // -----------------------
    // Collect / Restore Logic
    // -----------------------
    private GameSaveData CollectGameState()
    {
        var gs = new GameSaveData();

        // Time
        if (TimeCycleManager.Instance != null)
        {
            gs.dayCount = TimeCycleManager.Instance.DayCount;
            gs.currentPeriod = TimeCycleManager.Instance.CurrentPeriod;
        }

        // Gold
        if (GoldManager.Instance != null)
        {
            gs.gold = GoldManager.Instance.GetCurrentGold();
        }

        // Reputation
        if (ReputationManager.Instance != null)
        {
            gs.reputation = ReputationManager.Instance.GetRepulation();
            gs.playerRank = ReputationManager.Instance.currentRank;
        }

        // Research progress
        if (ResearchProgress.Instance != null)
        {
            gs.researchLevelIndex = ResearchProgress.Instance.CurrentLevelIndex;
        }

        // Unlocked recipes
        if (RecipeUnlockManager.Instance != null)
        {
            gs.unlockedRecipeNames = RecipeUnlockManager.Instance.GetUnlockedNames();
        }

        gs.maxIngrdients = Cauldron.Instance.maxSelectedIngredients;
        gs.maxTemperature = Cauldron.Instance.maxTemperature;

        //Upgrade
        UpgradeManager.Instance.SaveUpgrades();


        inventoryManager.Save();
        gs.inventoryMaxCapacity = InventoryManagerInstance().maxCapacity;


        // Recipes brewed counts (persist brewedCount per recipe's potion name)
        if (Cauldron.Instance != null && Cauldron.Instance.recipes != null)
        {
            gs.recipes = Cauldron.Instance.recipes
                .Where(r => r != null && r.potion != null)
                .Select(r => new RecipeSave { potionID = r.potion.itemName, brewedCount = r.brewedCount, threshold = r.autoUnlockThreshold })
                .ToList();
        }

        // Expedition teams global state: unlocked + troop counts
        {
            var allTeams = Resources.FindObjectsOfTypeAll<ExpeditionMilitary>();
            gs.expeditionTeams = new List<ExpeditionTeamSave>();
            foreach (var team in allTeams)
            {
                if (team == null) continue;
                gs.expeditionTeams.Add(new ExpeditionTeamSave
                {
                    name = team.expeditionMilitaryName,
                    isUnlocked = team.isUnlocked,
                    troopCount = team.troopCount
                });
            }
        }

        // Active expeditions - store data reference by expeditionTitle and other runtime fields
        if (ExpeditionManager.Instance != null)
        {
            // Save activeExpeditions list as-is
            foreach (var exp in ExpeditionManager.Instance.activeExpeditions)
            {
                if (exp == null || exp.data == null) continue;
                var es = new ExpeditionSave
                {
                    expeditionID = exp.data.expeditionTitle,
                    remainingPeriods = exp.remainingPeriods,
                    isActive = exp.isActive,
                    isComplete = exp.isComplete,
                    currentSuccessRate = exp.data.currentSuccessRate,
                    expeditionMilitaryName = exp.data.expeditionMilitary != null ? exp.data.expeditionMilitary.expeditionMilitaryName : null,
                    isOnExpedition = exp.data.isOnExpedition,
                    completeInfo = exp.compeleteExpeditionInfo,
                };

                // carried potions
                es.carriedPotions = new List<SlotSave>();
                if (exp.carriedItems != null)
                {
                    foreach (var pot in exp.carriedItems)
                    {
                        if (pot == null) continue;
                        es.carriedPotions.Add(new SlotSave
                        {
                            itemID = pot.itemName,
                            type = ItemType.Potion,
                            quantity = 1,
                            potion = new PotionSave
                            {
                                name = pot.itemName,
                                description = pot.description,
                                purity = pot.purity,
                                strength = pot.strength,
                                rarity = pot.itemRarity,
                                baseValue = pot.baseValue,
                                explorationBonus = pot.explorationBonus,
                                elementMixs = new List<ElementType>(pot.elementMixs ?? new List<ElementType>()),
                                favoredRegionElement = new List<ElementType>(pot.favoredRegionElement ?? new List<ElementType>())
                            }
                        });
                    }
                }

                gs.expeditions.Add(es);
            }
        }

        return gs;
    }

    private void RestoreGameState(GameSaveData gs)
    {
        if (gs == null) return;

        Cauldron.Instance.maxSelectedIngredients = gs.maxIngrdients;
        Cauldron.Instance.maxTemperature = gs.maxTemperature;

        UpgradeManager.Instance.LoadUpgrades();

        // Gold
        if (GoldManager.Instance != null)
        {
            int current = GoldManager.Instance.GetCurrentGold();
            int diff = gs.gold - current;
            if (diff > 0) GoldManager.Instance.AddGold(diff);
            else if (diff < 0)
            {
                while (GoldManager.Instance.SpendGold(1) && GoldManager.Instance.GetCurrentGold() > gs.gold) ;
            }
        }

        // Reputation
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.SetReputation(gs.reputation);
            var rankField = ReputationManager.Instance.GetType().GetField("currentRank", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (rankField != null) rankField.SetValue(ReputationManager.Instance, gs.playerRank);

        }

        // Research progress
        if (ResearchProgress.Instance != null)
        {
            ResearchProgress.Instance.SetLevel(gs.researchLevelIndex);
        }

        // Recipe unlocks
        if (RecipeUnlockManager.Instance != null)
        {
            RecipeUnlockManager.Instance.SetUnlockedFromNames(gs.unlockedRecipeNames);
        }


        inventoryManager.Load();
        if (gs.inventoryMaxCapacity > 0)
            inventoryManager.maxCapacity = gs.inventoryMaxCapacity;

        // Recipes: write brewedCount back to matching recipe entries
        if (Cauldron.Instance != null && gs.recipes != null)
        {
            foreach (var rs in gs.recipes)
            {
                var recipe = Cauldron.Instance.recipes?.Find(r => r != null && r.potion != null && r.potion.itemName == rs.potionID);
                if (recipe != null) recipe.brewedCount = rs.brewedCount;
            }
        }

        // Expedition teams: restore unlocked and troop counts (global)
        if (gs.expeditionTeams != null && gs.expeditionTeams.Count > 0)
        {
            var allTeams = Resources.FindObjectsOfTypeAll<ExpeditionMilitary>();
            foreach (var teamSave in gs.expeditionTeams)
            {
                if (string.IsNullOrEmpty(teamSave.name)) continue;
                var team = allTeams.FirstOrDefault(t => t != null && t.expeditionMilitaryName == teamSave.name);
                if (team != null)
                {
                    team.isUnlocked = teamSave.isUnlocked;
                    team.troopCount = teamSave.troopCount;
                }
            }
        }

        // Expeditions: restore active expeditions and their runtime state
        if (ExpeditionManager.Instance != null)
        {
            if (ExpeditionManager.Instance.activeExpeditions != null)
                ExpeditionManager.Instance.activeExpeditions.Clear();

            foreach (var es in gs.expeditions)
            {
                if (es == null) continue;

                ExpeditionData dataAsset = null;

                try
                {
                    dataAsset = Resources.Load<ExpeditionData>($"Expeditions/{es.expeditionID}");
                }
                catch { dataAsset = null; }

                if (dataAsset == null)
                {
                    var all = Resources.FindObjectsOfTypeAll<ExpeditionData>();
                    dataAsset = all.FirstOrDefault(d => d != null && d.expeditionTitle == es.expeditionID);
                }

                if (dataAsset == null)
                { 
                    continue;
                }

                var carried = new List<PotionData>();
                if (es.carriedPotions != null)
                {
                    foreach (var ps in es.carriedPotions)
                    {
                        if (ps == null) continue;
                        PotionData basePotion = Resources.Load<PotionData>($"Potions/{ps.itemID}");
                        PotionData newPotion = ScriptableObject.CreateInstance<PotionData>();
                        newPotion.itemName = ps.potion.name;
                        newPotion.description = ps.potion.description;
                        newPotion.itemType = ItemType.Potion;
                        newPotion.itemRarity = ps.potion.rarity;
                        newPotion.purity = ps.potion.purity;
                        newPotion.strength = ps.potion.strength;
                        newPotion.baseValue = ps.potion.baseValue;
                        newPotion.explorationBonus = ps.potion.explorationBonus;
                        newPotion.elementMixs = ps.potion.elementMixs ?? new List<ElementType>();
                        newPotion.favoredRegionElement = ps.potion.favoredRegionElement ?? new List<ElementType>();
                        if (basePotion != null) newPotion.icon = basePotion.icon;
                        carried.Add(newPotion);
                    }
                }

                ExpeditionManager.Instance.StartExpedition(dataAsset, carried);

                dataAsset.isOnExpedition = es.isOnExpedition;
                if (!string.IsNullOrEmpty(es.expeditionMilitaryName) && dataAsset.expeditionMilitary != null)
                {
                    if (dataAsset.expeditionMilitary.expeditionMilitaryName == es.expeditionMilitaryName)
                        dataAsset.expeditionMilitary.isInExpedition = true;
                    else
                    {
                        var allMil = Resources.FindObjectsOfTypeAll<ExpeditionMilitary>();
                        var mil = allMil.FirstOrDefault(m => m != null && m.expeditionMilitaryName == es.expeditionMilitaryName);
                        if (mil != null) mil.isInExpedition = true;
                    }
                }

                var instance = ExpeditionManager.Instance.GetExpeditionbyExpeditionData(dataAsset);
                if (instance != null)
                {
                    try
                    {
                        instance.compeleteExpeditionInfo = es.completeInfo;
                    }
                    catch { }

                    try
                    {
                        instance.remainingPeriods = es.remainingPeriods;
                        instance.isActive = es.isActive;
                        instance.isComplete = es.isComplete;
                    }
                    catch { }

                    if (es.carriedPotions != null && es.carriedPotions.Count > 0)
                    {
                        instance.carriedItems = carried;
                    }

                    try
                    {
                        if (es.currentSuccessRate > 0f)
                            instance.data.currentSuccessRate = es.currentSuccessRate;
                    }
                    catch { }
                }
            }
        }
        // Time
        if (TimeCycleManager.Instance != null)
        {
            var dayCountProp = TimeCycleManager.Instance.GetType().GetProperty("DayCount");
            if (dayCountProp != null) dayCountProp.SetValue(TimeCycleManager.Instance, gs.dayCount);
            TimeCycleManager.Instance.SetPeriod(gs.currentPeriod);
            TimeCycleManager.Instance.timer = 0;
        }

    }

    // Helpers
    private InventoryManager InventoryManagerInstance()
    {
        if (InventoryManagerInstanceCache == null)
            InventoryManagerInstanceCache = Resources.FindObjectsOfTypeAll<InventoryManager>().FirstOrDefault();
        return InventoryManagerInstanceCache;
    }
    private InventoryManager InventoryManagerInstanceCache;

    // -----------------------
    // Save Data Containers
    // -----------------------
    [Serializable]
    private class GameSaveData
    {
        // Time
        public int dayCount;
        public TimePeriod currentPeriod;

        // economy
        public int gold;
        public int reputation;
        public Rank playerRank;

        // research
        public int researchLevelIndex;
        public List<string> unlockedRecipeNames = new List<string>();

        // inventory
        //  public InventorySave inventory = new InventorySave();
        public int inventoryMaxCapacity;

        // recipes
        public List<RecipeSave> recipes = new List<RecipeSave>();

        // expedition teams global
        public List<ExpeditionTeamSave> expeditionTeams = new List<ExpeditionTeamSave>();

        // expeditions
        public List<ExpeditionSave> expeditions = new List<ExpeditionSave>();

        //Breing Upgrade
        public int maxIngrdients;
        public int maxTemperature;
    }

    [Serializable]
    private class ExpeditionTeamSave
    {
        public string name;
        public bool isUnlocked;
        public int troopCount;
    }

    [Serializable]
    private class InventorySave
    {
        public List<SlotSave> items = new List<SlotSave>();
    }

    [Serializable]
    private class SlotSave
    {
        public string itemID;
        public ItemType type;
        public int quantity;
        public PotionSave potion;
    }

    [Serializable]
    private class PotionSave
    {
        public string name;
        public string description;
        public float purity;
        public float strength;
        public ItemRarity rarity;
        public int baseValue;
        public float explorationBonus;
        public List<ElementType> elementMixs;
        public List<ElementType> favoredRegionElement;
    }

    [Serializable]
    private class RecipeSave
    {
        public string potionID;
        public int brewedCount;
        public int threshold;
    }

    [Serializable]
    private class ExpeditionSave
    {
        public string expeditionID;
        public int remainingPeriods;
        public bool isActive;
        public bool isComplete;
        public List<SlotSave> carriedPotions;
        public CompleteExpeditionInfo completeInfo;

        // added runtime fields
        public float currentSuccessRate;
        public string expeditionMilitaryName;
        public bool isOnExpedition;
    }
}