using TMPro;
using UnityEngine;

public class TimeUIController : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI periodText;
    public TextMeshProUGUI dayText;

    private void OnDisable()
    {
        TimeCycleManager.Instance.OnTimeUpdated -= UpdateUI;    
        TimeCycleManager.Instance.OnPeriodChanged -= UpdatePeriod;
    }
    private void Start()
    {
        if (TimeCycleManager.Instance != null)
        {
            TimeCycleManager.Instance.OnTimeUpdated += UpdateUI;
            TimeCycleManager.Instance.OnPeriodChanged += UpdatePeriod;
        }
    }
    private void UpdateUI(int hour, int minute,TimePeriod period)
    {
        timeText.text = $"Time: {hour:00}:{minute:00}";
        dayText.text = $"Day: {TimeCycleManager.Instance.DayCount}";
    }

    private void UpdatePeriod(TimePeriod period)
    {
        periodText.text = period.ToString();
    }
}
