using UnityEngine;

public class FpsSettings : MonoBehaviour
{
    public enum FpsMode
    {
        Auto,
        Fixed60
    }

    [Header("FPS Mode")]
    public FpsMode mode = FpsMode.Auto;

    void Start()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        switch (mode)
        {
            case FpsMode.Auto:
                // Unity tự điều chỉnh, không giới hạn FPS
                Application.targetFrameRate = -1;
                QualitySettings.vSyncCount = 0;
                break;

            case FpsMode.Fixed60:
                // Khóa 60 FPS
                Application.targetFrameRate = 60;
                QualitySettings.vSyncCount = 0;
                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
