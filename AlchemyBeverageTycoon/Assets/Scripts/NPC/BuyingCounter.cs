using System.Collections;
using UnityEngine;

public class BuyingCounter : MonoBehaviour
{
    public static BuyingCounter Instance;
    public bool IsBusy { get; private set; }

    private NPCController currentNPC;

    private void Awake()
    {
        Instance = this;
    }

    public void OnCustomer(NPCController npc)
    {
        if (npc == null) return;
        currentNPC = npc;
        IsBusy = true;
        var npcInteract = currentNPC.GetComponent<NPCInteract>();
        npcInteract.OpenPanel();
    }


    public void FinishSellingTo()
    {
        if (currentNPC == null) return;
        QueueManager.Instance?.OnCustomerDone(currentNPC);
        currentNPC.GetComponent<NPCInteract>().ClosePanel();
        IsBusy = false;
        currentNPC = null;

    }
}