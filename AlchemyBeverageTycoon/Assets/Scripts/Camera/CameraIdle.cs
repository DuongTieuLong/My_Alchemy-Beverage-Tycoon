using UnityEngine;

public class CameraIdle : MonoBehaviour
{
    [Header("Rotation Idle")]
    [SerializeField] private float rotationAmplitude = 1.5f;   // Biên độ xoay (độ)
    [SerializeField] private float rotationSpeed = 0.5f;        // Tốc độ dao động xoay

    [Header("Position Idle (optional)")]
    [SerializeField] private bool usePositionIdle = false;
    [SerializeField] private float positionAmplitude = 0.05f;   // Biên độ di chuyển
    [SerializeField] private float positionSpeed = 0.3f;        // Tốc độ di chuyển

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        float t = Time.time;

        // --- Rotation ---
        float rotX = Mathf.Sin(t * rotationSpeed) * rotationAmplitude;
        float rotY = Mathf.Cos(t * rotationSpeed * 0.8f) * rotationAmplitude * 0.7f;

        Quaternion targetRot = startRot * Quaternion.Euler(rotX, rotY, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime);

        // --- Position (optional) ---
        if (usePositionIdle)
        {
            float posX = Mathf.Sin(t * positionSpeed) * positionAmplitude;
            float posY = Mathf.Cos(t * positionSpeed * 1.3f) * positionAmplitude;

            Vector3 targetPos = startPos + new Vector3(posX, posY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime);
        }
    }
}
