using UnityEngine;

public class PotionSellUI : BasePotionInventoryUI
{
    public Vector3 offset = Vector3.zero;

    public override void OnItemSelected(InventorySlot slot)
    {
        base.OnItemSelected(slot);
        foreach (Transform transform in contentParent.transform)
        {
            if(transform.GetComponent<PotionSlotUI>().slot == slot)
            {
                detailPanel.transform.position = transform.position + offset;
            }
        }
    }

    public override void OnConfirmAction()
    {
        if (selectedSlot == null)
        {
            return;
        }
        detailPanel.Hide();
        ShopManager.Instance.AddPotionToNpcBag(selectedSlot.item as PotionData);
    }
}
