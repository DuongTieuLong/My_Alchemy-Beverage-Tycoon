using UnityEngine;
using Unity.Cinemachine; 
using UnityEngine.EventSystems;

[AddComponentMenu("Camera/Cinemachine Look Around")]
public class CinemachineLookAround : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cineCamera; // Gán camera Cinemachine hiện tại

    [Header("Rotation Settings")]
    public float rotateSpeed = 0.5f;
    public float yawLimit = 90f;   // Giới hạn 180° ngang (-90 -> 90)
    public float pitchLimit = 90f; // Giới hạn 180° dọc (-90 -> 90)

    [Header("Zoom Settings")]
    public bool zoomByFOV = true;      // true = zoom bằng FOV, false = zoom bằng di chuyển Z
    public float zoomSpeed = 2f;
    public float minZoom = 30f;        // Nếu zoomByFOV: FOV min
    public float maxZoom = 70f;        // Nếu zoomByFOV: FOV max
    public float minDistance = 2f;     // Nếu không dùng FOV
    public float maxDistance = 10f;

    private float yaw;
    private float pitch;
    private float targetZoom;
    private Vector2 lastTouchPos;
    private bool dragging;
    private Transform camTransform;

    void Start()
    {
        if (cineCamera == null)
            cineCamera = GetComponent<CinemachineCamera>();

        camTransform = cineCamera.transform;

        // Lưu giá trị ban đầu
        Vector3 euler = camTransform.localEulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        if (zoomByFOV)
            targetZoom = cineCamera.Lens.FieldOfView;
        else
            targetZoom = camTransform.localPosition.z;
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif

        ApplyRotation();
        ApplyZoom();
    }

    void HandleMouse()
    {
        // 🚫 Nếu chuột đang trỏ lên UI thì bỏ qua
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;


        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            lastTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouchPos;
            lastTouchPos = Input.mousePosition;

            yaw += delta.x * rotateSpeed;
            pitch -= delta.y * rotateSpeed;

            yaw = Mathf.Clamp(yaw, -yawLimit, yawLimit);
            pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetZoom -= scroll * zoomSpeed * 10f;
        }
    }

    void HandleTouch()
    {

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                dragging = true;
                lastTouchPos = t.position;
            }
            else if (t.phase == TouchPhase.Moved && dragging)
            {
                Vector2 delta = t.position - lastTouchPos;
                lastTouchPos = t.position;

                yaw += delta.x * rotateSpeed * 0.5f;
                pitch -= delta.y * rotateSpeed * 0.5f;

                yaw = Mathf.Clamp(yaw, -yawLimit, yawLimit);
                pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                dragging = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch a = Input.GetTouch(0);
            Touch b = Input.GetTouch(1);

            Vector2 prevA = a.position - a.deltaPosition;
            Vector2 prevB = b.position - b.deltaPosition;

            float prevDist = Vector2.Distance(prevA, prevB);
            float curDist = Vector2.Distance(a.position, b.position);
            float delta = curDist - prevDist;

            targetZoom -= delta * zoomSpeed * 0.05f;
        }
    }

    void ApplyRotation()
    {
        camTransform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void ApplyZoom()
    {
        if (zoomByFOV)
        {
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            cineCamera.Lens.FieldOfView = Mathf.Lerp(cineCamera.Lens.FieldOfView, targetZoom, Time.deltaTime * 10f);
        }
        else
        {
            targetZoom = Mathf.Clamp(targetZoom, -maxDistance, -minDistance);
            Vector3 pos = camTransform.localPosition;
            pos.z = Mathf.Lerp(pos.z, targetZoom, Time.deltaTime * 10f);
            camTransform.localPosition = pos;
        }
    }
}
