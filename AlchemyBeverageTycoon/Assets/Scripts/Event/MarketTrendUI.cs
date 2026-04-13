using System.Text;
using TMPro;
using UnityEngine;

public class MarketTrendUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI trendNameText;
    public TextMeshProUGUI trendDescriptionText;
    public TextMeshProUGUI requirementText;


    public UIAnimatorTool trendAnimatorTool;

    public void Start()
    {
        trendAnimatorTool = GetComponent<UIAnimatorTool>();
        MarketTrendManager.Instance.OnTrendChanged += ShowTrendUI;
    }

    /// <summary>
    /// Displays the content of a MarketTrend SO onto the UI.
    /// </summary>
    /// 
    public void ShowCurrentUiTrend()
    {
        ShowTrendUI(MarketTrendManager.Instance.currentTrend);
    }

    private void ShowTrendUI(MarketTrend trend)
    {
        trendAnimatorTool.Show();
        if (trend == null)
        {
            trendNameText.text = "No Active Market Trend";
            trendDescriptionText.text = "The market remains stable today.";
            requirementText.text = "";
            return;
        }

        // 1. Title & Description
        trendNameText.text = trend.trendName;
        trendDescriptionText.text = trend.description;

        // 2. Build requirement details
        StringBuilder req = new StringBuilder();
        req.AppendLine("<b>Requirements:</b>");

        switch (trend.type)
        {
            case PriceBoostType.BoostElement:
                req.AppendLine($"• Element: <color=#FFCC66>{trend.boostedElement}</color>");
                break;

            case PriceBoostType.BoostRarity:
                req.AppendLine($"• Rarity: <color=#66CCFF>{trend.boostedRarity}</color>");
                break;

            case PriceBoostType.BoostedPurity:
                req.AppendLine($"• Purity ≥ <color=#99FFAA>{trend.boostedPurity}</color>");
                break;

            case PriceBoostType.BoostStrengt:
                req.AppendLine($"• Strength ≥ <color=#FF9999>{trend.boostedStrenght}</color>");
                break;

            case PriceBoostType.BoostAll:
                req.AppendLine("• Applies to all potion types.");
                break;

            default:
                req.AppendLine("• No special requirements.");
                break;
        }

        // 3. Add price multiplier
        req.AppendLine($"• Price Increase: <b>x{trend.priceMultiplier}</b>");

        // Apply UI
        requirementText.text = req.ToString();
    }
    public void CloseUI()
    {
        trendAnimatorTool.Hide();
    }
}
