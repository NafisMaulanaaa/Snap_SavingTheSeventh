using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    [Header("Camera Limits")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target == null) return;

        // posisi target + offset
        Vector3 desiredPos = target.position + offset;

        // batasi posisi kamera
        float clampX = Mathf.Clamp(desiredPos.x, minX, maxX);
        float clampY = Mathf.Clamp(desiredPos.y, minY, maxY);

        Vector3 finalPos = new Vector3(clampX, clampY, desiredPos.z);

        // smooth camera movement
        transform.position = Vector3.Lerp(transform.position, finalPos, smoothSpeed * Time.deltaTime);
    }
}
