using UnityEngine;
using System;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance; // Singleton để truy cập dễ dàng

    [Header("Initial Settings")]
    [SerializeField] private int startingGold = 500;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI goldText;

    private int currentGold;
    public int Gold => currentGold;

    // Sự kiện khi vàng thay đổi (cho UI hoặc hệ thống khác lắng nghe)
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartGameGold()
    {
        currentGold = startingGold;
        UpdateGoldUI();
    }


    /// <summary>
    /// Thêm vàng
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        UpdateGoldUI();
    }

    /// <summary>
    /// Trừ vàng, trả về true nếu giao dịch thành công
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        if (currentGold < amount)
        {
            return false;
        }

        currentGold -= amount;
        UpdateGoldUI();
        return true;
    }

    /// <summary>
    /// Kiểm tra có đủ vàng hay không
    /// </summary>
    public bool HasEnoughGold(int amount) => currentGold >= amount;

    /// <summary>
    /// Lấy số vàng hiện tại
    /// </summary>
    public int GetCurrentGold() => currentGold;

    /// <summary>
    /// Cập nhật UI và gọi sự kiện
    /// </summary>
    private void UpdateGoldUI()
    {
        goldText.text = currentGold.ToString();
        OnGoldChanged?.Invoke(currentGold);
    }
}
