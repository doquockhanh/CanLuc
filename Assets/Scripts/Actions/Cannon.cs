using System.Collections;
using UnityEngine;

public class Cannon : FocusableBase, IForceAction
{
    [Header("Rotation Settings")]
    [SerializeField] private Transform childToRotate;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fixedAngle = 60f;
    [SerializeField] private GameObject bullet;

    [Header("Firing Settings")]
    [SerializeField] private static float minDelayTime = 0f; // Thời gian delay tối thiểu
    [SerializeField] private float maxDelayTime = 5f; // Thời gian delay tối đa
    [SerializeField] private float forceToDelayMultiplier = 0.5f; // Hệ số chuyển đổi force thành delay time

    [Header("Bullet Settings")]
    [SerializeField] private int bulletsPerBurst = 5; // Số bullet mỗi loạt
    [SerializeField] private float timeBetweenBullets = 0.2f; // Thời gian giữa các bullet
    [SerializeField] private float bulletSpeed = 10f; // Tốc độ bullet

    // Private variables
    private bool isExecuting = false;
    private bool isCharging = false;
    private Coroutine currentExecution;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(StartRotation());
    }

    private IEnumerator StartRotation()
    {
        float angle = 0;
        float speed = 0.1f;
        while (angle < fixedAngle)
        {
            angle += speed * 10f;
            childToRotate.localEulerAngles = new Vector3(0, 0, angle);
            yield return new WaitForSeconds(speed);
        }
    }

    public void Execute(float force)
    {
        if (force <= 0f || bullet == null || isExecuting) return;

        // Dừng execution hiện tại nếu có
        if (currentExecution != null)
        {
            StopCoroutine(currentExecution);
        }

        // Bắt đầu execution mới
        currentExecution = StartCoroutine(ExecuteCannon(force));
    }

    private IEnumerator ExecuteCannon(float force)
    {
        isExecuting = true;

        // Tính thời gian delay dựa trên force
        float delayTime = CalculateDelayTime(force);

        Debug.Log($"[{gameObject.name}] Cannon charging for {delayTime:F2} seconds (force: {force})");

        // Bắt đầu charging phase
        yield return StartCoroutine(ChargingPhase(delayTime));

        // Bắt đầu firing phase
        yield return StartCoroutine(FiringPhase());

        isExecuting = false;
        currentExecution = null;
    }

    private float CalculateDelayTime(float force)
    {
        // Force càng cao thì delay càng lâu
        float maxForce = GetComponent<ForceAccumulator>().MaxForce;
        float delayTime = force * forceToDelayMultiplier / maxForce;
        return Mathf.Clamp(delayTime, minDelayTime, maxDelayTime);
    }

    private IEnumerator ChargingPhase(float delayTime)
    {
        isCharging = true;

        // Chờ thời gian delay
        yield return new WaitForSeconds(delayTime);

        isCharging = false;
    }

    private IEnumerator FiringPhase()
    {

        // Bắn từng bullet
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            FireBullet();

            // Chờ trước khi bắn bullet tiếp theo
            if (i < bulletsPerBurst - 1)
            {
                yield return new WaitForSeconds(timeBetweenBullets);
            }
        }
    }

    private void FireBullet()
    {
        if (bullet == null) return;

        // Tạo bullet
        GameObject bulletInstance = Instantiate(bullet, firePoint.transform.position, transform.rotation);

        // Thiết lập velocity cho bullet
        Rigidbody2D bulletRb = bulletInstance.GetComponent<Rigidbody2D>();
        float randomForce = bulletSpeed + Random.Range(-(bulletSpeed * 10 / 100), bulletSpeed * 10 / 100);
        if (bulletRb != null)
        {
            bulletRb.AddForce(childToRotate.right * randomForce, ForceMode2D.Impulse);
        }

        Debug.Log($"[{gameObject.name}] Fired bullet with velocity: {bulletRb?.linearVelocity}");
    }
}