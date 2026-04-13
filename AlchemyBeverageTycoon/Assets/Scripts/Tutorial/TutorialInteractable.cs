using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class TutorialInteractable : MonoBehaviour
{
    [Tooltip("Unique ID cho hành động này, ví dụ 'OpenInventory', 'CraftButton'")]
    public string triggerID;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        TutorialSignal.Emit(triggerID);
    }

    private void OnDestroy()
    {
        if (btn != null) btn.onClick.RemoveListener(OnClicked);
    }
}
