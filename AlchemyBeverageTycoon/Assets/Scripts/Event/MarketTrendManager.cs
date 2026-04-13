using UnityEngine;
using System;

public class MarketTrendManager : MonoBehaviour
{
    public static MarketTrendManager Instance;

    public MarketTrend currentTrend;

    public event Action<MarketTrend> OnTrendChanged;

    public MarketTrend[] randomTrends; // danh sách event ngẫu nhiên

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    private void Start()
    {
        TimeCycleManager.Instance.OnPeriodChanged += CheckNewDay;
    }

    public void CheckNewDay(TimePeriod period)
    {
        if (period == TimePeriod.Day && TimeCycleManager.Instance.DayCount > 3)
        {
            GenerateNewTrend();
        }
    }

    public void GenerateNewTrend()
    {
        // 50% random, 50% none
        float roll = UnityEngine.Random.value;

        if (roll < 0.5f)
            currentTrend = randomTrends[UnityEngine.Random.Range(0, randomTrends.Length)];
        else
            currentTrend = null;

        OnTrendChanged?.Invoke(currentTrend);
    }
    public bool IsElementBoosted(ElementType e)
    {
        if (currentTrend == null) return false;
        return currentTrend.boostedElement == e;
    }
}
