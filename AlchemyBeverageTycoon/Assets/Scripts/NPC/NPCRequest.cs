
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Request", menuName = "NPC/NPC Request")]
public class NPCRequest : ScriptableObject
{
    public string npcName = "Sir Bramble";
    [TextArea] public string requestText = "I need something that burns the veins!";
    [TextArea] public string responeGoodText =  $"Great!";
    [TextArea] public string responeBadText = $"What is this nonsense?";
    public ElementType requestedElement = ElementType.Fire;
    public Rank npcRank = Rank.Apprentice;

    public int potionCountRequest = 1;

    [Header("Rewards")]
    public int rewardGold = 50;
    public int rewardReputation = 5;

    [Header("Penalty (if wrong)")]
    public int penaltyReputation = 3;
}