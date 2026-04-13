using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public NPCRequest npcRequest;
    public GameObject interactPanel;

    private void Start()
    {
        interactPanel.SetActive(false);
    }

    public void OpenPanel()
    {
        interactPanel.SetActive(true);
    }
    public void ClosePanel()
    {
        interactPanel.SetActive(false);
    }
    public void OnInteract()
    {
        ShopManager.Instance.NpcRefresh(npcRequest);
    }
}
