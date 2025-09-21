using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyType type;

    // Enemy execution state
    protected bool isEnemyExecuting = false;
    protected bool isEnemyCompleted = false;

    protected virtual void Awake()
    {
        // Đăng ký với PhaseManager để quản lý tập trung
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.RegisterEnemy(this);
        }
    }

    protected virtual void OnDestroy()
    {
        // Hủy đăng ký khi component bị destroy
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.UnregisterEnemy(this);
        }
    }

    public EnemyType GetEnemyType() {
        return type;
    }

    #region Enemy Execution and Completion

    /// <summary>
    /// Called by PhaseManager to execute this enemy
    /// </summary>
    public virtual void ExecuteEnemy()
    {
        if (isEnemyExecuting || isEnemyCompleted) return;
        
        isEnemyExecuting = true;
        isEnemyCompleted = false;
        
        // Call derived class implementation
        OnEnemyExecuted();
    }

    /// <summary>
    /// Override this method in derived classes to implement specific enemy logic
    /// </summary>
    protected virtual void OnEnemyExecuted()
    {
        // Default implementation - mark as completed immediately
        // Derived classes should override this and call MarkEnemyCompleted() when done
        MarkEnemyCompleted();
    }

    /// <summary>
    /// Mark this enemy as completed and notify PhaseManager
    /// </summary>
    protected virtual void MarkEnemyCompleted()
    {
        if (isEnemyCompleted) return;
        
        isEnemyCompleted = true;
        isEnemyExecuting = false;
        
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnEnemyCompleted(this);
        }
    }

    /// <summary>
    /// Check if this enemy is completed
    /// </summary>
    public virtual bool IsEnemyCompleted()
    {
        return isEnemyCompleted;
    }

    /// <summary>
    /// Check if this enemy is currently executing
    /// </summary>
    public virtual bool IsEnemyExecuting()
    {
        return isEnemyExecuting;
    }

    /// <summary>
    /// Reset enemy state for next execution
    /// </summary>
    protected virtual void ResetEnemyState()
    {
        isEnemyExecuting = false;
        isEnemyCompleted = false;
    }

    /// <summary>
    /// Public method to reset enemy state when transitioning to Prepare phase
    /// </summary>
    public virtual void ResetForNewPhase()
    {
        ResetEnemyState();
    }

    #endregion
}


public enum EnemyType
{
    A7,
    B52,
    Balloon,
    F4
}
