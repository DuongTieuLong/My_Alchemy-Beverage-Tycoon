using System;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance;

    [SerializeField]  private int reputation = 0;
    public int startReputation = 0;
    public event Action<int> OnReputationChanged;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI reputation2Text;
    public TextMeshProUGUI rankText;
    public Image backGroundRank;

    public Rank currentRank = Rank.Apprentice;
    public int adeptRequired =100;
    public int expertRequired = 1000;
    public int masterRequired = 5000;
    public int grandMasterRequired = 10000;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGameReputation()
    {
        reputation = startReputation;
        StartSetup();
    }

    private void StartSetup()
    {
        backGroundRank.color = ColorByRank(currentRank);
        UnityEngine.Color c = backGroundRank.color;
        c.a = Mathf.Clamp01(0.2f);
        backGroundRank.color = c;
        AddReputation(0);
    }
    public int GetRepulation()
    {
        return reputation;              
    }

    public void AddReputation(int amount)
    {
        reputation += amount;
        if(reputation < 0) reputation = 0;
        reputationText.text = reputation.ToString();
        reputation2Text.text = reputation.ToString();
        CheckRankUp();
        OnReputationChanged?.Invoke(reputation);
    }

    public void SetReputation(int value)
    {
        reputation = value;
        reputationText.text = reputation.ToString();
        reputation2Text.text = reputation.ToString();
        StartSetup();
        OnReputationChanged?.Invoke(reputation);
    }

    public void CheckRankUp()
    {
        Rank newRank = currentRank;

        if (reputation >= grandMasterRequired)
            newRank = Rank.Grandmaster;
        else if (reputation >= masterRequired)
            newRank = Rank.Master;
        else if (reputation >= expertRequired)
            newRank = Rank.Expert;
        else if (reputation >= adeptRequired)
            newRank = Rank.Adept;
        else
            newRank = Rank.Apprentice;

        if (newRank != currentRank)
        {
            currentRank = newRank;
            rankText.text = newRank.ToString();
            backGroundRank.color = ColorByRank(currentRank);
            UnityEngine.Color c = backGroundRank.color;
            c.a = Mathf.Clamp01(0.2f);
            backGroundRank.color = c;
        }

       
    }
    public static UnityEngine.Color ColorByRank(Rank rank)
    {
        switch (rank)
        {
            case Rank.Apprentice:
                return UnityEngine.Color.white;
            case Rank.Adept:
                return HexToColor("00FF16"); // xanh lá
            case Rank.Expert:
                return HexToColor("00B1FF"); // xanh dương
            case Rank.Master:
                return HexToColor("FF0000"); // đỏ
            case Rank.Grandmaster:
                return HexToColor("FF7A00"); // cam
            default:
                return UnityEngine.Color.white;
        }
    }

    private static UnityEngine.Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out UnityEngine.Color color))
            return color;
        return UnityEngine.Color.white;
    }
}