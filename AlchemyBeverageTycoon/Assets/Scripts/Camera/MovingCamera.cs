using UnityEngine;

public class MovingCamera : MonoBehaviour
{
    public void TeleportTo(Transform newTranform)
    {
        transform.position = newTranform.position;
        transform.rotation = newTranform.rotation;
    }
}

