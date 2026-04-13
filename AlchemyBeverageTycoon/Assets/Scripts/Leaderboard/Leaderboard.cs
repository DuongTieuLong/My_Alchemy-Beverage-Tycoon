using System;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public static Leaderboard Instance;

    public int leaderboardSize = 20;
    public int maxIncrease;
    public string playerName = "You";

    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

    private string saveKey = "offline_leaderboard";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Auto update khi reputation thay đổi
        ReputationManager.Instance.OnReputationChanged += OnPlayerReputationChanged;
        // Cập nhật lần đầu
        UpdatePlayerScore(ReputationManager.Instance.GetRepulation());
        TimeCycleManager.Instance.OnPeriodChanged += SimulateAIRise;
    }

    // Khi reputation thay đổi → Cập nhật rank
    private void OnPlayerReputationChanged(int newValue)
    {
        UpdatePlayerScore(newValue);
    }

    // Khi chưa có dữ liệu → tạo AI giả lập
    void GenerateAIEntries()
    {
        entries.Clear();
        string[] aiNames = {
             "Nina", "Zero", "Mira", "Lime", "Bolt", "Kai", "Nero", "Fenix", "Skye", "Rin",
             "Astra", "Blitz", "Cinder", "Drake", "Echo", "Flare", "Gale", "Haze", "Iris", "Jinx",
             "Kuro", "Luna", "Mako", "Nova", "Onyx", "Pyra", "Quartz", "Raze", "Shade", "Talon",
             "Umbra", "Vex", "Wisp", "Xeno", "Yami", "Zephyr", "Rogue", "Violet", "Crux", "Sora",
             "Niko", "Ryder", "Flint", "Sable", "Valk", "Orion", "Rei", "Cobalt", "Nox", "Vale"
        };

        for (int i = 0; i < leaderboardSize - 1; i++)
        {
            int randomScore = UnityEngine.Random.Range(100, 7000);
            string name = aiNames[UnityEngine.Random.Range(0, aiNames.Length)];

            entries.Add(new LeaderboardEntry(name, randomScore));
        }

        entries.Add(new LeaderboardEntry(playerName, 0, true));
    }

    void EnsurePlayerExist()
    {
        if (!entries.Exists(e => e.isPlayer))
        {
            entries.Add(new LeaderboardEntry(playerName, 0, true));
        }
    }

    // Cập nhật điểm người chơi dựa trên Reputation
    public void UpdatePlayerScore(int newScore)
    {
        var p = entries.Find(e => e.isPlayer);
        if (p != null) p.score = newScore;

        SortLeaderboard();
    }

    // AI tăng reputation để mô phỏng cạnh tranh
    public void SimulateAIRise(TimePeriod time)
    {
        if (time == TimePeriod.Night)
        {
            foreach (var e in entries)
            {
                if (!e.isPlayer)
                {
                    e.score += UnityEngine.Random.Range(0, maxIncrease);
                }
                
            }
        }
        SortLeaderboard();
        SaveLeaderboard();
    }

    void SortLeaderboard()
    {
        entries.Sort((a, b) => b.score.CompareTo(a.score));
    }

    // Lấy top N để đổ ra UI
    public List<LeaderboardEntry> GetTop(int count)
    {
        return entries.GetRange(0, Mathf.Min(count, entries.Count));
    }

    public int GetPlayerRank()
    {
        return entries.FindIndex(e => e.isPlayer) + 1;
    }

    // Lưu dữ liệu
    public void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(new LeaderboardWrapper(entries));
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteKey(saveKey);

        LoadLeaderboard();
        EnsurePlayerExist();
        UpdatePlayerScore(ReputationManager.Instance.GetRepulation());
        SortLeaderboard();
    }

    public void LoadGame()
    {
        LoadLeaderboard();
        EnsurePlayerExist();
        UpdatePlayerScore(ReputationManager.Instance.GetRepulation());
        SortLeaderboard();
    }

    // Load dữ liệu
    public void LoadLeaderboard()
    {
        if (!PlayerPrefs.HasKey(saveKey))
        {
            GenerateAIEntries();
            EnsurePlayerExist();
            SaveLeaderboard();
            return;
        }

        string json = PlayerPrefs.GetString(saveKey);
        var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
        entries = wrapper.list;
    }
    public LeaderboardEntry GetPlayerEntry()
    {
        return entries.Find(e => e.isPlayer);
    }

}

[Serializable]
public class LeaderboardWrapper
{
    public List<LeaderboardEntry> list;
    public LeaderboardWrapper() { }

    public LeaderboardWrapper(List<LeaderboardEntry> list)
    {
        this.list = list;
    }
}

[Serializable]
public class LeaderboardEntry
{
    public string name;
    public int score;
    public bool isPlayer;

    public LeaderboardEntry(string name, int score, bool isPlayer = false)
    {
        this.name = name;
        this.score = score;
        this.isPlayer = isPlayer;
    }

}

