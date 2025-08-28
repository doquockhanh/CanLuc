using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Flappy Bird Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float forwardSpeed = 2f;
    [SerializeField] private float maxRotation = 45f; // Độ nghiêng tối đa khi nhảy lên
    [SerializeField] private float minRotation = -90f; // Độ nghiêng tối đa khi rơi xuống
    [SerializeField] private float rotationLerpSpeed = 10f;
    [Header("Rotation Speed Settings")]
    [SerializeField] private float maxFallSpeedForRotation = -10f;
    [SerializeField] private float maxRiseSpeedForRotation = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D component on the same GameObject.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Click chuột trái hoặc chạm màn hình trên mobile
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        UpdateRotation();
    }

    private void Jump()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void UpdateRotation()
    {
        if (rb == null) return;
        // Góc nghiêng dựa trên vận tốc y, dùng biến cấu hình thay cho magic number
        float angle = Mathf.Lerp(
            minRotation,
            maxRotation,
            Mathf.InverseLerp(maxFallSpeedForRotation, maxRiseSpeedForRotation, rb.linearVelocity.y)
        );
        // Làm mượt chuyển động xoay
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * rotationLerpSpeed);
    }
}
