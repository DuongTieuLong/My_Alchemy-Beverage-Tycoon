using UnityEngine;

public static class PriceCalculator
{
    public static int GetFinalPrice(PotionData p)
    {
        float price = p.baseValue;

        var e = MarketTrendManager.Instance.currentTrend;
        if (e == null || e.type == PriceBoostType.None)
            return Mathf.RoundToInt(price);

        switch (e.type)
        {
            case PriceBoostType.BoostElement:
                foreach (var element in p.elementMixs)
                {
                    if (element == e.boostedElement)
                    {
                        price *= e.priceMultiplier;
                        break;
                    }
                }
                break;

            case PriceBoostType.BoostRarity:
                if (p.itemRarity == e.boostedRarity)
                    price *= e.priceMultiplier;
                break;

            case PriceBoostType.BoostedPurity:
                if (p.purity >= e.boostedPurity)
                    price *= e.priceMultiplier;
                break;

            case PriceBoostType.BoostStrengt:
                if (p.strength == e.boostedStrenght)
                    price *= e.priceMultiplier;
                break;

            case PriceBoostType.BoostAll:
                price *= e.priceMultiplier;
                break;
        }

        return Mathf.RoundToInt(price);
    }
}
