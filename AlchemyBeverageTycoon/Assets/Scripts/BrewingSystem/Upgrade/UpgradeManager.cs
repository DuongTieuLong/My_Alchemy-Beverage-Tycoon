using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Database")]
    public UpgradeDatabase database;

    [Header("UI")]
    public GameObject buyButton;           // Nút mua chung
    public TMPro.TMP_Text priceText;       // Hiển thị giá

    private UpgradeData currentUpgrade;     // Nâng cấp đang chọn
    private GameObject currentUpgradeCheckMark;

    private void Awake()
    {
        Instance = this;
    }


    public bool CheckHasPurchased(string upgradeId)
    {
        var upgrade = database.upgrades.Find(u => u.id == upgradeId);
        if (upgrade.purchased) 
        {
            return true;
        }
        return false;
    }


    // Gọi khi chọn một nút nâng cấp riêng lẻ
    public void SelectUpgrade( GameObject checkMark ,string upgradeId)
    {
        currentUpgrade = database.upgrades.Find(u => u.id == upgradeId);
        currentUpgradeCheckMark = checkMark;

        if (currentUpgrade.purchased)
        {
            buyButton.GetComponent<Button>().interactable = false;
            priceText.text = "0";
            return;
        }

        priceText.text = currentUpgrade.price.ToString();
        buyButton.GetComponent<Button>().interactable = true;

        RefreshBuyButtonState();
    }

    // Kiểm tra đủ tiền => sáng nút
    private void RefreshBuyButtonState()
    {
        buyButton.GetComponent<UnityEngine.UI.Button>().interactable =
            !currentUpgrade.purchased && GoldManager.Instance.Gold >= currentUpgrade.price;
    }

    public void BuyCurrentUpgrade()
    {
        if (currentUpgrade == null) return;
        if (currentUpgrade.purchased) return;
        if (GoldManager.Instance.Gold < currentUpgrade.price) return;

        // Trừ vàng và đánh dấu đã mua
        GoldManager.Instance.SpendGold(currentUpgrade.price);
        currentUpgrade.purchased = true;

        Cauldron.Instance.OnUpgrade(currentUpgrade.addItem, currentUpgrade.addHeat);


        // Ẩn nút mua
        buyButton.GetComponent<Button>().interactable = false;
        currentUpgradeCheckMark.SetActive(true);
        priceText.text = "0";
    }

    // ============================
    //           SAVE / LOAD
    // ============================

    [Serializable]
    private class UpgradeSaveWrapper
    {
        public List<bool> purchasedList;
        public int gold;
    }


    public void NewGame()
    {
        foreach (var u in database.upgrades)
        {
            u.purchased = false;
        }
    }

    public void SaveUpgrades()
    {
        UpgradeSaveWrapper data = new UpgradeSaveWrapper();
        data.purchasedList = new List<bool>();
        foreach (var u in database.upgrades)
            data.purchasedList.Add(u.purchased);


        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString("UPGRADE_DATA", json);
    }

    public void LoadUpgrades()
    {
        if (!PlayerPrefs.HasKey("UPGRADE_DATA")) return;

        string json = PlayerPrefs.GetString("UPGRADE_DATA");
        var data = JsonUtility.FromJson<UpgradeSaveWrapper>(json);

        for (int i = 0; i < database.upgrades.Count; i++)
        {
            database.upgrades[i].purchased = data.purchasedList[i];
        }

    }
}
