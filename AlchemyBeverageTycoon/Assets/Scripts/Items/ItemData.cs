using UnityEngine;

public enum ItemType { Ingredient, Catalyst, Potion }
public enum sourceRegion { Greenwood_Forest, Sunny_Plains, Blackmist_Swamp, Everfrost_Cavern, Crimson_Volcano, Spirit_Grove, Sunscar_Desert, Crafting}

public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(menuName = "Alchemy/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Base Info")]
    public string itemName;
    [TextArea(2, 4)] public string description;
    public ItemType itemType;
    public Sprite icon;
    public sourceRegion sourceRegion;

    [Header("Stats")]
    public int baseValue = 10;
    public ItemRarity itemRarity = ItemRarity.Common;
    public override string ToString() => $"{itemName} ({itemType})";
}


