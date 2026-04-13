using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
[System.Serializable]
public class ExpeditionInstance
{
    public InventoryManager inventoryManager;
    public ExpeditionData data;
    public int remainingPeriods;
    public bool isActive;
    public List<PotionData> carriedItems;
    public List<ItemData> itemCollected = new List<ItemData>();
    public bool isComplete;
    public CompleteExpeditionInfo compeleteExpeditionInfo = new CompleteExpeditionInfo();


    public ExpeditionInstance(InventoryManager inventoryManager,ExpeditionData data, List<PotionData> carriedItems)
    {
        this.inventoryManager = inventoryManager;
        this.data = data;
        this.carriedItems = carriedItems;
        remainingPeriods = data.durationInPeriods;
        isActive = true;
    }

    public void ProgressOnePeriod()
    {
        if (!isActive) return;

        remainingPeriods--;
        if (remainingPeriods <= 0)
        {
            SimulateBattle();
            CompleteExpedition();
        }
    }

    public bool isSuccessful;
    private void SimulateBattle()
    {
      
        ExpeditionMilitary military = data.expeditionMilitary;
        int currentTroops = military.troopCount;

        
        float roll = UnityEngine.Random.Range(0f, 100f);
        bool isSuccessful = roll <= data.currentSuccessRate;
        this.isSuccessful = isSuccessful;


        float lossRate;
        if (isSuccessful)
        {
            // Nếu thắng → tổn thất nhẹ (5–15%)
            lossRate = UnityEngine.Random.Range(0.15f, 0.20f);
        }
        else
        {
            // Nếu thất bại → tổn thất nặng (40–90%)
            lossRate = UnityEngine.Random.Range(0.4f, 0.9f);
        }

        int troopsLost = Mathf.RoundToInt(currentTroops * lossRate);
        military.troopCount = Mathf.Max(0, currentTroops - troopsLost);

        // --- 4. Ghi log kết quả ---

        compeleteExpeditionInfo.result = isSuccessful;
        compeleteExpeditionInfo.troopsLost = troopsLost;
        compeleteExpeditionInfo.data = data;
        compeleteExpeditionInfo.curentTroops = currentTroops;
    }

    private void CompleteExpedition()
    {
        isActive = false;
        if (isSuccessful)
        {
            var loots = GetRandomLoots(data.possibleLoots, UnityEngine.Random.Range(Mathf.RoundToInt(data.maxLootItems * 0.8f), data.maxLootItems));
            List<ItemData> lootItem = new List<ItemData>(loots);
            compeleteExpeditionInfo.collectedItems = lootItem;
            itemCollected = loots;
        }

        isComplete = true;
    }

    public CompleteExpeditionInfo ClaimRewards()
    {
        if (!isComplete) return null;
        foreach (var item in compeleteExpeditionInfo.collectedItems)
        {
            inventoryManager.AddItem(item, 1);
        }
        itemCollected.Clear();
        isComplete = false;
        data.expeditionMilitary.isInExpedition = false;
        data.isOnExpedition = false;
        return compeleteExpeditionInfo;
    }

    // ---------------------- HÀM RANDOM LOOT ----------------------
    private List<ItemData> GetRandomLoots(List<ItemData> possibleLoots, int count)
    {
        List<ItemData> results = new List<ItemData>();
        if (possibleLoots == null || possibleLoots.Count == 0) return results;

        // Tách loot theo loại
        List<ItemData> catalystList = possibleLoots.FindAll(i => i.itemType == ItemType.Catalyst);
        List<ItemData> ingredientList = possibleLoots.FindAll(i => i.itemType == ItemType.Ingredient);

        // Trọng số độ hiếm
        Dictionary<ItemRarity, float> rarityWeights = new Dictionary<ItemRarity, float>
    {
        { ItemRarity.Common, 50f },
        { ItemRarity.Uncommon, 20f },
        { ItemRarity.Rare, 15f },
        { ItemRarity.Epic, 10f },
        { ItemRarity.Legendary, 5f }
    };

        for (int i = 0; i < count; i++)
        {
            // 20% Catalyst | 80% Ingredient
            bool chooseCatalyst = UnityEngine.Random.value <= 0.15f;

            List<ItemData> targetList = null;

            if (chooseCatalyst && catalystList.Count > 0)
            {
                targetList = catalystList;
            }
            else if (ingredientList.Count > 0)
            {
                targetList = ingredientList;
            }
            else if (catalystList.Count > 0)
            {
                targetList = catalystList;   // fallback
            }
            else
            {
                break; // Không có item nào
            }

            // Tính tổng trọng số trong nhóm được chọn
            float totalWeight = 0f;
            foreach (var item in targetList)
                totalWeight += rarityWeights[item.itemRarity];

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var item in targetList)
            {
                cumulative += rarityWeights[item.itemRarity];
                if (randomValue <= cumulative)
                {
                    results.Add(item);
                    break;
                }
            }
        }

        return results;
    }

}
[Serializable]
public class CompleteExpeditionInfo
{
    public bool result;
    public ExpeditionData data;
    public int curentTroops;
    public int troopsLost;
    public List<ItemData> collectedItems = new List<ItemData>();
}