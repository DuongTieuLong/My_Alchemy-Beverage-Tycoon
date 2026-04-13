using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image icon;
    public TextMeshProUGUI quantityText;
    public Button button;
    public GameObject selectedBorder;

    private InventorySlot slot;
    private InventoryUI parentUI;

    public void Setup(InventorySlot slot, InventoryUI parent)
    {
        this.slot = slot;
        this.parentUI = parent;

        icon.sprite = slot.item.icon;
        quantityText.text = slot.quantity.ToString();
        selectedBorder.SetActive(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {

        parentUI.OnItemSelected(slot);
        // Cập nhật border chọn
        foreach (var s in parentUI.contentParent.GetComponentsInChildren<ItemSlotUI>())
            s.selectedBorder.SetActive(false);

        selectedBorder.SetActive(true);
    }
}
