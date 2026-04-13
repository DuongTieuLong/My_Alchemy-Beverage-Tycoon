using UnityEngine;

public class ExitRouteHolder : MonoBehaviour
{
    public static ExitRouteHolder Instance;

    [Header("Exit Route Waypoints")]
    public Transform[] exitRoute;
    // Kéo thả các waypoint vào đây theo thứ tự rời cửa hàng

    private void Awake()
    {
        Instance = this;
    }
}
