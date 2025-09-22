using UnityEngine;
using System.Collections;

public class MiniBomController : EnemyBase
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.right; // Hướng di chuyển
    [SerializeField] private float moveDistance = 2f; // Khoảng cách di chuyển
    [SerializeField] private float moveSpeed = 3f; // Tốc độ di chuyển
    public int maxPhaseLive = 3;
    private int phaseCount = 0;

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffectPrefab; // Effect phát nổ
    [SerializeField] private float explosionRadius = 1.5f; // Bán kính vụ nổ
    [SerializeField] private float explosionDelay = 0.5f; // Delay trước khi nổ
    [SerializeField] private LayerMask actionLayerMask = 1; // Layer của Action

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;

    private bool hasMoved = false; // Đã di chuyển chưa
    private bool isMoving = false; // Đang di chuyển
    private Vector3 startPosition; // Vị trí bắt đầu
    private bool isFacingRight = true;
    private EnemyStats enemyStats; // Reference đến EnemyStats để lấy damage

    protected override void Awake()
    {
        base.Awake(); // Đăng ký với PhaseManager
        Vector3 scale = transform.localScale;
        scale.x = moveDirection == Vector3.right ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        // Lấy reference đến EnemyStats
        enemyStats = GetComponent<EnemyStats>();
    }

    #region Enemy Execution Override

    protected override void OnEnemyExecuted()
    {
        // Lần đầu tiên được tạo ra: không làm gì cả
        if (!hasMoved)
        {
            MarkEnemyCompleted();
            return;
        }

        // Các phase tiếp theo: di chuyển
        if (!isMoving)
        {
            StartCoroutine(Move());
        }
    }


    public override void ResetForNewPhase()
    {
        base.ResetForNewPhase();

        // Đánh dấu đã có thể di chuyển ở phase tiếp theo
        if (!hasMoved)
        {
            hasMoved = true;
        }

        phaseCount++;
        if (phaseCount >= maxPhaseLive)
        {
            StartCoroutine(ExplodeWithDelay());
        }
    }

    #endregion

    #region Movement

    private IEnumerator Move()
    {
        isMoving = true;
        startPosition = transform.position;
        float moveDuration = moveDistance / moveSpeed; // thời gian cần để đi hết khoảng cách
        float elapsedTime = 0f;

        // Di chuyển smooth đến vị trí đích
        while (elapsedTime < moveDuration)
        {
            transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;

            // Kiểm tra Action trong tầm nổ mỗi frame
            if (CheckForActionsInRange())
            {
                isMoving = false;
                yield return StartCoroutine(ExplodeWithDelay());
                yield break; // Dừng di chuyển và thoát
            }

            yield return null;
        }

        isMoving = false;
        MarkEnemyCompleted();
    }

    #endregion

    #region Explosion & Damage

    private bool CheckForActionsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, actionLayerMask);

        // Kiểm tra xem có Action nào có thể nhận sát thương không
        foreach (Collider2D collider in colliders)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator ExplodeWithDelay()
    {
        // Delay trước khi nổ
        yield return new WaitForSeconds(explosionDelay);

        // Nổ
        Explode();
        MarkEnemyCompleted();
    }

    private void Explode()
    {
        // Phát hiệu ứng nổ
        PlayExplosionEffect();

        // Phát âm thanh nổ
        PlayExplosionSound();

        // Tìm tất cả action trong bán kính nổ
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, actionLayerMask);

        foreach (Collider2D collider in colliders)
        {
            DealDamageToAction(collider.gameObject);
        }

        // Tự hủy sau khi nổ
        if (enemyStats != null)
        {
            enemyStats.TakeDamage(enemyStats.MaxHealth);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DealDamageToAction(GameObject action)
    {
        // Kiểm tra xem action có component IDamageable không
        IDamageable damageable = action.GetComponent<IDamageable>();
        if (damageable != null && damageable.IsAlive)
        {
            int damage = enemyStats != null ? enemyStats.Damage : 1;
            damageable.TakeDamage(damage, gameObject);
        }
    }

    private void PlayExplosionEffect()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }

    #endregion
}
