using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable
{
    [Header("Health & Combat")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int scoreValue = 100; // Điểm khi bị phá hủy
    [SerializeField] protected bool isDestroyed = false;

    [Header("Debug Settings")]
    [SerializeField] protected bool enableLogging = false;

    // Events
    public System.Action<int, GameObject> OnDamageTaken { get; set; }
    public System.Action<GameObject> OnDestroyed { get; set; }

    // Properties
    public bool IsAlive => !isDestroyed && currentHealth > 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int ScoreValue => scoreValue;
    public bool IsDestroyed => isDestroyed;

    protected virtual void Start()
    {
        InitializeHealth();
    }

    /// <summary>
    /// Khởi tạo health
    /// </summary>
    protected virtual void InitializeHealth()
    {
        currentHealth = maxHealth;
        isDestroyed = false;
    }

    /// <summary>
    /// Nhận damage từ nguồn khác
    /// </summary>
    /// <param name="damage">Lượng damage nhận được</param>
    /// <param name="damageSource">Nguồn gây damage (có thể null)</param>
    public virtual void TakeDamage(int damage, GameObject damageSource = null)
    {
        if (isDestroyed || damage <= 0) return;

        currentHealth -= damage;

        if (enableLogging)
        {
            string sourceName = damageSource != null ? damageSource.name : "Unknown";
            Debug.Log($"[{gameObject.name}] Took {damage} damage from {sourceName}. Health: {currentHealth}/{maxHealth}");
        }

        // Trigger damage event
        OnDamageTaken?.Invoke(damage, damageSource);

        // Kiểm tra xem có bị phá hủy không
        if (currentHealth <= 0)
        {
            DestroyEnemy(damageSource);
        }
    }

    /// <summary>
    /// Phá hủy enemy
    /// </summary>
    /// <param name="destroyer">Object gây ra sự phá hủy (có thể null)</param>
    protected virtual void DestroyEnemy(GameObject destroyer = null)
    {
        if (isDestroyed) return;

        isDestroyed = true;

        if (enableLogging)
        {
            string destroyerName = destroyer != null ? destroyer.name : "Unknown";
            Debug.Log($"[{gameObject.name}] Enemy destroyed by {destroyerName}! Awarding {scoreValue} points");
        }

        // Trigger destroyed event
        OnDestroyed?.Invoke(gameObject);

        // Cộng điểm cho màn chơi (trừ khi nguồn hủy là NoScoreKillSource)
        if (ShouldAwardScore(destroyer))
        {
            AwardScore();
            // Tăng kill count và phát kill sound (chỉ khi bị player tiêu diệt)
            AwardKill();
        }

        // Phát hiệu ứng phá hủy
        PlayDestructionEffects();

        // Destroy GameObject sau một khoảng thời gian ngắn
        Destroy(gameObject, 0.1f);
    }

    private bool ShouldAwardScore(GameObject destroyer)
    {
        if (destroyer == null) return true;
        // Không cộng điểm nếu destroyer là FinishObstacle
        return destroyer.GetComponent<FinishObstacle>() == null;
    }

    /// <summary>
    /// Cộng điểm cho màn chơi
    /// </summary>
    protected virtual void AwardScore()
    {
        // Tìm ScoreManager để cộng điểm
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
        else
        {
            // Fallback: log warning nếu không có ScoreManager
            Debug.LogWarning($"[{gameObject.name}] ScoreManager not found! Cannot award {scoreValue} points");
        }
    }

    /// <summary>
    /// Tăng kill count và phát kill sound (chỉ khi bị player tiêu diệt)
    /// </summary>
    protected virtual void AwardKill()
    {
        // Tìm ScoreManager để tăng kill count
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddKill();
        }
        else
        {
            // Fallback: log warning nếu không có ScoreManager
            Debug.LogWarning($"[{gameObject.name}] ScoreManager not found! Cannot award kill");
        }
    }

    /// <summary>
    /// Phát hiệu ứng phá hủy
    /// </summary>
    protected virtual void PlayDestructionEffects()
    {
        // Phát particle effect
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayParticleSystem("vehicleExplosion", transform.position);
        }

        // Có thể override để thêm các hiệu ứng khác
        OnPlayDestructionEffects();
    }

    /// <summary>
    /// Override để thêm các hiệu ứng phá hủy tùy chỉnh
    /// </summary>
    protected virtual void OnPlayDestructionEffects()
    {
        // Override trong derived classes để thêm hiệu ứng riêng
    }

    /// <summary>
    /// Heal enemy
    /// </summary>
    /// <param name="healAmount">Lượng máu hồi</param>
    public virtual void Heal(int healAmount)
    {
        if (isDestroyed || healAmount <= 0) return;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        if (enableLogging && currentHealth != oldHealth)
        {
            Debug.Log($"[{gameObject.name}] Healed for {currentHealth - oldHealth}. Health: {currentHealth}/{maxHealth}");
        }
    }

    /// <summary>
    /// Set health về giá trị cụ thể
    /// </summary>
    /// <param name="newHealth">Health mới</param>
    public virtual void SetHealth(int newHealth)
    {
        if (isDestroyed) return;

        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        if (enableLogging)
        {
            Debug.Log($"[{gameObject.name}] Health set to {currentHealth}/{maxHealth}");
        }

        // Kiểm tra xem có bị phá hủy không
        if (currentHealth <= 0)
        {
            DestroyEnemy();
        }
    }

    /// <summary>
    /// Set max health và điều chỉnh current health tương ứng
    /// </summary>
    /// <param name="newMaxHealth">Max health mới</param>
    public virtual void SetMaxHealth(int newMaxHealth)
    {
        if (newMaxHealth <= 0) return;

        float healthPercentage = (float)currentHealth / maxHealth;
        maxHealth = newMaxHealth;
        currentHealth = Mathf.RoundToInt(maxHealth * healthPercentage);

        if (enableLogging)
        {
            Debug.Log($"[{gameObject.name}] Max health set to {maxHealth}, current health: {currentHealth}");
        }
    }

    /// <summary>
    /// Set score value
    /// </summary>
    /// <param name="newScoreValue">Score value mới</param>
    public virtual void SetScoreValue(int newScoreValue)
    {
        scoreValue = Mathf.Max(0, newScoreValue);

        if (enableLogging)
        {
            Debug.Log($"[{gameObject.name}] Score value set to {scoreValue}");
        }
    }

    /// <summary>
    /// Kiểm tra xem có thể nhận damage không
    /// </summary>
    /// <returns>True nếu có thể nhận damage</returns>
    public virtual bool CanTakeDamage()
    {
        return !isDestroyed && currentHealth > 0;
    }

    /// <summary>
    /// Lấy health percentage (0-1)
    /// </summary>
    /// <returns>Health percentage</returns>
    public virtual float GetHealthPercentage()
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }
}

