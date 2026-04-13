using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button loadGameButton;
    public GameObject mainMenuCamera;
    public UIAnimatorTool mainMenuPanel;
    public UIAnimatorTool mainPanel;
    public UIAnimatorTool timeGoldRepPanel;
    public UIAnimatorTool tutorialPanel;
    public List<UIAnimatorTool> needToHide = new List<UIAnimatorTool>();

    private Sequence seq;

    private bool isTutorialMode = false;

    public void Start()
    {
        ShowMainUI();
    }

    public void ShowMainUI()
    {
        // Đảm bảo sequence cũ luôn bị hủy
        if (seq != null && seq.IsActive())
            seq.Kill();

        // Tạo sequence mới
        seq = DOTween.Sequence();

        // Kiểm tra load game
        loadGameButton.interactable = GameSaveManager.Instance.CheckHasFileLoad();

        // Reset trạng thái
        NPCManager.Instance.ResetOnNewPeriod(TimePeriod.Night);
        TimeCycleManager.Instance.SetPeriod(TimePeriod.None);

        mainMenuCamera.SetActive(true);

        tutorialPanel.HideIfShow();

        mainPanel.HideIfShow();
        timeGoldRepPanel.HideIfShow();

        foreach (UIAnimatorTool tool in needToHide)
            tool.HideIfShow();

        // --- Sequence ---
        seq.AppendCallback(() =>
        {
            // Có thể đặt logic chuẩn bị nếu cần
        });

        seq.AppendInterval(1f);

        seq.AppendCallback(() =>
        {
            mainMenuPanel.Show();
        });
    }

    private void UIStartGame()
    {
        // Hủy sequence cũ
        if (seq != null && seq.IsActive())
            seq.Kill();

        // Tạo sequence mới
        seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            mainMenuCamera.SetActive(false);
            mainMenuPanel.Hide();
        });

        seq.AppendInterval(2f);

        seq.AppendCallback(() =>
        {
            mainPanel.Show();
            timeGoldRepPanel.Show();
            if (isTutorialMode)
            {
                tutorialPanel.ShowIfHide();
            }
        });
    }


    public void NewGame()
    {
        isTutorialMode = true;
        UIStartGame();
        GameSaveManager.Instance.NewGame();
        TimeCycleManager.Instance.StartGame();
        Leaderboard.Instance.NewGame();
        TutorialManager.Instance.StartTutorial();
        ReputationManager.Instance.AddReputation(0);
    }
    public void LoadGame()
    {
        isTutorialMode = false;
        UIStartGame();
        GameSaveManager.Instance.LoadGame();
        Leaderboard.Instance.LoadGame();
    }
}
