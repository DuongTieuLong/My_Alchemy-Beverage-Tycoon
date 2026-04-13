using System.Collections.Generic;
using UnityEngine;

public abstract class BasePotionInventoryUI : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory;
    public Transform contentParent;
    public GameObject itemSlotPrefab;
    public ShowPotionInfo detailPanel;

    [Header("Filter")]
    public ItemType showType = ItemType.Potion;

    protected List<PotionSlotUI> spawnedSlots = new List<PotionSlotUI>();
    public InventorySlot selectedSlot;

    protected virtual void Start()
    {
        inventory.OnInventoryChanged += RefreshUI;
        inventory.RebuildCategoryLists();
        RefreshUI();
    }

    public virtual void RefreshUI()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        spawnedSlots.Clear();

        List<InventorySlot> slots = inventory.GetItemsByType(showType);
        foreach (var slot in slots)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, contentParent);
            PotionSlotUI slotUI = newSlot.GetComponent<PotionSlotUI>();
            slotUI.Setup(slot, this);
            spawnedSlots.Add(slotUI);
        }

        if (selectedSlot == null)
            detailPanel.Hide();
    }

    public virtual void OnItemSelected(InventorySlot slot)
    {
        selectedSlot = slot;
        detailPanel.Show(slot.item as PotionData);
    }

    // Lớp con sẽ định nghĩa hành động này (ví dụ: đem đi expedition, hoặc bán)
    public abstract void OnConfirmAction();
}
