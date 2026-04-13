using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotionInventoryUIManager : BasePotionInventoryUI
{
    public ExpeditionUI expeditionUI;
    public UIAnimatorTool animatorTool;
    public override void OnItemSelected(InventorySlot slot)
    {
        TutorialSignal.Emit("ViewApotion");
        base.OnItemSelected(slot);

        foreach (Transform t in contentParent.transform)
        {
            PotionSlotUI potionSlot = t.GetComponent<PotionSlotUI>();
            if (potionSlot != null && potionSlot.slot == slot)
            {
                ShowDetailAt(t.GetComponent<RectTransform>());
                break;
            }
        }
    }

    [Tooltip("Canvas chứa UI (Screen Space - Overlay/Camera hoặc World)")]
    public Canvas canvas;

    [Tooltip("Khoảng cách mặc định so với target (pixel, local canvas space). Ví dụ (250,0) để hiện bên phải.")]
    public Vector2 offset = new Vector2(250f, 0f);

    /// <summary>
    /// Hiển thị detailPanel cạnh target RectTransform, tự động chuyển sang bên trái nếu bên phải tràn,
    /// đồng thời clamp để không ra khỏi canvas (trên/dưới/trái/phải).
    /// </summary>
    public void ShowDetailAt(RectTransform target)
    {
        var detailPanel = this.detailPanel.GetComponent<RectTransform>();

        if (detailPanel == null || canvas == null || target == null)
        {
            return;
        }

        // Chọn camera tương ứng với render mode của canvas
        Camera cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            cam = canvas.worldCamera;

        // Lấy điểm screen của target
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, target.position);

        // Chuyển screenPoint sang local point trong canvas RectTransform
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out localPoint);

        // Bắt đầu với vị trí mặc định (bên phải)
        Vector2 desiredPos = localPoint + offset;

        // Nếu bên phải tràn → thử bên trái
        // Đo kích thước và pivot panel
        Vector2 panelSize = detailPanel.rect.size;
        Vector2 panelPivot = detailPanel.pivot;

        float halfCanvasW = canvasRect.rect.width * 0.5f;
        float halfCanvasH = canvasRect.rect.height * 0.5f;

        // Tính toạ độ cạnh trái/ phải/ trên/ dưới của panel nếu đặt tại desiredPos
        float rightEdge = desiredPos.x + panelSize.x * (1f - panelPivot.x);
        float leftEdge = desiredPos.x - panelSize.x * panelPivot.x;

        // Nếu right tràn quá canvas → đặt bên trái của target
        if (rightEdge > halfCanvasW)
        {
            Vector2 leftCandidate = localPoint - offset;
            // kiểm tra leftCandidate có tràn trái không; nếu tràn phải thì sẽ clamp sau
            desiredPos = leftCandidate;
        }

        // Bây giờ clamp cả chiều ngang và chiều dọc để chắc chắn không ra ngoài canvas
        float minX = -halfCanvasW + panelSize.x * panelPivot.x;
        float maxX = halfCanvasW - panelSize.x * (1f - panelPivot.x);
        float minY = -halfCanvasH + panelSize.y * panelPivot.y;
        float maxY = halfCanvasH - panelSize.y * (1f - panelPivot.y);

        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);

        // Áp dụng vị trí
        detailPanel.anchoredPosition = desiredPos;
        if (!detailPanel.gameObject.activeSelf) detailPanel.gameObject.SetActive(true);
    }

    public override void OnConfirmAction()
    {
        if (selectedSlot == null)
        {
            return;
        }

        if (selectedSlot.quantity >= 1)
        {
            expeditionUI.AddPotion(selectedSlot.item as PotionData);
        }
        detailPanel.Hide();
    }

    public void CloseUI()
    {
        selectedSlot = null;
        animatorTool.HideIfShow();
        RefreshUI();
    }

}
