using UnityEngine;

public class ActionStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int currentHealth;
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
    public bool IsDestroyed => isDestroyed;

    protected virtual void Start()
    {
        InitializeHealth();
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
            Debug.Log($"[{gameObject.name}] Action took {damage} damage from {sourceName}. Health: {currentHealth}/{maxHealth}");
        }

        // Trigger damage event
        OnDamageTaken?.Invoke(damage, damageSource);

        // Kiểm tra xem có bị phá hủy không
        if (currentHealth <= 0)
        {
            DestroyAction(damageSource);
        }
    }

    protected virtual void DestroyAction(GameObject destroyer = null)
    {
        if (isDestroyed) return;

        isDestroyed = true;

        if (enableLogging)
        {
            string destroyerName = destroyer != null ? destroyer.name : "Unknown";
            Debug.Log($"[{gameObject.name}] Action destroyed by {destroyerName}!");
        }

        // Trigger destroyed event
        OnDestroyed?.Invoke(gameObject);

        // Phát hiệu ứng phá hủy
        PlayDestructionEffects();

        // Destroy GameObject sau một khoảng thời gian ngắn
        Destroy(gameObject, 0.1f);
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
            Debug.Log($"[{gameObject.name}] Action healed for {currentHealth - oldHealth}. Health: {currentHealth}/{maxHealth}");
        }
    }

    public virtual void SetHealth(int newHealth)
    {
        if (isDestroyed) return;

        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        if (enableLogging)
        {
            Debug.Log($"[{gameObject.name}] Action health set to {currentHealth}/{maxHealth}");
        }

        // Kiểm tra xem có bị phá hủy không
        if (currentHealth <= 0)
        {
            DestroyAction();
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
            Debug.Log($"[{gameObject.name}] Action max health set to {maxHealth}, current health: {currentHealth}");
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
}
