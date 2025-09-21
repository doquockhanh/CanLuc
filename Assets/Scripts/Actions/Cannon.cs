using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : ActionBase, IMultiForceAction
{
    [Header("Rotation Settings")]
    [SerializeField] private Transform childToRotate;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fixedAngle = 60f;
    [SerializeField] private GameObject bullet;

    [Header("Firing Settings")]
    [SerializeField] private static float minDelayTime = 0f; // Thời gian delay tối thiểu
    [SerializeField] private float maxDelayTime = 5f; // Thời gian delay tối đa

    [Header("Bullet Settings")]
    [SerializeField] private int bulletsPerBurst = 5; // Số bullet mỗi loạt
    [SerializeField] private float timeBetweenBullets = 0.2f; // Thời gian giữa các bullet
    [SerializeField] private float baseBulletSpeed = 10f; // Tốc độ bullet cơ bản
    [SerializeField] private float forceToSpeedMultiplier = 0.1f; // Hệ số chuyển đổi từ force sang tốc độ

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootingSound;

    // Private variables
    private bool isExecuting = false;
    private bool isCharging = false;
    private Coroutine currentExecution;
    private float bulletForce = 0f; // Lưu trữ lực bắn từ thanh 1
    private float delayForce = 0f; // Lưu trữ lực delay từ thanh 2

    // Bullet tracking
    private List<GameObject> activeBullets = new List<GameObject>();
    private Coroutine bulletTrackingCoroutine;

    // IMultiForceAction implementation
    public int ForceBarCount => 2;

    protected override void Awake()
    {
        base.Awake();
        audioSource.clip = shootingSound;
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

    public void Execute(float[] forces)
    {
        if (forces == null || forces.Length < 2 || bullet == null || isExecuting)
        {
            MarkActionCompleted();
            return;
        }
        if (forces[0] <= 0f)
        {
            MarkActionCompleted();
            return;
        }

        // Lưu trữ lực từ 2 thanh
        bulletForce = forces[0];
        delayForce = forces[1];

        // Dừng execution hiện tại nếu có
        if (currentExecution != null)
        {
            StopCoroutine(currentExecution);
        }

        // Bắt đầu execution mới
        currentExecution = StartCoroutine(ExecuteCannon());
    }

    public override void ResetForNewPhase()
    {
        base.ResetForNewPhase();

        // Reset Cannon-specific variables
        isExecuting = false;
        isCharging = false;
        bulletForce = 0f;
        delayForce = 0f;

        // Clear bullet tracking list
        activeBullets.Clear();

        // Stop any running coroutine
        if (currentExecution != null)
        {
            StopCoroutine(currentExecution);
            currentExecution = null;
        }
    }

    private IEnumerator ExecuteCannon()
    {
        isExecuting = true;

        // Tính thời gian delay dựa trên thanh lực 2
        float delayTime = CalculateDelayTime(delayForce);

        // Bắt đầu charging phase
        yield return StartCoroutine(ChargingPhase(delayTime));

        // Bắt đầu firing phase với lực từ thanh 1
        yield return StartCoroutine(FiringPhase());

        // Start tracking bullets for completion
        if (activeBullets.Count > 0)
        {
            StartCoroutine(TrackBulletsForCompletion());
        }
        else
        {
            // No bullets fired, mark as completed immediately
            MarkActionCompleted();
        }

        isExecuting = false;
        currentExecution = null;
    }

    private float CalculateDelayTime(float force)
    {
        // Force càng cao thì delay càng lâu
        float maxForce = GetComponent<ForceAccumulator>().MaxForce;
        float delayTime = force * maxDelayTime / maxForce;
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

    private void FireBullet()
    {
        if (bullet == null) return;

        audioSource.Play();

        // Tạo bullet
        GameObject bulletInstance = Instantiate(bullet, firePoint.transform.position, transform.rotation);

        // Add bullet to tracking list
        activeBullets.Add(bulletInstance);

        // Thiết lập velocity cho bullet dựa trên lực từ thanh 1
        Rigidbody2D bulletRb = bulletInstance.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            // Tính tốc độ dựa trên lực từ thanh 1
            float calculatedSpeed = baseBulletSpeed + (bulletForce * forceToSpeedMultiplier);
            float randomForce = calculatedSpeed + Random.Range(-(calculatedSpeed * 10 / 100), calculatedSpeed * 10 / 100);
            bulletRb.AddForce(childToRotate.right * randomForce, ForceMode2D.Impulse);
        }
    }
}