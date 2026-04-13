using System;
using UnityEngine;
public enum TimePeriod { None, Day, Night }
public class TimeCycleManager : MonoBehaviour
{
    public static TimeCycleManager Instance;

    [Header("Time Config")]
    public float periodDuration = 180f; // 3 phút = 180 giây
    public float timer;
    public Light directionLight;
    public TimePeriod CurrentPeriod { get; private set; } = TimePeriod.None;

    public int DayCount { get; private set; } = 1;

    public event Action<TimePeriod> OnPeriodChanged;
    public event Action<int, int, TimePeriod> OnTimeUpdated;
    // gửi UI: hour, minute, period

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartGame()
    {
        CurrentPeriod = TimePeriod.Day;
        DayCount = 1;
        timer = 0;
    }

    private void Update()
    {
        RunTimeCycle();
        UpdateSunLight();
    }

    private void UpdateSunLight()
    {
        if (directionLight == null || CurrentPeriod == TimePeriod.None) return;

        // Normalized time trong period (0 → 1)
        float normalized = timer / periodDuration;

        // Góc xoay mặt trời: Day 6h → 18h, Night 18h → 6h
        float sunAngle;

        if (CurrentPeriod == TimePeriod.Day)
        {
            // Day: 6:00 → 18:00 = 180 độ (mặt trời mọc → lặn)
            sunAngle = Mathf.Lerp(0f, 180f, normalized);
        }
        else
        {
            // Night: 18:00 → 6:00 = 180 độ (mặt trời đi xuống → lên)
            sunAngle = Mathf.Lerp(180f, 360f, normalized);
        }

        // Áp dụng góc xoay theo trục X
        directionLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Thay đổi cường độ ánh sáng để mượt: max 1 → min 0.1
        float intensity = (CurrentPeriod == TimePeriod.Day)
                            ? Mathf.Lerp(0.1f, 1f, Mathf.Sin(normalized * Mathf.PI))  // sin curve cho ánh sáng tự nhiên
                            : Mathf.Lerp(0.1f, 0f, Mathf.Sin(normalized * Mathf.PI));

        directionLight.intensity = intensity;

        // Optional: đổi màu ánh sáng (vàng ban ngày → xanh lam ban đêm)
        Color dayColor = new Color(1f, 0.956f, 0.839f); // vàng nhạt
        Color nightColor = new Color(0.05f, 0.1f, 0.2f); // xanh đêm
        directionLight.color = Color.Lerp(dayColor, nightColor, CurrentPeriod == TimePeriod.Day ? 1f - Mathf.Sin(normalized * Mathf.PI) : Mathf.Sin(normalized * Mathf.PI));
    }
    private void RunTimeCycle()
    {
        if (CurrentPeriod == TimePeriod.None) return;
        timer += Time.deltaTime;

        float normalized = timer / periodDuration;  // 0 → 1
        float totalMinutes = normalized * 720f;     // mỗi period = 12 giờ = 720 phút

        int minuteOfDay;

        if (CurrentPeriod == TimePeriod.Day)
        {
            // Day: 6:00 → 18:00
            minuteOfDay = 360 + Mathf.FloorToInt(totalMinutes);  // 360 = 6*60
        }
        else
        {
            // Night: 18:00 → 6:00 (qua ngày hôm sau)
            minuteOfDay = 1080 + Mathf.FloorToInt(totalMinutes); // 1080 = 18*60
        }

        // Nếu vượt quá 24h, loop về đầu
        minuteOfDay %= 1440;

        // Convert minute → hour + minute
        int hour = minuteOfDay / 60;
        int minute = minuteOfDay % 60;

        OnTimeUpdated?.Invoke(hour, minute, CurrentPeriod);

        // Kết thúc chu kỳ Day hoặc Night
        if (timer >= periodDuration)
        {
            timer = 0;
            SwitchPeriod();
        }
    }


    private void SwitchPeriod()
    {
        CurrentPeriod = (CurrentPeriod == TimePeriod.Day) ? TimePeriod.Night : TimePeriod.Day;
        if (CurrentPeriod == TimePeriod.Day)
            DayCount++;

        OnPeriodChanged?.Invoke(CurrentPeriod);

    }

    public void SetPeriod(TimePeriod period)
    {
        CurrentPeriod = period;
    }

    public void SkipPeriod()
    {
        CurrentPeriod = (CurrentPeriod == TimePeriod.Day) ? TimePeriod.Night : TimePeriod.Day;
        if (CurrentPeriod == TimePeriod.Day)
            DayCount++;
        timer = 0;

        OnPeriodChanged?.Invoke(CurrentPeriod);
    }
}
