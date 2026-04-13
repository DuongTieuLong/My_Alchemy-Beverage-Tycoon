using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventory;      // ScriptableObject quản lý toàn bộ item
    public Transform contentParent;         // ScrollView -> Content
    public GameObject itemSlotPrefab;       // Prefab slot hiển thị item
    public TextMeshProUGUI typeInventory;
    public TextMeshProUGUI capacity;
    public TextMeshProUGUI sortText;
    public Button sellButton;

    public GameObject itemDetailPanel;      // Panel hiển thị thông tin chi tiết
    public GameObject potionDetailPanel;      // Panel hiển thị thông tin chi tiết
    public UIAnimatorTool animatorTool;

    [Header("Category Buttons")]
    public Button itemInventoryButton;
    public Button potionInventoryButton;
    public Button catalystInventoryButton;

    [Header("Filter")]
    public ItemType showType = ItemType.Potion;

    [Header("Control Feature")]
    public bool isOnBrewingMode = false;

    [Header("NPC Interaction (optional)")]
    public NPCRequest activeNPCRequest;

    private InventorySlot selectedSlot;
    private readonly List<ItemSlotUI> spawnedSlots = new();

    void OnEnable()
    {
        inventory.OnInventoryChanged += RefreshUI;
        animatorTool = GetComponent<UIAnimatorTool>();
    }

    void OnDisable()
    {
        inventory.OnInventoryChanged -= RefreshUI;
    }

    void Start()
    {
        // Gán sự kiện cho nút
        itemInventoryButton.onClick.AddListener(() => ShowCategory(ItemType.Ingredient));
        potionInventoryButton.onClick.AddListener(() => ShowCategory(ItemType.Potion));
        catalystInventoryButton.onClick.AddListener(() => ShowCategory(ItemType.Catalyst));

        inventory.RebuildCategoryLists();
        ShowCategory(ItemType.Ingredient);

        sellButton.interactable = false;

    }
    [ContextMenu("Reset Inventory")]
    public void ResetInventory()
    {
        SaveSystem.DeleteSave();
    }


    public void ShowInventoryWithBrewingMode(bool brewingMode = false)
    {
        animatorTool.Show();
        ShowCategory(ItemType.Ingredient);
        isOnBrewingMode = brewingMode;
    }

    public void HideInventory()
    {
        selectedSlot = null;
        animatorTool.HideIfShow();
        RefreshUI();
        isOnBrewingMode = false;
    }
    public void ShowCategory(ItemType type)
    {
        typeInventory.text = type.ToString();
        showType = type;
        capacity.text = $"Capacity: {inventory.GetTotalItemCount()} / {inventory.maxCapacity}";
        UpdateButtonVisuals();
        LoadSortByRarity();
    }

    public void RefreshCapacity()
    {
        capacity.text = $"Capacity: {inventory.GetTotalItemCount()} / {inventory.maxCapacity}";
    }

    private void UpdateButtonVisuals()
    {
        itemInventoryButton.interactable = (showType == ItemType.Ingredient) ? false : true;
        potionInventoryButton.interactable = (showType == ItemType.Potion) ? false : true;
        catalystInventoryButton.interactable = (showType == ItemType.Catalyst) ? false : true;
    }

    public void RefreshUI()
    {
        // Xóa slot cũ
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        spawnedSlots.Clear();

        // Lấy danh sách item theo loại hiện tại
        List<InventorySlot> slots = inventory.GetItemsByType(showType);

        foreach (var slot in slots)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, contentParent);
            ItemSlotUI slotUI = newSlot.GetComponent<ItemSlotUI>();

            // Gọi Setup, truyền callback OnItemSelected
            slotUI.Setup(slot, this);

            spawnedSlots.Add(slotUI);

            if (selectedSlot == slot)
                slotUI.selectedBorder.SetActive(true);
        }

        if (selectedSlot != null && inventory.HasItem(selectedSlot.item))
        {
            ShowInfo(selectedSlot.item.itemType);
        }
        else
        {
            HideInfo();
            sellButton.interactable = false;
            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Select Item";

        }

        if (isOnBrewingMode && selectedSlot != null)
        {
            OnItemSelected(selectedSlot);
        }
    }

    public void OnItemSelected(InventorySlot slot)
    {

        selectedSlot = slot;
        ShowInfo(selectedSlot.item.itemType);
        if (isOnBrewingMode)
        {
            if (slot.item.itemType == Cauldron.Instance.currentTypeNeed)
            {
                sellButton.interactable = true;
                sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Add to Brew";
            }
            else
            {
                sellButton.interactable = false;
                sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Cannot Add";
            }
            return;
        }

        if (slot.item.itemType == ItemType.Ingredient || slot.item.itemType == ItemType.Catalyst)
        {
            sellButton.interactable = true;
            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell";
        }
        else
        {
            if (slot.item.itemName == "Failed Potion")
            {
                sellButton.interactable = true;
                sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell";
                return;
            }
            sellButton.interactable = false;
            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Cannot Sell";
        }

    }


    public void ShowInfo(ItemType type)
    {
        if (ItemType.Ingredient == type || ItemType.Catalyst == type)
        {
            potionDetailPanel.SetActive(false);
            itemDetailPanel.SetActive(true);

            Transform panel = itemDetailPanel.transform;

            panel.Find("Icon").GetComponent<Image>().sprite = selectedSlot.item.icon;
            panel.Find("Name").GetComponent<TextMeshProUGUI>().text = selectedSlot.item.itemName;
            panel.Find("Value").GetComponent<TextMeshProUGUI>().text = $"Value: <color=#FFFF00>{selectedSlot.item.baseValue}</color>";
            panel.Find("Description").GetComponent<TextMeshProUGUI>().text = selectedSlot.item.description;


            var tmp1 = panel.Find("Source").GetComponent<TextMeshProUGUI>();
            ReformatRegionText(tmp1, selectedSlot.item.sourceRegion.ToString());
            var tmp2 = panel.Find("Rarity").GetComponent<TextMeshProUGUI>();
            ColoredRarityText(tmp2, selectedSlot.item.itemRarity);
        }
        else if (ItemType.Potion == type)
        {
            var potion = (PotionData)selectedSlot.item;
            Transform panel = potionDetailPanel.transform;

            itemDetailPanel.SetActive(false);
            potionDetailPanel.SetActive(true);

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
            purityText.text = $"Purity: {potion.purity * 100f:F1}%";
            strengthText.text = $"Strength: {potion.strength * 100f:F1}%";
            valueText.text = $"Value: <color=#FFFF00>{potion.baseValue}</color>";
            elementMixText.text = "Element Mix: " + string.Join(", ", potion.elementMixs);
            descText.text = potion.description;

            ColoredRarityText(rarityText, potion.itemRarity);
        }
    }

    public void HideInfo()
    {
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
        if (potionDetailPanel != null)
            potionDetailPanel.SetActive(false);
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

    public void ReformatRegionText(TextMeshProUGUI textComponent, string region)
    {
        if (textComponent == null)
            return;
        textComponent.text = "Source: " + region.Replace("_", " ");
    }

    // SẮP XẾP TRONG INVENTORY UI
    // =============================
    public bool descennding = false;
    public void SortByRarity()
    {
        descennding = !descennding;
        if (descennding)
            sortText.text = "Sort By Rarity Descending";
        else
            sortText.text = "Sort By Rarity Ascending";
        SortCurrentCategoryByRarity(descennding);
    }

    public void LoadSortByRarity()
    {
        if (descennding)
            sortText.text = "Sort By Rarity Descending";
        else
            sortText.text = "Sort By Rarity Ascending";
        SortCurrentCategoryByRarity(descennding);

    }

    public void SortCurrentCategoryByRarity(bool descending)
    {
        if (inventory == null)
        {
            return;
        }
        // Gọi hàm sắp xếp từ InventoryManager theo loại đang hiển thị
        inventory.SortByRarity(showType, descending);

        // Làm mới UI sau khi sắp xếp
        RefreshUI();
    }
    public void SellItem()
    {
        if (selectedSlot == null)
        {
            return;
        }

        // Existing brewing behavior
        if (isOnBrewingMode)
        {
            Cauldron.Instance.AddIngredient(selectedSlot.item);
            HideInventory();
            return;
        }

        // Normal sell flow (ingredients/catalysts or failed potions)
        if (selectedSlot.item.itemType == ItemType.Ingredient || selectedSlot.item.itemType == ItemType.Catalyst)
        {
            if (inventory.RemoveItem(selectedSlot.item, 1))
            {
                LoadSortByRarity();
                if (selectedSlot != null)
                    GoldManager.Instance.AddGold(selectedSlot.item.baseValue);
                return;
            }
        }
        else
        {
            if (selectedSlot.item.itemName == "Failed Potion")
            {
                inventory.RemoveItem(selectedSlot.item, 1);
                LoadSortByRarity();
                GoldManager.Instance.AddGold(selectedSlot.item.baseValue);
                return;
            }

        }
    }
}
