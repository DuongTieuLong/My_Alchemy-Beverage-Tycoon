
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionCompleteUI : MonoBehaviour
{
    public GameObject itemUIPrefabs;

    public GameObject ResultSuccess;
    public GameObject ResultFail;

    public TextMeshProUGUI description;
    public TextMeshProUGUI expeditionName;
    public TextMeshProUGUI originTroops;
    public TextMeshProUGUI lostTroops;
    public TextMeshProUGUI currentTroops;

    public Transform collectedItemsContent;
    public Dictionary<ItemData, int> itemCollectedDict = new Dictionary<ItemData, int>();

    public UIAnimatorTool uiAnimatorTool;
    private void Start()
    {
        uiAnimatorTool = GetComponent<UIAnimatorTool>();
    }

    public void RefreshCompleteUI(CompleteExpeditionInfo expInfo)
    {
        itemCollectedDict.Clear();
        uiAnimatorTool.Show();

        ResultSuccess.SetActive(expInfo.result);
        ResultFail.SetActive(!expInfo.result);

        if (expInfo.result)
            description.text = GetDescriptWinByRegion(expInfo.data.expeditionTitle);
        else
            description.text = GetDescriptLossByRegion(expInfo.data.expeditionTitle);

        var expMilitary = expInfo.data.expeditionMilitary;

        expeditionName.text = expMilitary.expeditionMilitaryName;
        originTroops.text = "Origin Troops: " + expInfo.curentTroops;
        lostTroops.text = "Lost Troops: " + expInfo.troopsLost;
        currentTroops.text = "Current Troops: " + expMilitary.troopCount;

        foreach (Transform child in collectedItemsContent)
        {
            Destroy(child.gameObject);
        }

        if (expInfo.collectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in expInfo.collectedItems)
        {
            itemCollectedDict[item] = itemCollectedDict.ContainsKey(item) ? itemCollectedDict[item] + 1 : 1;
        }
        foreach (var kvp in itemCollectedDict)
        {
            GameObject itemUIObj = Instantiate(itemUIPrefabs, collectedItemsContent);
            itemUIObj.GetComponent<Image>().sprite = kvp.Key.icon;
            itemUIObj.GetComponentInChildren<TextMeshProUGUI>().text = "x" + kvp.Value;
        }

    }

    public string GetDescriptWinByRegion(string region)
    {
        switch (region)
        {
            case "Greenwood Forest":
                return "Your squad returns covered in leaves but smiling — the forest spirits must’ve liked them!\nYour team suffered minor losses.";
            case "Sunny Plains":
                return "The sun shines bright on your victorious crew! Even the wild bunnies salute their bravery!\nYour team suffered minor losses.";
            case "Blackmist Swamp":
                return "They emerged from the fog smelling terrible, but rich and triumphant — a fair trade!\nYour team suffered minor losses.";
            case "Everfrost Cavern":
                return "They came back half-frozen but glittering with frost crystals and pride!\nYour team suffered minor losses.";
            case "Crimson Volcano":
                return "The ground still smokes beneath their boots — but the loot is hotter than lava!\nYour team suffered minor losses.";
            case "Spirit Grove":
                return "Your team returned whispering tales of dancing spirits and shimmering treasures!\nYour team suffered minor losses.";
            case "Sunscar Desert":
                return "They survived the heat, the sandstorms, and maybe a few mirages — what a blazing success!\nYour team suffered minor losses.";
            default:
                return "The expedition returned with glory and great stories to tell!\nYour team suffered minor losses.";
        }
    }

    public string GetDescriptLossByRegion(string region)
    {
        switch (region)
        {
            case "Greenwood Forest":
                return "They got lost chasing glowing mushrooms... again. The forest wins this round.\nYour team suffered very heavy losses.";
            case "Sunny Plains":
                return "Too much sun, not enough luck — your team needs some shade and a lemonade.\nYour team suffered very heavy losses.";
            case "Blackmist Swamp":
                return "They sank faster than their morale — next time, maybe fewer leeches.\nYour team suffered very heavy losses.";
            case "Everfrost Cavern":
                return "They froze halfway through a victory cheer. Maybe pack extra socks next time.\nYour team suffered very heavy losses.";
            case "Crimson Volcano":
                return "Turns out lava hurts. Who knew? They’ll need a long, *cool* vacation.\nYour team suffered very heavy losses.";
            case "Spirit Grove":
                return "The spirits were not impressed... they sent your crew home with ghostly giggles.\nYour team suffered very heavy losses.";
            case "Sunscar Desert":
                return "The desert swallowed them whole — or maybe they just fell asleep under a cactus.\nYour team suffered very heavy losses.";
            default:
                return "The expedition failed, but hey, they came back with good excuses!\nYour team suffered very heavy losses.";
        }
    }

}
