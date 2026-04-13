using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public UIAnimatorTool shopPanel;
    public PotionSellUI potionSellUI;
    public UIAnimatorTool mainCanvas;

    public InventoryManager inventoryManager;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI npcRequestText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI totalGoldText;
    public Transform bagContent;

    public NPCRequest currentNPC;
    public int totalGoldCount;

    public GameObject sellRefuseButton;
    public GameObject confirmButton;
    private Button sellButton;

    public int maxBagPotion;
    public List<PotionData> npcBagPotion = new List<PotionData>();
    public GameObject potionPrefabs;

    public bool isOnConfirm = false;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GoldTotal();
        sellButton = sellRefuseButton.transform.Find("Sell").GetComponent<Button>();
        RefreshStateSellButton();
    }

    public void NpcRefresh(NPCRequest npcRequest)
    {
        mainCanvas.HideIfShow();
        shopPanel.Show();

        currentNPC = npcRequest;
        npcNameText.text = npcRequest.name;
        npcRequestText.text = npcRequest.requestText;
        quantityText.text = $"Quatity: {npcRequest.potionCountRequest} potion";
        maxBagPotion = npcRequest.potionCountRequest;

        TutorialSignal.Emit("FirstCustomer");
    }

    public void AddPotionToNpcBag(PotionData potionData)
    {
        if(isOnConfirm) return;
        if (npcBagPotion.Count == maxBagPotion)
        {
            SwapPotion(potionData);
            return;
        }
        if (potionData == null) return;
        npcBagPotion.Add(potionData);
        inventoryManager.RemoveItem(potionData, 1);
        RefreshPotoionInBag();
    }

    public void SwapPotion(PotionData potionData)
    {
        inventoryManager.AddItem(npcBagPotion[0], 1);
        npcBagPotion[0] = potionData;
        inventoryManager.RemoveItem(potionData, 1);
        RefreshPotoionInBag();
    }

    public void RefreshPotoionInBag()
    {
        foreach (Transform child in bagContent)
            Destroy(child.gameObject);

        foreach (var potion in npcBagPotion)
        {
            GameObject newPotion = GameObject.Instantiate(potionPrefabs, bagContent);
            newPotion.GetComponent<Button>().interactable = false;
            newPotion.transform.Find("Icon").GetComponent<Image>().sprite = potion.icon;
            var btn = newPotion.transform.Find("Close").GetComponent<Button>();
            btn.gameObject.SetActive(true);
            btn.onClick.AddListener(() => RemovePotionBag(potion));
        }

        GoldTotal();
        RefreshStateSellButton();
    }

    public void RemovePotionBag(PotionData potionData)
    {
        if(isOnConfirm) return;
        inventoryManager.AddItem(potionData, 1);
        npcBagPotion.Remove(potionData);
        RefreshPotoionInBag();
    }

    public void RefreshStateSellButton()
    {
        if (currentNPC != null)
        {
            if (npcBagPotion.Count == maxBagPotion)
                sellButton.interactable = true;
            else
                sellButton.interactable = false;
        }
        else
        {
            sellButton.interactable = false;
        }
    }

    public void GoldTotal()
    {
        totalGoldCount = 0;
        foreach (var potion in npcBagPotion)
        {
            totalGoldCount += PriceCalculator.GetFinalPrice(potion);
        }
        totalGoldText.text = totalGoldCount.ToString();
    }

    public void Refuse()
    {
        npcRequestText.text = $"{currentNPC.responeBadText} \nPenalty: <color=#FF6347>-{(int)(currentNPC.penaltyReputation * 0.2f)} Rep.</color>";
        sellRefuseButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(true);
        isOnConfirm = true;
        ReputationManager.Instance?.AddReputation(-(int)(currentNPC.penaltyReputation * 0.2f));

    }

    public bool isGoodTrade = false;

    public void Sell()
    {
        foreach (var potion in npcBagPotion)
        {
            isGoodTrade = NPCRequestManager.TryFulfillRequest(inventoryManager, potion, currentNPC);
            if (isGoodTrade)
            {
                int totalGold = totalGoldCount + currentNPC.rewardGold;
                string message = $"{currentNPC.npcName}: {currentNPC.responeGoodText}\n" +
                      $"<color=#00FF00>{currentNPC.npcName} accepted the {potion.itemName}.</color>\n" +
                      $"Gold of Potion: <color=#FFD700>+{totalGoldCount} Gold</color>." +
                      $"\nRewards: <color=#FFD700>+{currentNPC.rewardGold} Gold</color>, " +
                      $"<color=#00BFFF>+{currentNPC.rewardReputation} Rep.</color>" +
                      $"\nTotal Gold: <color=#FFD700> + {totalGold} Gold.</color>";
                npcRequestText.text = message;

                totalGoldText.text = $"{totalGold}";
                GoldManager.Instance.AddGold(totalGold);
            }
            else
            {
                string message = $"{currentNPC.npcName}: {currentNPC.responeBadText}\n" +
                            $"<color=red>{currentNPC.npcName} rejected the potion.</color> " +
                            $"\nPenalty: <color=#FF6347>-{currentNPC.penaltyReputation} Rep.</color>";

                npcRequestText.text = message;
                break;
            }


        }
        sellRefuseButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(true);
        isOnConfirm = true;
        currentNPC = null;
        maxBagPotion = 0;
        totalGoldCount = 0;
    }

    public void Confirm()
    {
 
        if (isGoodTrade)
        {
            npcBagPotion.Clear();
            TutorialSignal.Emit("FirstGoodTrade");
        }
        else
        {
            foreach (var potion in npcBagPotion)
            {
                inventoryManager.AddItem(potion);
            }
            npcBagPotion.Clear();
        }

        RefreshPotoionInBag();
        sellRefuseButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(false);
        isOnConfirm = false;
        mainCanvas.Show();

        potionSellUI.selectedSlot = null;
        potionSellUI.detailPanel.Hide();
    }
    public void EndOfDay()
    {
        if (currentNPC != null)
        {
            npcBagPotion.Clear();
        }

        if (shopPanel.isVisible)
            mainCanvas.ShowIfHide();
        shopPanel.HideIfShow();

        potionSellUI.selectedSlot = null;
        potionSellUI.detailPanel.Hide();

        RefreshPotoionInBag();
        sellRefuseButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(false);
    }
}

