using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaoNamAction : ActionBase, IForceAction, IMoveAction
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float maxMovePower = 10f;
    private float currentMovePower;


    [Header("Gun Settings")]
    public Transform gunBarrel;
    public float rotationSpeed = 90f;
    public float maxRotationAngle = 45f;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform[] bulletSpawnPoints = new Transform[3]; // 3 điểm spawn cho 3 viên đạn
    public float bulletSpreadAngle = 15f; // Góc lệch của 2 viên đạn bên cạnh
    [SerializeField] private float baseBulletSpeed = 10f; // Tốc độ bullet cơ bản
    [SerializeField] private float forceToSpeedMultiplier = 0.1f; // Hệ số chuyển đổi từ force sang tốc độ


    private bool canMove = true;
    private bool isFacingRight = true;
    private float currentGunRotation = 0f;
    private List<GameObject> activeBullets = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        currentMovePower = maxMovePower;
    }

    protected override void HandleInput()
    {
        base.HandleInput(); // Gọi base để xử lý tích lực

        // Xử lý di chuyển A/D
        HandleMovement();

        // Xử lý xoay súng S/W
        HandleGunRotation();
        
        // Xử lý flip Q/E (không cần movePower)
        HandleFacing();
    }

    private void HandleMovement()
    {
        if (!canMove) return;

        float horizontalInput = 0f;

        // A/D để di chuyển
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        if (horizontalInput != 0 && currentMovePower > 0f)
        {
            // Di chuyển với tốc độ cố định
            transform.Translate(Vector3.right * horizontalInput * moveSpeed * Time.deltaTime);

            // Tiêu hao sức mạnh cố định
            currentMovePower -= 1f * Time.deltaTime;
            currentMovePower = Mathf.Max(0, currentMovePower);
        }
    }

    private void HandleGunRotation()
    {
        if (gunBarrel == null) return;

        float rotationInput = 0f;

        // S/W để xoay súng
        if (Input.GetKey(KeyCode.S))
        {
            rotationInput = isFacingRight ? -1f : 1f;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            rotationInput = isFacingRight ? 1f : -1f;
        }

        if (rotationInput != 0)
        {
            // Cập nhật góc xoay
            currentGunRotation += rotationInput * rotationSpeed * Time.deltaTime;
            currentGunRotation = Mathf.Clamp(currentGunRotation, -maxRotationAngle, maxRotationAngle);

            // Áp dụng xoay cho nòng súng
            gunBarrel.rotation = Quaternion.Euler(0, 0, currentGunRotation);
        }
    }

    private void HandleFacing()
    {
        // Q/E để flip hướng (không cần movePower)
        if (Input.GetKeyDown(KeyCode.A) && isFacingRight)
        {
            FlipAction(false); // Quay trái
        }
        else if (Input.GetKeyDown(KeyCode.D) && !isFacingRight)
        {
            FlipAction(true); // Quay phải
        }
    }

    private void FlipAction(bool facingRight)
    {
        isFacingRight = facingRight;

        // Flip toàn bộ GameObject
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        // Đảo ngược góc xoay súng nếu cần
        if (gunBarrel != null)
        {
            currentGunRotation = -currentGunRotation;
            gunBarrel.rotation = Quaternion.Euler(0, 0, currentGunRotation);
        }
    }

    // IForceAction implementation
    public void Execute(float force)
    {
        if (bulletPrefab == null || bulletSpawnPoints[0] == null) return;

        // Bắn 3 viên đạn
        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = bulletSpawnPoints[i].position;
            Quaternion spawnRotation = bulletSpawnPoints[i].rotation;

            // Tính góc cho viên đạn bên cạnh dựa theo góc của gunBarrel
            if (i == 1) // Viên đạn trái
            {
                spawnRotation = gunBarrel.rotation * Quaternion.Euler(0, 0, -bulletSpreadAngle);
            }
            else if (i == 2) // Viên đạn phải
            {
                spawnRotation = gunBarrel.rotation * Quaternion.Euler(0, 0, bulletSpreadAngle);
            }
            else // Viên đạn giữa (i == 0)
            {
                spawnRotation = gunBarrel.rotation;
            }

            // Tạo đạn
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);

            activeBullets.Add(bullet);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                float calculatedSpeed = baseBulletSpeed + (force * forceToSpeedMultiplier);
                float randomForce = calculatedSpeed + Random.Range(-(calculatedSpeed * 10 / 100), calculatedSpeed * 10 / 100);
                Vector3 direction = isFacingRight ? bullet.transform.right : -bullet.transform.right;
                bulletRb.AddForce(direction * randomForce, ForceMode2D.Impulse);
            }
        }

        if (activeBullets.Count > 0)
        {
            StartCoroutine(TrackBulletsForCompletion());
        }
        else
        {
            MarkActionCompleted();
        }
    }

    private IEnumerator TrackBulletsForCompletion()
    {
        // Wait until all bullets are destroyed
        while (activeBullets.Count > 0)
        {
            // Remove null references (destroyed bullets)
            activeBullets.RemoveAll(bullet => bullet == null);

            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
        }

        // All bullets destroyed, mark action as completed
        MarkActionCompleted();
    }

    // Gọi khi sang phase mới để hồi đầy sức mạnh
    public override void ResetForNewPhase()
    {
        base.ResetForNewPhase();
        currentMovePower = maxMovePower;
        activeBullets.Clear();
    }

    // Các phương thức để MoveBarUI có thể truy cập
    public float GetCurrentPower()
    {
        return currentMovePower;
    }

    public float GetMaxPower()
    {
        return maxMovePower;
    }


    public override void OnFocused(GameObject previous)
    {
        base.OnFocused(previous);
        CameraController.Instance.LockCameraMovement();
    }

    public override void OnDefocused(GameObject next)
    {
        base.OnDefocused(next);
        CameraController.Instance.UnlockCameraMovement();
    }
}
