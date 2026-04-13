using UnityEngine;

public class RegionAligner : MonoBehaviour
{
    public RectTransform mapRect;
    public RectTransform regionRect;
    public bool convertToSquare = false;


    public Vector2 normalizedPos;
    public float sceneWidth= 1600f;
    public float sceneHeight= 1080f;

    private void Start()
    {
        Vector2 anchored = new Vector2(
           (normalizedPos.x - 0.5f) * mapRect.rect.width,
           (normalizedPos.y - 0.5f) * mapRect.rect.height
       );
        regionRect.anchoredPosition = anchored;

        if (convertToSquare)
        {
            float sizeScaleX = mapRect.rect.width / sceneWidth;
            float sizeScaleY = mapRect.rect.height / sceneHeight;
            if (sizeScaleX < sizeScaleY)
            {
                Vector3 scaleConfig = new Vector3(sizeScaleX, sizeScaleX, 1f);
                regionRect.localScale = scaleConfig;
            }
            else
            {
                Vector3 scaleConfig = new Vector3(sizeScaleY, sizeScaleY, 1f);
                regionRect.localScale = scaleConfig;
            }
        }
        else
        {
            float sizeScaleX = mapRect.rect.width / sceneWidth;
            float sizeScaleY = mapRect.rect.height / sceneHeight;
            Vector3 scaleConfig = new Vector3(sizeScaleX, sizeScaleY, 1f);
            regionRect.localScale = scaleConfig;
        }


    }
}
