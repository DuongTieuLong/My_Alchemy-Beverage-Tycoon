using UnityEngine;
using UnityEngine.UI;

public class InventoryUpgrade : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public InventoryUI inventoryUI;
    public UIAnimatorTool panel;
    public Button buyButton;

    private void Start()
    {
        buyButton.onClick.AddListener(Upgrade);
    }

    public void OpenPanel()
    {
        panel.Show();
        if (GoldManager.Instance.Gold >= 1000)
        {
            buyButton.interactable = true;
        }
        else
        {
            buyButton.interactable = false;
        }
    }

    public void Upgrade()
    {
        if (GoldManager.Instance.SpendGold(1000))
        {
            buyButton.interactable = false;
            inventoryManager.maxCapacity += 10;
            inventoryUI.RefreshCapacity();
        }
        panel.Hide();
    }

}
