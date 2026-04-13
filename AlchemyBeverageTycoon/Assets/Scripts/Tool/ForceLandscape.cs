using UnityEngine;

public class ForceLandscape : MonoBehaviour
{
    void Awake()
    {
        // Cho phép landscape trái/phải
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Tắt portrait
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // Bật chế độ tự xoay
        Screen.orientation = ScreenOrientation.AutoRotation;
    }
}
