using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
public static class NPCRequestManager
{
    /// <summary>
    /// Try to fulfill an NPC request with the provided potion.
    /// Returns true when accepted (reward applied), false when rejected (penalty applied).
    /// Inventory removal is applied in both cases if the potion existed.
    /// </summary>
    public static bool TryFulfillRequest(InventoryManager inventory, PotionData potion, NPCRequest request)
    {
        if (inventory == null)
        {
            return false;
        }
        if (potion == null)
        {
            return false;
        }
        if (request == null)
        {
            return false;
        }

        bool match = potion.elementMixs != null && potion.elementMixs.Contains(request.requestedElement);

        if (match)
        {
            ReputationManager.Instance?.AddReputation(request.rewardReputation);
            return true;
        }
        else
        {
            ReputationManager.Instance?.AddReputation(-request.penaltyReputation);
            return false;
        }
    }

}

