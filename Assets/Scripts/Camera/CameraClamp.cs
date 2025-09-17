using UnityEngine;

public class CameraClamp : MonoBehaviour
{
    public float minX, maxX; // Trái - Phải
    public float minY, maxY; // Dưới - Trên

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        cam = GetComponent<Camera>();
        camHalfHeight = cam.orthographicSize;                // nửa chiều cao camera
        camHalfWidth = camHalfHeight * cam.aspect;           // nửa chiều rộng camera (theo tỉ lệ màn hình)
    }

    void LateUpdate()
    {
        // Giữ nguyên Z
        float newX = Mathf.Clamp(transform.position.x, minX + camHalfWidth, maxX - camHalfWidth);
        float newY = Mathf.Clamp(transform.position.y, minY + camHalfHeight, maxY - camHalfHeight);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
