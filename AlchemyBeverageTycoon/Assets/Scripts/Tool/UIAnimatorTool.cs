using UnityEngine;
using DG.Tweening;

/// <summary>
/// Gắn vào Panel / CanvasGroup để bật tắt với animation DOTween
/// Gọi Toggle(), Show(), Hide()
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIAnimatorTool : MonoBehaviour
{
    public enum UIAnimType
    {
        ScaleFade,
        Slide
    }

    [Header("✨ Animation Type")]
    public UIAnimType animType = UIAnimType.ScaleFade;
    public bool hideOnStart = true;

    [Header("🎛 Common Settings")]
    public float duration = 0.4f;
    public Ease easeIn = Ease.OutBack;
    public Ease easeOut = Ease.InBack;

    [Header("🌟 Scale + Fade Settings")]
    [Range(0.5f, 1.5f)] public float closedScale = 0.8f;

    [Header("📦 Slide Settings")]
    public Vector2 slideOffset = new Vector2(0, -300f);
    public bool useFade = true;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    public bool isVisible = false;
    private Tween tween;

    Vector2 orriginPos;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        if (hideOnStart)
        {
            // Init trạng thái ban đầu
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isVisible = false;
        }
        else
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isVisible = true;
        }

        orriginPos = rectTransform.anchoredPosition;
    }

    public void Toggle()
    {
        if (isVisible) Hide();
        else Show();
    }

    public void HideIfShow()
    {
        if (isVisible)
        {
            Hide();
        }
    }
    public void ShowIfHide()
    {
        if (!isVisible) Show();
    }


    public void Show()
    {
        if (tween != null) tween.Kill();
        isVisible = true;

        gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        switch (animType)
        {
            case UIAnimType.ScaleFade:
                PlayScaleFade(show: true);
                break;
            case UIAnimType.Slide:
                PlaySlide(show: true);
                break;
        }
    }

    public void Hide()
    {
        if (tween != null) tween.Kill();
        isVisible = false;

        switch (animType)
        {
            case UIAnimType.ScaleFade:
                PlayScaleFade(show: false);
                break;
            case UIAnimType.Slide:
                PlaySlide(show: false);
                break;
        }
    }

    //───────────────────────────────────────
    #region Animation Implementations

    private void PlayScaleFade(bool show)
    {
        float startScale = show ? closedScale : 1f;
        float endScale = show ? 1f : closedScale;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;

        rectTransform.localScale = Vector3.one * startScale;
        canvasGroup.alpha = startAlpha;

        Sequence seq = DOTween.Sequence();

        seq.Join(rectTransform.DOScale(endScale, duration).SetEase(show ? easeIn : easeOut));
        seq.Join(canvasGroup.DOFade(endAlpha, duration * 0.9f).SetEase(show ? Ease.OutSine : Ease.InSine));

        tween = seq.OnComplete(() =>
        {
            if (!show)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        });
    }

    private void PlaySlide(bool show)
    {


        Vector2 startPos = show ? slideOffset : orriginPos;
        Vector2 endPos = show ? orriginPos : slideOffset;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;

        rectTransform.anchoredPosition = startPos;
        canvasGroup.alpha = useFade ? startAlpha : 1f;

        Sequence seq = DOTween.Sequence();

        seq.Join(rectTransform.DOAnchorPos(endPos, duration).SetEase(show ? easeIn : easeOut));
        if (useFade)
            seq.Join(canvasGroup.DOFade(endAlpha, duration * 0.9f));

        tween = seq.OnComplete(() =>
        {
            if (!show)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        });
    }

    #endregion
    //───────────────────────────────────────
}
