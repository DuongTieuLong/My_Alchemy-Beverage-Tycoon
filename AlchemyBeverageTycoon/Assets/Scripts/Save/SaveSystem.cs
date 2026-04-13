using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "inventory.json");

    [System.Serializable]
    private class InventoryData
    {
        public List<SlotSaveData> items = new();
    }

    [System.Serializable]
    private class SlotSaveData
    {
        public string itemID;
        public ItemType type;
        public int quantity;
        public PotionSaveData potion;
    }

    [System.Serializable]
    public class PotionSaveData
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

    public static void SaveInventory(InventoryManager inventory)
    {
        InventoryData data = new InventoryData();

        foreach (var slot in inventory.GetAllItems())
        {
            if (slot.item == null) continue;

            if (slot.item.itemType == ItemType.Potion)
            {
                PotionData potion = slot.item as PotionData;
                if (potion == null) continue;

                data.items.Add(new SlotSaveData
                {
                    itemID = potion.itemName,
                    type = potion.itemType,
                    quantity = slot.quantity,
                    potion = new PotionSaveData
                    {
                        name = potion.itemName,
                        description = potion.description,
                        purity = potion.purity,
                        strength = potion.strength,
                        rarity = potion.itemRarity,
                        baseValue = potion.baseValue,
                        explorationBonus = potion.explorationBonus,
                        elementMixs = new List<ElementType>(potion.elementMixs),
                        favoredRegionElement = new List<ElementType>(potion.favoredRegionElement)
                    }
                });
            }
            else
            {
                data.items.Add(new SlotSaveData
                {
                    itemID = slot.item.itemName,
                    type = slot.item.itemType,
                    quantity = slot.quantity
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static void DeleteSavePath()
    {
        File.Delete(SavePath);
    }

    public static void LoadInventory(InventoryManager inventory)
    {
        if (!File.Exists(SavePath))
        {
            return;
        }

        string json = File.ReadAllText(SavePath);
        InventoryData data = JsonUtility.FromJson<InventoryData>(json);

        inventory.ClearInventory();

        foreach (var itemSave in data.items)
        {
            PotionData basePotion = Resources.Load<PotionData>($"Potions/{itemSave.itemID}");
            if (itemSave.type == ItemType.Potion && itemSave.potion != null)
            {
                PotionData newPotion = ScriptableObject.CreateInstance<PotionData>();
                newPotion.itemName = itemSave.potion.name;
                newPotion.description = itemSave.potion.description;
                newPotion.itemType = ItemType.Potion;
                newPotion.itemRarity = itemSave.potion.rarity;
                newPotion.purity = itemSave.potion.purity;
                newPotion.strength = itemSave.potion.strength;
                newPotion.baseValue = itemSave.potion.baseValue;
                newPotion.explorationBonus = itemSave.potion.explorationBonus;
                newPotion.elementMixs = new List<ElementType>(itemSave.potion.elementMixs);
                newPotion.favoredRegionElement = new List<ElementType>(itemSave.potion.favoredRegionElement);
       
                if (basePotion != null)
                    newPotion.icon = basePotion.icon;

                inventory.AddItem(newPotion, itemSave.quantity);
            }
            else
            {
                ItemData baseItem = Resources.Load<ItemData>($"Items/{itemSave.itemID}");
                if (baseItem != null)
                    inventory.AddItem(baseItem, itemSave.quantity);
            }
        }

    }

    public static void LoadTest(InventoryManager inventory)
    {
        var baseItem = Resources.LoadAll<ItemData>($"Items/");
        foreach(var item in baseItem)
        {
            inventory.AddItem((ItemData)item,100);
        }
    }


    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}
