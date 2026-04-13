using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class UIButtonPanelToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIAnimatorTool panelToToggle;   // Panel sẽ bật / tắt
    [SerializeField] private UIAnimatorTool panelToShowClosed; // Panel hiện khi đóng panel chính (nếu có)
    [SerializeField] private GameObject buttonToShowWhenClosed; // Nút hiện lại khi panel tắt 

    private Button button;


    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        // Toggle logic
        bool isPanelActive = panelToToggle.isVisible;

        if (!isPanelActive)
        {
            panelToToggle.Toggle();
            if (panelToShowClosed != null)
                panelToShowClosed.Toggle();
            if (buttonToShowWhenClosed != null)
                buttonToShowWhenClosed.gameObject.SetActive(false);
        }
        else
        {
            panelToToggle.Toggle();

            if (panelToShowClosed != null)
                panelToShowClosed.Toggle();
            if (buttonToShowWhenClosed != null)
                buttonToShowWhenClosed.gameObject.SetActive(true);
        }
    }

    public void CloseWithSafe()
    {
        if (panelToToggle.isVisible)
        {
            panelToToggle.Toggle();

            if (panelToShowClosed != null)
                panelToShowClosed.Toggle();
            if (buttonToShowWhenClosed != null)
                buttonToShowWhenClosed.gameObject.SetActive(true);
        }
    }

}
