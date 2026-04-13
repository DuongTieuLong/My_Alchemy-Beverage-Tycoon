using UnityEngine;

[CreateAssetMenu(menuName = "Game/MarketTrend")]
public class MarketTrend : ScriptableObject
{
    public string trendName;
    [TextArea] public string description;

    public PriceBoostType type;

    public ElementType boostedElement;
    public ItemRarity boostedRarity;

    public float boostedPurity;
    public float boostedStrenght;

    [Range(1f, 5f)]
    public float priceMultiplier = 1f;

}
public enum PriceBoostType
{
    None,
    BoostElement,
    BoostRarity,
    BoostStrengt,
    BoostedPurity,
    BoostAll,
}
