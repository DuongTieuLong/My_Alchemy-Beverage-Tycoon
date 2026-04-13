using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionUI : MonoBehaviour
{
    public static ExpeditionUI Instance { get; private set; }
    #region === Serialized Fields ===

    [Header("Managers & Systems")]
    public InventoryManager inventoryManager;
    public PotionInventoryUIManager potionInventory;
    public ExpeditionTeamUI expeditionTeamUI;
    public ExpeditionCompleteUI expeditionCompleteUI;

    [Header("Expedition Data")]
    public List<ExpeditionData> expeditions = new();
    public List<Button> buttons = new();
    public List<ExpeditionMilitary> expeditionMilitaries = new();

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI expeditionTitleText;
    [SerializeField] private TextMeshProUGUI expeditionDescriptionText;
    [SerializeField] private TextMeshProUGUI expeditionLevelDifficultyText;
    [SerializeField] private TextMeshProUGUI requiredPowerText;
    [SerializeField] private TextMeshProUGUI timetoExplore;
    [SerializeField] private TextMeshProUGUI successRateText;
    [SerializeField] private TextMeshProUGUI noityfication;
    [SerializeField] private UIAnimatorTool InfoRegionTF;

    [Header("Buttons")]
    public Button selectMilitaryBtn;
    public Button startBtn;

    [Header("Expedition Military")]
    public Transform currentExpeditionMilitaryTranform;
    public Sprite defaultMilitaryIcon;

    [Header("Potions")]
    public Transform potionCarriedParent;
    public Transform potionCarriedContent;
    public Sprite defauth;

    [Header("Possible Loots")]
    public Transform possibleItemsToCollectContent;
    public GameObject possibleItemsToCollectPrefab;

    [Header("Expedition Markers")]
    public GameObject markerExpeditionPrefab;

    #endregion

    #region === Private Fields ===

    private ExpeditionData currentExpeditionData;
    private ExpeditionMilitary currentExpeditionMilitary;

    private Dictionary<Button, ExpeditionData> buttonExpeditionMap = new();
    private List<PotionData> potionCarriedTemp = new();
    private List<PotionData> potionCarriedView = new();
    private List<GameObject> markerExpeditionList = new();

    private Button lastSelectMapBtn;

    #endregion

    #region === Unity Lifecycle ===

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (expeditions.Count != buttons.Count)
        {
            return;
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            var expedition = expeditions[i];
            buttonExpeditionMap[btn] = expedition;
            btn.onClick.AddListener(() => OnExpeditionButtonClicked(btn));
        }

    }

    private void Start()
    {
        startBtn.interactable = false;
        startBtn.onClick.AddListener(StartExpedition);
        ResetAll();
        TimeCycleManager.Instance.OnPeriodChanged += RefreshCurrentExpedionOnComplete;
    }

    #endregion

    #region === Expedition Control ===

    public void RefreshAfterLoad()
    {
        MarkerForExpedition();
    }

    private void StartExpedition()
    {

        currentExpeditionMilitary.isInExpedition = true;
        currentExpeditionData.isOnExpedition = true;
        currentExpeditionData.expeditionMilitary = currentExpeditionMilitary;

        CalculateSuccessRate(potionCarriedTemp);

        List<PotionData> carried = new(potionCarriedTemp);
        potionCarriedTemp.Clear();

        currentExpeditionData.maxLootItems = GetItemCollectByRank(currentExpeditionData.bassMaxLootItems);

        ExpeditionManager.Instance.StartExpedition(currentExpeditionData, carried);

        MarkerForExpedition();

        potionInventory.CloseUI();
        ResetAll();
        InfoRegionTF.HideIfShow();

        TutorialSignal.Emit("StartExpedition");
    }

    private void OnExpeditionButtonClicked(Button clickedButton)
    {
        if (!buttonExpeditionMap.TryGetValue(clickedButton, out ExpeditionData expedition))
        {
            return;
        }


        expeditionCompleteUI.uiAnimatorTool.HideIfShow();
        ResetAll();
        currentExpeditionData = expedition;

        InfoRegionTF.Show();

        UpdateExpeditionDetails(expedition);

        if (lastSelectMapBtn != null)
            lastSelectMapBtn.interactable = true;

        clickedButton.interactable = false;
        lastSelectMapBtn = clickedButton;

        selectMilitaryBtn.interactable = !expedition.isOnExpedition;
        noityfication.text = expedition.isOnExpedition ? "In Expedition" : "Select Team & Potion";

        if (expedition.isOnExpedition)
            RefreshUIOnExpedition(expedition);
        else
            EnablePotionInteraction(true);
    }

    public void CloseInfoRegion()
    {
        InfoRegionTF.HideIfShow();
        if (lastSelectMapBtn != null)
            lastSelectMapBtn.interactable = true;

    }

    private void RefreshCurrentExpedionOnComplete(TimePeriod timePeriod)
    {
        if (currentExpeditionData == null) return;

        expeditionCompleteUI.uiAnimatorTool.HideIfShow();
        UpdateExpeditionDetails(currentExpeditionData);

        if (lastSelectMapBtn != null)
            lastSelectMapBtn.interactable = true;


        selectMilitaryBtn.interactable = !currentExpeditionData.isOnExpedition;
        noityfication.text = currentExpeditionData.isOnExpedition ? "In Expedition" : noityfication.text;

        if (currentExpeditionData.isOnExpedition)
            RefreshUIOnExpedition(currentExpeditionData);
        else
            EnablePotionInteraction(true);
    }

    private void OnCompleteExpedition()
    {
        var info = ExpeditionManager.Instance.CollectComplete(
            ExpeditionManager.Instance.GetExpeditionbyExpeditionData(currentExpeditionData)
        );

        potionInventory.animatorTool.HideIfShow();
        expeditionTeamUI.uIAnimatorTool.HideIfShow();

        expeditionCompleteUI.RefreshCompleteUI(info);
        ResetMarkers(currentExpeditionData.expeditionMilitary);
        ResetAll();
        InfoRegionTF.HideIfShow();
        currentExpeditionMilitary = null;
        currentExpeditionData = null;
    }

    #endregion

    #region === UI Refresh ===

    private void UpdateExpeditionDetails(ExpeditionData expedition)
    {


        expeditionTitleText.text = expedition ? expedition.expeditionTitle : "Region";
        expeditionDescriptionText.text = expedition ? expedition.description : "";
        expeditionLevelDifficultyText.text = expedition ? $"Difficulty: {expedition.levelDifficulty}" : "Difficulty: Unknown";
        requiredPowerText.text = expedition ? $"Required Power: {expedition.requiredPower}" : "Required Power: 0";
        timetoExplore.text = expedition ? $"Time to Explore: {expedition.durationInPeriods} Periods" : "Time to Explore: 0 Periods";
        successRateText.text = expedition ? $"Success Rate: {expedition.baseSuccessRate}%" : "Success Rate: 0%";

        foreach (Transform child in possibleItemsToCollectContent)
            Destroy(child.gameObject);

        if (expedition == null) return;

        foreach (ItemData item in expedition.possibleLoots)
        {
            var itemGO = Instantiate(possibleItemsToCollectPrefab, possibleItemsToCollectContent);
            itemGO.transform.Find("Icon").GetComponent<Image>().sprite = item.icon;
        }
    }

    private void RefreshUIOnExpedition(ExpeditionData data)
    {
        UpdateExpeditionDetails(data);

        var activeExpedition = ExpeditionManager.Instance.GetExpeditionbyExpeditionData(data);
        if (activeExpedition == null)
        {
            return;
        }

        timetoExplore.text = $"Time to Explore: {activeExpedition.remainingPeriods} Periods";
        AssignMilitaryToExpedition(data.expeditionMilitary.expeditionMilitaryName);
        selectMilitaryBtn.interactable = false;

        potionCarriedView = new(activeExpedition.carriedItems);
        RefreshCarriedPotionForInspect();
        successRateText.text = $"Success Rate: {activeExpedition.data.currentSuccessRate}%";

        EnablePotionInteraction(false);

        if (!activeExpedition.isComplete)
        {
            noityfication.text = "In Expedition";
            ConfigureStartButton("In Expedition", false, StartExpedition);
        }
        else
        {
            noityfication.text = "Expedition Complete!";
            ConfigureStartButton("Complete", true, OnCompleteExpedition);
        }
    }

    #endregion

    #region === Potions ===

    public void AddPotion(PotionData potion)
    {
        if (potionCarriedTemp.Count >= 5) return;
        inventoryManager.RemoveItem(potion, 1);
        potionCarriedTemp.Add(potion);
        CalculateSuccessRate(potionCarriedTemp);
        RefreshCarriedPotion();
        TutorialSignal.Emit("AddPotionToExpedition");
    }

    public void ReturnPotion(PotionData potion)
    {
        if (!potionCarriedTemp.Remove(potion)) return;
        inventoryManager.AddItem(potion, 1);
        CalculateSuccessRate(potionCarriedTemp);
        RefreshCarriedPotion();
    }

    public void ClearCarriedPotions()
    {
        for (int i = potionCarriedTemp.Count - 1; i >= 0; i--)
            ReturnPotion(potionCarriedTemp[i]);
        RefreshCarriedPotion();
    }

    private void RefreshCarriedPotion()
    {
        for (int i = 0; i < potionCarriedParent.childCount; i++)
        {
            var slot = potionCarriedParent.GetChild(i);
            var icon = slot.Find("Icon").GetComponent<Image>();
            var closeBtn = slot.Find("Close").GetComponent<Button>();

            if (i < potionCarriedTemp.Count)
            {
                var potion = potionCarriedTemp[i];
                icon.sprite = potion.icon;
                closeBtn.gameObject.SetActive(true);
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() => ReturnPotion(potion));
            }
            else
            {
                icon.sprite = defauth;
                closeBtn.gameObject.SetActive(false);
            }
        }
    }

    private void RefreshCarriedPotionForInspect()
    {
        for (int i = 0; i < potionCarriedParent.childCount; i++)
        {
            var slot = potionCarriedParent.GetChild(i);
            var icon = slot.Find("Icon").GetComponent<Image>();
            icon.sprite = i < potionCarriedView.Count ? potionCarriedView[i].icon : defauth;
        }
    }

    private void CalculateSuccessRate(List<PotionData> potionCarried)
    {
        if (currentExpeditionData == null) return;

        float baseRate = currentExpeditionData.baseSuccessRate;
        float totalBonus = 0f;

        foreach (var potion in potionCarried)
            totalBonus += potion.GetExpeditionBoost(currentExpeditionData.regionElement) * 100f;

        float finalRate = Mathf.Clamp(baseRate + totalBonus, 0f, 100f);
        successRateText.text = $"Success Rate: {finalRate}%";
        currentExpeditionData.currentSuccessRate = finalRate;
    }

    #endregion

    #region === Military ===

    public void AssignMilitaryToExpedition(string name)
    {
        currentExpeditionMilitary = expeditionMilitaries.Find(m => m.expeditionMilitaryName == name);
        if (currentExpeditionMilitary == null)
        {
            return;
        }

        CheckMilitaryPower();
        SetMilitaryUI();
    }

    private void CheckMilitaryPower()
    {
        if (currentExpeditionData == null || currentExpeditionMilitary == null) return;
        if (currentExpeditionMilitary.CalculatePower() >= currentExpeditionData.requiredPower)
        {
            noityfication.text = "Now, can explore!";
            startBtn.interactable = true;
        }
        else
        {
            noityfication.text = "Not enough power!";
            startBtn.interactable = false;
        }
    }

    public void RefreshCurrentTeamChossed()
    {
        CheckMilitaryPower();
        SetMilitaryUI();
    }

    private void SetMilitaryUI()
    {
        if (currentExpeditionMilitary == null) return;
        currentExpeditionMilitaryTranform.Find("Icon").GetComponent<Image>().sprite = currentExpeditionMilitary.icon;
        currentExpeditionMilitaryTranform.Find("Name").GetComponent<TextMeshProUGUI>().text = currentExpeditionMilitary.expeditionMilitaryName;
        currentExpeditionMilitaryTranform.Find("ATK").GetComponent<TextMeshProUGUI>().text = PowerConvert(currentExpeditionMilitary.CalculatePower());
    }

    private void ResetMilitary()
    {
        try
        {
            currentExpeditionMilitaryTranform.Find("Icon").GetComponent<Image>().sprite = defaultMilitaryIcon;
            currentExpeditionMilitaryTranform.Find("Name").GetComponent<TextMeshProUGUI>().text = "";
            currentExpeditionMilitaryTranform.Find("ATK").GetComponent<TextMeshProUGUI>().text = "";
        }
        catch
        {
        }
    }

    #endregion

    #region === Markers & Utility ===

    private void MarkerForExpedition()
    {

        foreach (var btn in buttons)
        {

            if (!buttonExpeditionMap.TryGetValue(btn, out var expeditionData))
                continue;

            if (!expeditionData.isOnExpedition)
                continue;

            var currentMilitary = expeditionData.expeditionMilitary;

            // Kiểm tra marker đã tồn tại chưa
            var existed = markerExpeditionList.Find(x =>
                x != null &&
                x.name == currentMilitary.expeditionMilitaryName
            );

            if (existed != null)
                continue;  // CHỈ bỏ qua btn này, KHÔNG return toàn hàm

            // Tạo marker mới
            GameObject marker = Instantiate(markerExpeditionPrefab, btn.transform);
            markerExpeditionList.Add(marker);

            marker.name = currentMilitary.expeditionMilitaryName;
            marker.transform.localPosition = new Vector3(0, 60, 0);

            marker.GetComponent<Image>().sprite = currentMilitary.icon;
            marker.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = currentMilitary.expeditionMilitaryName;
            marker.transform.Find("ATK").GetComponent<TextMeshProUGUI>().text = PowerConvert(currentMilitary.totalPower);
        }
    }
    public void ClearMarkersIfNoExpedition()
    {
        // Kiểm tra xem có expedition nào đang hoạt động không
        bool hasAnyExpedition =
            buttonExpeditionMap.Values.Any(data => data != null && data.isOnExpedition);

        if (hasAnyExpedition)
            return;

        // Nếu không có expedition nào → xoá toàn bộ marker
        for (int i = markerExpeditionList.Count - 1; i >= 0; i--)
        {
            var marker = markerExpeditionList[i];
            if (marker != null)
                Destroy(marker);

            markerExpeditionList.RemoveAt(i);
        }
    }


    private void ResetMarkers(ExpeditionMilitary military)
    {
        var toRemove = markerExpeditionList.FindAll(m => m != null && m.name == military.expeditionMilitaryName);
        foreach (var marker in toRemove)
        {
            markerExpeditionList.Remove(marker);
            Destroy(marker);
        }
    }

    private string PowerConvert(float totalPower)
    {
        if (totalPower >= 1_000_000_000) return (totalPower / 1_000_000_000f).ToString("0.###") + "B";
        if (totalPower >= 1_000_000) return (totalPower / 1_000_000f).ToString("0.###") + "M";
        if (totalPower >= 1_000) return (totalPower / 1_000f).ToString("0.###") + "K";
        return totalPower.ToString("0");
    }

    public void RefreshExpeditonTeam()
    {
        foreach (var expMilitary in expeditionMilitaries)
            expeditionTeamUI.RefreshUI(expMilitary);

    }

    private void ConfigureStartButton(string label, bool interactable, UnityEngine.Events.UnityAction onClick)
    {
        startBtn.GetComponentInChildren<TextMeshProUGUI>().text = label;
        startBtn.interactable = interactable;
        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(onClick);
        startBtn.onClick.AddListener(() => { TutorialSignal.Emit("OnComplete"); });
    }

    private void EnablePotionInteraction(bool enable)
    {
        foreach (Transform child in potionCarriedContent)
        {
            var btn = child.GetComponent<Button>();
            if (btn) btn.interactable = enable;
        }
    }

    #endregion

    #region === Reset & Close ===

    public void ResetAll()
    {
        ConfigureStartButton("Start Expedition", false, StartExpedition);

        if (lastSelectMapBtn != null)
            lastSelectMapBtn.interactable = true;
        lastSelectMapBtn = null;

        selectMilitaryBtn.interactable = false;
        EnablePotionInteraction(false);

        noityfication.text = "Select Region";
        UpdateExpeditionDetails(null);
        ClearCarriedPotions();
        ResetMilitary();
        RefreshExpeditonTeam();
        CheckMilitaryPower();

        expeditionTeamUI.uIAnimatorTool.HideIfShow();
        potionInventory.animatorTool.HideIfShow();
    }

    public void OnClose()
    {

        currentExpeditionData = null;
        currentExpeditionMilitary = null;

        if (lastSelectMapBtn != null)
        {
            lastSelectMapBtn.interactable = true;
            lastSelectMapBtn = null;
        }

        selectMilitaryBtn.interactable = false;

        ResetMilitary();
        ClearCarriedPotions();
        UpdateExpeditionDetails(null);
        EnablePotionInteraction(false);

        ConfigureStartButton("Start Expedition", false, StartExpedition);
        noityfication.text = "Select Region";

        foreach (Transform child in possibleItemsToCollectContent)
            Destroy(child.gameObject);

        expeditionCompleteUI?.uiAnimatorTool.HideIfShow();
        potionInventory?.CloseUI();
        expeditionTeamUI.uIAnimatorTool.HideIfShow();

    }

    #endregion

    public void OpenPotionInventory()
    {
        potionInventory.animatorTool.Show();
        expeditionTeamUI.uIAnimatorTool.HideIfShow();
    }
    public void OpenExpeditionTeamUI()
    {
        potionInventory.animatorTool.HideIfShow();
        expeditionTeamUI.Show();
    }

    public int GetItemCollectByRank(int currentMaxLoots)
    {
        int maxCount = 0;
        var currentRank = ReputationManager.Instance.currentRank;

        float mutiply =
        currentRank switch
        {
            Rank.Apprentice => 1,
            Rank.Adept => 1.2f,
            Rank.Expert => 1.5f,
            Rank.Master => 1.8f,
            Rank.Grandmaster => 2f,
            _ => 1
        };

        maxCount = (int)mutiply * currentMaxLoots;
        return maxCount;
    }
}
