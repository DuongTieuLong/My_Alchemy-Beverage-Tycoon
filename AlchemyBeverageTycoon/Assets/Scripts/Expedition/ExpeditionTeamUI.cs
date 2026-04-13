using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ExpeditionTeamUI : MonoBehaviour
{
    public ExpeditionUI expeditionUI;
    public List<Transform> expeditionTeamPanel = new List<Transform>();
    public UIAnimatorTool uIAnimatorTool;

    [Header("Panels")]
    public UIAnimatorTool unlockPanel;
    public UIAnimatorTool recruitPanel;

    public TextMeshProUGUI notificationText;

    // =============================
    // REFRESH UI MỖI ĐỘI QUÂN
    // =============================
    public void RefreshUI(ExpeditionMilitary data)
    {
        Transform expTeam = expeditionTeamPanel.Find(x => x.name == data.expeditionMilitaryName);
        if (expTeam == null) return;

        // ---- Basic info ----
        expTeam.Find("ExpeditionMilitaryName").GetComponent<TextMeshProUGUI>().text = data.expeditionMilitaryName;
        expTeam.Find("TroopCount").GetComponent<TextMeshProUGUI>().text = $"{data.troopCount} / {data.maxTroopCapacity}";
        expTeam.Find("Power").GetComponent<TextMeshProUGUI>().text = data.CalculatePower().ToString();

        // ---- Select Button ----
        var selectBtn = expTeam.Find("Select").GetComponent<Button>();
        var selectText = selectBtn.GetComponentInChildren<TextMeshProUGUI>();
        selectBtn.onClick.RemoveAllListeners();

        if (!data.isUnlocked)
        {
            selectText.text = "Locked";
            selectBtn.interactable = false;
        }
        else if (data.isInExpedition)
        {
            selectText.text = "In Expedition";
            selectBtn.interactable = false;
        }
        else
        {
            selectText.text = "Select";
            selectBtn.interactable = true;
            selectBtn.onClick.AddListener(() =>
            {
                TutorialSignal.Emit("SelectTeam1");
                expeditionUI.AssignMilitaryToExpedition(data.expeditionMilitaryName);
                uIAnimatorTool.Hide();
            });
        }

        // ---- Unlock Condition ----
        if (!data.isUnlocked &&
            data.expeditionRequired <= ReputationManager.Instance.currentRank)
        {
            selectBtn.interactable = true;
            selectText.text = "Unlock";
            selectBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.AddListener(() => { OpenUnlockPanel(data); TutorialSignal.Emit("ClickUnlockTeam1"); });
        }

        // ---- Recruit Button ----
        var recruitBtn = expTeam.Find("Recruit").GetComponent<Button>();
        recruitBtn.onClick.RemoveAllListeners();

        if (data.isUnlocked && !data.isInExpedition && data.troopCount < data.maxTroopCapacity)
        {
            recruitBtn.interactable = true;
            recruitBtn.onClick.AddListener(() =>
            {
                OpenRecruitPanel(data);
                TutorialSignal.Emit("SelectRecruit");
            }
            );


        }
        else
        {
            recruitBtn.interactable = false;
        }
    }

    // =============================
    // UNLOCK PANEL
    // =============================
    public void OpenUnlockPanel(ExpeditionMilitary data)
    {
        unlockPanel.Show();

        unlockPanel.transform.Find("Gold").GetComponent<TextMeshProUGUI>().text =
            data.valueToUnlock.ToString();

        var btn = unlockPanel.transform.Find("Unlock").GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        bool canUnlock = GoldManager.Instance.Gold >= data.valueToUnlock;
        btn.interactable = canUnlock;

        if (!canUnlock) return;

        btn.onClick.AddListener(() =>
        {
            if (GoldManager.Instance.SpendGold(data.valueToUnlock))
            {
                data.isUnlocked = true;
                unlockPanel.Hide();
                RefreshUI(data);
                TutorialSignal.Emit("BuyTeam1");
            }
        });
    }

    // =============================
    // RECRUIT PANEL
    // =============================
    public void OpenRecruitPanel(ExpeditionMilitary data)
    {
        recruitPanel.Show();

        // Tìm button đúng vị trí UI
        var weakBtn = recruitPanel.transform.Find("Expedition Team 1").Find("RecruitWeak").GetComponent<Button>();
        var mercBtn = recruitPanel.transform.Find("Expedition Team 2").Find("RecruitMerc").GetComponent<Button>();
        var eliteBtn = recruitPanel.transform.Find("Expedition Team 3").Find("RecruitElite").GetComponent<Button>();

        // Xóa listener cũ
        weakBtn.onClick.RemoveAllListeners();
        mercBtn.onClick.RemoveAllListeners();
        eliteBtn.onClick.RemoveAllListeners();

        // ========== KIỂM TRA GOLD TRƯỚC ==========
        int gold = GoldManager.Instance.Gold;

        // Weak (100 gold)
        weakBtn.interactable = gold >= 100;
        if (weakBtn.interactable)
            weakBtn.onClick.AddListener(() => { Recruit(data, 500, 1, 100); TutorialSignal.Emit("Recruit1"); });
        weakBtn.GetComponentInChildren<TextMeshProUGUI>().text = "500";

        // Merc (300 gold)
        mercBtn.interactable = gold >= 300;
        if (mercBtn.interactable)
            mercBtn.onClick.AddListener(() => Recruit(data, 1000, 100, 300));
        mercBtn.GetComponentInChildren<TextMeshProUGUI>().text = "1000";

        // Elite (500 gold)
        eliteBtn.interactable = gold >= 500;
        if (eliteBtn.interactable)
            eliteBtn.onClick.AddListener(() => Recruit(data, 2000, 300, 500));
        eliteBtn.GetComponentInChildren<TextMeshProUGUI>().text = "2000";
    }

    // =============================
    // RECRUIT LOGIC
    // =============================
    private void Recruit(ExpeditionMilitary data, int cost, int minAdd, int maxAdd)
    {
        if (GoldManager.Instance.Gold < cost)
            return;

        if (!GoldManager.Instance.SpendGold(cost))
            return;

        int addTroop = 0;

        if (TutorialManager.Instance.isOnTutorial)
        {
            addTroop += 100;
            data.troopCount += addTroop;
            data.troopCount = Mathf.Min(data.troopCount, data.maxTroopCapacity);
            TutorialManager.Instance.isOnTutorial = false;
        }
        else
        {
            addTroop = Random.Range(minAdd, maxAdd + 1);
            data.troopCount += addTroop;
            data.troopCount = Mathf.Min(data.troopCount, data.maxTroopCapacity);
        }


        recruitPanel.Hide();
        RefreshUI(data);
        expeditionUI.RefreshCurrentTeamChossed();

        // ============================
        // DOTWEEN NOTIFICATION
        // ============================
        notificationText.text = $"{data.expeditionMilitaryName} recruited +{addTroop} troops";

        // Hủy tween cũ nếu đang chạy (tránh chồng hiệu ứng)
        notificationText.DOKill();

        // Bắt đầu từ trong suốt
        notificationText.alpha = 0f;

        notificationText
            .DOFade(1f, 0.5f)          // Fade in 0.5s
            .OnComplete(() =>
            {
                notificationText
                    .DOFade(0f, 0.5f)  // Fade out 0.5s
                    .SetDelay(2.5f);   // Chờ 1.5s
            });
    }
    public void Show()
    {
        uIAnimatorTool.Show();
    }


}
