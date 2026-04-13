using UnityEngine;

public class UIControlWithLocation : MonoBehaviour
{
    public GameObject returnBrewButton;
    public GameObject returnMainUI;
    public UIButtonPanelToggle closeBrewButton;
    public UIAnimatorTool inventory;
    public UIAnimatorTool recipeBook;
    public UIAnimatorTool upgradePanel;
    public UIAnimatorTool researhPanel;
    public UIAnimatorTool researchCompletePanel;
    public UIAnimatorTool potionCompletePanel;

    public void RefreshUIWithLocation(PlayerLocation currentLocation)
    {
        switch (currentLocation)
        {
            case PlayerLocation.Shop:
                AtShop();
                break;
            case PlayerLocation.Brew:
                AtBrew();
                break;
            case PlayerLocation.Explore:
                break;
        }
    }

    public void AtShop()
    {
        returnBrewButton.SetActive(true);
        returnMainUI.SetActive(true);
        closeBrewButton.CloseWithSafe(); // nếu đang bật thì đống lại một cách an toàn
        inventory.HideIfShow();
        recipeBook.HideIfShow();
        upgradePanel.HideIfShow();
        researhPanel.HideIfShow();
        researchCompletePanel.HideIfShow();
        potionCompletePanel.HideIfShow();


    }
    public void AtBrew()
    {
        returnBrewButton.SetActive(false);
        returnMainUI.SetActive(false);
    }
    public void AtExplore()
    {

    }
}
