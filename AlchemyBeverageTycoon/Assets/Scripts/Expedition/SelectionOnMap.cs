using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectionOnMap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite borderSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image && borderSprite)
            image.sprite = borderSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image && normalSprite)
            image.sprite = normalSprite;
    }
}
