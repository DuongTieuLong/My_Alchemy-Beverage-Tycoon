using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PotionSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image icon;
    public TextMeshProUGUI quantityText;
    public Button button;
    public GameObject selectedBorder;

    public InventorySlot slot;
    private BasePotionInventoryUI parentUI;

    public void Setup(InventorySlot slot, BasePotionInventoryUI parent)
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
        foreach (var s in parentUI.contentParent.GetComponentsInChildren<PotionSlotUI>())
            s.selectedBorder.SetActive(false);

        selectedBorder.SetActive(true);
    }
}
