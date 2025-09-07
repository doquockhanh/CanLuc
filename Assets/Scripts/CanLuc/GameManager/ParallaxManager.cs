using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    public ParallaxLayer[] layers; // danh sách layer
    private Vector3 lastCameraPosition;

    void Start()
    {
        lastCameraPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = transform.position - lastCameraPosition;

        foreach (var layer in layers)
        {
            if (layer.layerTransform == null) continue;

            float moveX = layer.affectX ? delta.x * layer.parallaxFactor : 0f;
            float moveY = layer.affectY ? delta.y * layer.parallaxFactor : 0f;

            layer.layerTransform.position += new Vector3(moveX, moveY, 0);
        }

        lastCameraPosition = transform.position;
    }
}

[System.Serializable]
public class ParallaxLayer
{
    public Transform layerTransform; // object background
    [Tooltip("0 = xa, 1 = di chuyển cùng camera, >1 = gần hơn")]
    public float parallaxFactor = 0.5f;
    public bool affectX = true;
    public bool affectY = false;
}