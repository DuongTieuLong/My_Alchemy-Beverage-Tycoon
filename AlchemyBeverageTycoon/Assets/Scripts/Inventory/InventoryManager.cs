using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

[CreateAssetMenu(menuName = "Alchemy/Inventory")]
public class InventoryManager : ScriptableObject
{
    [Header("Inventory Data")]
    [SerializeField] private List<InventorySlot> allItems = new List<InventorySlot>();

    // Chia riêng từng loại
    [NonSerialized] public List<InventorySlot> ingredients = new List<InventorySlot>();
    [NonSerialized] public List<InventorySlot> catalysts = new List<InventorySlot>();
    [NonSerialized] public List<InventorySlot> potions = new List<InventorySlot>();

    public event Action OnInventoryChanged;

    public int maxCapacity = 100;

    // ================================================================
    #region CORE FUNCTIONS
    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return;

        if (maxCapacity > 0 && GetTotalItemCount() + amount > maxCapacity)
        {
            return;
        }

        var slot = allItems.Find(x => x.item == item);
        if (slot != null)
        {
            slot.quantity += amount;
        }
        else
        {
            slot = new InventorySlot(item, amount);
            allItems.Add(slot);
        }

        RebuildCategoryLists();
        OnInventoryChanged?.Invoke();
    }
    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (var slot in allItems)
        {
            total += slot.quantity;
        }
        return total;
    }
    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        var slot = allItems.Find(x => x.item == item);
        if (slot == null) return false;

        slot.quantity -= amount;
        if (slot.quantity <= 0)
            allItems.Remove(slot);

        RebuildCategoryLists();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetItemCount(ItemData item)
    {
        var slot = allItems.Find(x => x.item == item);
        return slot != null ? slot.quantity : 0;
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        return GetItemCount(item) >= amount;
    }

    public void ClearInventory()
    {
        allItems.Clear();
        ingredients.Clear();
        catalysts.Clear();
        potions.Clear();
        OnInventoryChanged?.Invoke();
    }
    #endregion
    // ================================================================

    #region CATEGORY MANAGEMENT
    /// <summary>
    /// Xây lại danh sách chia loại
    /// </summary>
    public void RebuildCategoryLists()
    {
        ingredients.Clear();
        catalysts.Clear();
        potions.Clear();

        foreach (var slot in allItems)
        {
            if (slot.item == null) continue;

            switch (slot.item.itemType)
            {
                case ItemType.Ingredient:
                    ingredients.Add(slot);
                    break;
                case ItemType.Catalyst:
                    catalysts.Add(slot);
                    break;
                case ItemType.Potion:
                    potions.Add(slot);
                    break;
            }
        }
    }

    public List<InventorySlot> GetAllItems() => allItems;
    public List<InventorySlot> GetItemsByType(ItemType type)
    {
        return type switch
        {
            ItemType.Ingredient => ingredients,
            ItemType.Catalyst => catalysts,
            ItemType.Potion => potions,
            _ => allItems
        };
    }
    #endregion
    // ================================================================

    #region SORTING SYSTEM

    /// <summary>
    /// Sắp xếp theo độ hiếm
    /// </summary>
    public void SortByRarity(ItemType? filterType = null, bool descending = true)
    {
        List<InventorySlot> list = filterType.HasValue ? GetItemsByType(filterType.Value) : allItems;
        list.Sort((a, b) =>
        {
            int compare = a.item.itemRarity.CompareTo(b.item.itemRarity);
            return descending ? -compare : compare;
        });
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Sắp xếp Potion theo ElementType đầu tiên trong danh sách elementMixs
    /// </summary>
    public void SortPotionsByElement(ElementType targetElement)
    {
        potions.Sort((a, b) =>
        {
            PotionData pa = a.item as PotionData;
            PotionData pb = b.item as PotionData;

            bool aHas = pa != null && pa.elementMixs.Contains(targetElement);
            bool bHas = pb != null && pb.elementMixs.Contains(targetElement);

            // Ưu tiên item có ElementType mong muốn lên đầu
            if (aHas && !bHas) return -1;
            if (!aHas && bHas) return 1;
            return string.Compare(pa?.itemName, pb?.itemName, StringComparison.OrdinalIgnoreCase);
        });
        OnInventoryChanged?.Invoke();
    }

    #endregion

    #region Save/ Load
    public void Save() => SaveSystem.SaveInventory(this);
    public void Load() => SaveSystem.LoadInventory(this);
    public void LoadTest() => SaveSystem.LoadTest(this);
    public void DeleteSave() => SaveSystem.DeleteSave();
    #endregion
}
