using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/ExpeditionData")]
public class ExpeditionData : ScriptableObject
{
    public string expeditionTitle;
    [TextArea(3, 10)]
    public string description;
    public Rank levelDifficulty;
    public int requiredPower;
    public int durationInPeriods; // số buổi cần
    public float baseSuccessRate;
    public float currentSuccessRate;
    public ElementType regionElement;
    public int maxLootItems;
    public int bassMaxLootItems;
    public List<ItemData> possibleLoots;
    public ExpeditionMilitary expeditionMilitary;
    public bool isOnExpedition;
}

