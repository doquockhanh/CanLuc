using UnityEngine;

public class EnemyStats : MonoBehaviour, IDamageable
{
    [Header("Health & Combat")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int damage = 1; // Sát thương gây ra
    [SerializeField] protected int scoreValue = 100; // Điểm khi bị phá hủy
    [SerializeField] protected bool isDestroyed = false;

    [Header("HP Bar Settings")]
    [SerializeField] protected bool showHpBar = false;
    
    [Header("Debug Settings")]
    [SerializeField] protected bool enableLogging = false;

    // Events
    public System.Action<int, GameObject> OnDamageTaken { get; set; }
    public System.Action<GameObject> OnDestroyed { get; set; }
    public System.Action<GameObject> OnDestroyedByAction { get; set; }

    // Properties
    public bool IsAlive => !isDestroyed && currentHealth > 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int Damage => damage;
    public int ScoreValue => scoreValue;
    public bool IsDestroyed => isDestroyed;
    public bool ShowHpBar => showHpBar;

    protected virtual void Start()
    {
        InitializeHealth();
        InitializeHpBar();
    }

    protected virtual void InitializeHealth()
    {
        currentHealth = maxHealth;
        isDestroyed = false;
    }

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
            OnDestroyedByAction?.Invoke(destroyer);
            EnemyType enemyType = GetComponent<EnemyBase>().GetEnemyType();
            ScoreManager.Instance.AddKillAndScore(enemyType, ScoreValue);
        }

        // Phát hiệu ứng phá hủy
        PlayDestructionEffects();

        // Destroy GameObject sau một khoảng thời gian ngắn
        Destroy(gameObject, 0.1f);
    }

    private bool ShouldAwardScore(GameObject destroyer)
    {
        if (destroyer == null) return false;
        // Không cộng điểm nếu destroyer là FinishObstacle
        return destroyer.GetComponent<FinishObstacle>() == null;
    }

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

    protected virtual void OnPlayDestructionEffects()
    {
        // Override trong derived classes để thêm hiệu ứng riêng
    }

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

    public virtual void SetScoreValue(int newScoreValue)
    {
        scoreValue = Mathf.Max(0, newScoreValue);

        if (enableLogging)
        {
            Debug.Log($"[{gameObject.name}] Score value set to {scoreValue}");
        }
    }

    public virtual void SetDamage(int newDamage)
    {
        damage = Mathf.Max(0, newDamage);

        if (enableLogging)
        {
            Debug.Log($"[{gameObject.name}] Damage set to {damage}");
        }
    }

    public virtual bool CanTakeDamage()
    {
        return !isDestroyed && currentHealth > 0;
    }

    public virtual float GetHealthPercentage()
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }
    
    /// <summary>
    /// Khởi tạo HP Bar nếu được bật
    /// </summary>
    protected virtual void InitializeHpBar()
    {
        if (showHpBar)
        {
            // Kiểm tra xem đã có EnemyHpBarController chưa
            var hpBarController = GetComponent(System.Type.GetType("EnemyHpBarController"));
            if (hpBarController == null)
            {
                // Tự động thêm component nếu chưa có
                hpBarController = gameObject.AddComponent(System.Type.GetType("EnemyHpBarController"));
            }
            
            // Cập nhật HP Bar ngay lập tức sau khi khởi tạo
            if (hpBarController != null)
            {
                var updateMethod = hpBarController.GetType().GetMethod("UpdateHpBarImmediately");
                updateMethod?.Invoke(hpBarController, null);
            }
        }
    }
    
    /// <summary>
    /// Bật/tắt hiển thị HP Bar
    /// </summary>
    public virtual void SetShowHpBar(bool show)
    {
        showHpBar = show;
        
        var hpBarController = GetComponent(System.Type.GetType("EnemyHpBarController"));
        if (hpBarController != null)
        {
            // Sử dụng reflection để gọi method
            var method = hpBarController.GetType().GetMethod("SetShowHpBar");
            method?.Invoke(hpBarController, new object[] { show });
            
            // Cập nhật HP Bar ngay lập tức
            var updateMethod = hpBarController.GetType().GetMethod("UpdateHpBarImmediately");
            updateMethod?.Invoke(hpBarController, null);
        }
        else if (show)
        {
            // Tự động thêm component nếu chưa có và muốn bật
            hpBarController = gameObject.AddComponent(System.Type.GetType("EnemyHpBarController"));
        }
    }
    
    /// <summary>
    /// Cập nhật offset vị trí HP Bar
    /// </summary>
    public virtual void SetHpBarOffset(Vector3 offset)
    {
        var hpBarController = GetComponent(System.Type.GetType("EnemyHpBarController"));
        if (hpBarController != null)
        {
            var method = hpBarController.GetType().GetMethod("SetHpBarOffset");
            method?.Invoke(hpBarController, new object[] { offset });
        }
    }
    
    /// <summary>
    /// Cập nhật kích thước HP Bar
    /// </summary>
    public virtual void SetHpBarSize(float width, float height)
    {
        var hpBarController = GetComponent(System.Type.GetType("EnemyHpBarController"));
        if (hpBarController != null)
        {
            var method = hpBarController.GetType().GetMethod("SetHpBarSize");
            method?.Invoke(hpBarController, new object[] { width, height });
        }
    }
}

