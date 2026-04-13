using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public GameObject tutorialPanel;

    public List<TutorialStep> steps;
    public TMP_Text instructionText;
    public Button nextButton;
    public GameObject instructionPanel;

    public Transform buttonTranform;
    public Image buttonIcon;

    private int index = -1;
    private bool tutorialEnded = false;
    public bool isOnTutorial = false;
    private Coroutine autoRoutine;

    private void Awake()
    {
        Instance = this;
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnEnable()
    {
        TutorialSignal.OnSignal += OnSignalReceived;
    }

    private void OnDisable()
    {
        TutorialSignal.OnSignal -= OnSignalReceived;
    }

    public void StartTutorial()
    {
        tutorialEnded = false;
        isOnTutorial = true;
        index = -1;
        NextStep();
    }

    public void NextStep()
    {
        if (tutorialEnded) return;

        index++;
        if (index >= steps.Count)
        {
            EndTutorial();
            return;
        }

        var step = steps[index];

        // Panel
        instructionPanel.SetActive(!step.hidePanel);

        // Clear old button clone
        foreach (Transform c in buttonTranform)
            Destroy(c.gameObject);

        // Spawn new tutorial UI button
        if (step.button != null)
        {
            var OBJ = Instantiate(step.button, buttonTranform);
            var rect = OBJ.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector3.zero;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        // Icon
        if (step.buttonIcon != null)
        {
            buttonIcon.gameObject.SetActive(true);
            buttonIcon.sprite = step.buttonIcon;
        }
        else buttonIcon.gameObject.SetActive(false);

        // Text
        instructionText.text = step.instructionText;

        step.onStart?.Invoke();

        ConfigureNextButton(step);

        // stop auto
        if (autoRoutine != null)
            StopCoroutine(autoRoutine);

        // auto
        if (step.autoComplete)
            autoRoutine = StartCoroutine(AutoCompleteRoutine(step.autoDelaySeconds));
    }

    private void ConfigureNextButton(TutorialStep step)
    {
        bool showButton =
            step.useNextButton &&
            !step.waitForSignal &&
            !step.autoComplete;

        nextButton.gameObject.SetActive(showButton);
    }

    private IEnumerator AutoCompleteRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (tutorialEnded) yield break;
        if (index >= steps.Count) yield break;

        var step = steps[index];

        step.onComplete?.Invoke();
        NextStep();
    }

    private void OnSignalReceived(string signalID)
    {
        if (tutorialEnded) return;
        if (index < 0 || index >= steps.Count) return;


        var step = steps[index];

        if (!step.waitForSignal) return;
        if (!step.requiredSignals.Contains(signalID)) return;

        step.onComplete?.Invoke();
        NextStep();
    }

    private void OnNextButtonClicked()
    {
        if (tutorialEnded) return;
        if (index < 0 || index >= steps.Count) return;

        var step = steps[index];
        if (!step.useNextButton) return;

        step.onComplete?.Invoke();
        NextStep();
    }

    private void EndTutorial()
    {
        tutorialEnded = true;
        isOnTutorial = false;

        nextButton.gameObject.SetActive(false);
        instructionPanel.SetActive(false);
        tutorialPanel.SetActive(false);

        // Stop auto
        if (autoRoutine != null)
            StopCoroutine(autoRoutine);
    }

    // -----------------------------------------
    // 🔥 NEW: Skip toàn bộ tutorial
    // -----------------------------------------
    public void SkipTutorial()
    {
        if (tutorialEnded) return;

        steps[index].onComplete?.Invoke();

        tutorialEnded = true;
        isOnTutorial = false;
        index = steps.Count;

        // Tắt UI ngay
        nextButton.gameObject.SetActive(false);
        instructionPanel.SetActive(false);

        foreach (Transform c in buttonTranform)
            Destroy(c.gameObject);

        buttonIcon.gameObject.SetActive(false);

        if (autoRoutine != null)
            StopCoroutine(autoRoutine);

        tutorialPanel.SetActive(false);
    }
}
