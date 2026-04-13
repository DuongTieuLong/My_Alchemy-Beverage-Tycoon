using UnityEngine;

public class TitleIdleAnimation : MonoBehaviour
{
    [Header("Position Idle")]
    [SerializeField] private float moveAmplitude = 10f;     // PX
    [SerializeField] private float moveSpeed = 1f;

    [Header("Scale Idle")]
    [SerializeField] private float scaleAmplitude = 0.02f;  // 2%
    [SerializeField] private float scaleSpeed = 0.7f;

    private RectTransform rt;
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;

    private void Start()
    {
        rt = GetComponent<RectTransform>();

        startPos = rt.anchoredPosition;
        startRot = rt.localRotation;
        startScale = rt.localScale;
    }

    private void Update()
    {
        float t = Time.time;

        // ---- Position ----
        float posOffset = Mathf.Sin(t * moveSpeed) * moveAmplitude;
        rt.anchoredPosition = startPos + new Vector3(0, posOffset,0);

        // ---- Scale ----
        float scaleOffset = 1 + Mathf.Sin(t * scaleSpeed) * scaleAmplitude;
        rt.localScale = startScale * scaleOffset;
    }
}
