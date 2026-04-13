using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// Gắn vào Button để tạo hiệu ứng nảy + âm thanh khi click.
/// Hỗ trợ DOTween animation.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIBtnFXTool : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("💥 Animation Settings")]
    [Range(0.8f, 1f)] public float pressedScale = 0.9f;
    public float bounceDuration = 0.2f;
    public Ease bounceEase = Ease.OutBack;

    [Header("🎵 Sound Settings")]
    public AudioClip clickSound;

    [Header("⚙️ Optional")]
    public bool disableButtonDuringAnim = false;

    private Button btn;
    private Tween scaleTween;
    private Vector3 originalScale;

    void Start()
    {
        btn = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (scaleTween != null) scaleTween.Kill();
        scaleTween = transform.DOScale(originalScale * pressedScale, bounceDuration * 0.5f).SetEase(Ease.OutSine);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleTween != null) scaleTween.Kill();
        scaleTween = transform.DOScale(originalScale, bounceDuration).SetEase(bounceEase);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();

        if (disableButtonDuringAnim)
            btn.interactable = false;

        // Nảy lại lần nữa cho cảm giác click thực
        if (scaleTween != null) scaleTween.Kill();
        transform.localScale = originalScale;
        scaleTween = transform
            .DOScale(originalScale * 0.95f, bounceDuration * 0.3f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                transform.DOScale(originalScale, bounceDuration * 0.5f).SetEase(bounceEase);
                if (disableButtonDuringAnim)
                    DOVirtual.DelayedCall(bounceDuration, () => btn.interactable = true);
            });
    }

    private void PlayClickSound()
    {
      AudioManager.Instance.PlayButtonSFX(clickSound);
    }
}
