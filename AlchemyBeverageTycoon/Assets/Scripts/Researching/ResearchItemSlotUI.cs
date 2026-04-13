using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchItemSlotUI : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Image icon;
    public TextMeshProUGUI amountText;

    public void Setup(ItemRequirement req)
    {
        icon.sprite = req.item.icon;

        int have = inventoryManager.GetItemCount(req.item);
        amountText.text = $"{have}/{req.requiredAmount}";

        amountText.color = have >= req.requiredAmount ? Color.white : Color.red;
    }
}
