using System.Collections.Generic;
using UnityEngine;

public class    ExpeditionManager : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public static ExpeditionManager Instance;
    public List<ExpeditionInstance> activeExpeditions = new();
    public List<ExpeditionInstance> completeExpeditions = new();


    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        TimeCycleManager.Instance.OnPeriodChanged += OnTimePeriodChanged;
    }

    public void StartExpedition(ExpeditionData data, List<PotionData> carriedItems)
    {
        ExpeditionInstance expedition = new ExpeditionInstance(inventoryManager, data, carriedItems);
        activeExpeditions.Add(expedition);
    }

    private void OnTimePeriodChanged(TimePeriod period)
    {
        foreach (var exp in activeExpeditions)
        {
            if (exp.isActive)
                exp.ProgressOnePeriod(); 
        }
    }

    public CompleteExpeditionInfo CollectComplete(ExpeditionInstance expedition)
    {
        CompleteExpeditionInfo completeInfo = expedition.ClaimRewards();
        activeExpeditions.Remove(expedition);
        return completeInfo;
    }

    public ExpeditionInstance GetExpeditionbyExpeditionData(ExpeditionData data)
    {
        foreach (var exp in activeExpeditions)
        {
            if (exp.data == data)
            {
                return exp;
            }
        }
        return null;
    }



}
