using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    public string upgradeId;
    public GameObject checkMark;

    void Start()
    {
        GameSaveManager.Instance.onNewGame += RefreshCheckMark;
        GameSaveManager.Instance.onLoadGame += RefreshCheckMark;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void RefreshCheckMark()
    {
        checkMark.SetActive(UpgradeManager.Instance.CheckHasPurchased(upgradeId));
    }

    void OnClick()
    {
        UpgradeManager.Instance.SelectUpgrade(checkMark, upgradeId);
    }
}
