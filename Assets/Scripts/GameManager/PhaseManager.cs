using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [Header("Phase Settings")]
    [SerializeField] private float phaseTransitionDelay = 0.5f;

    // Phase state
    private bool isExecutingActions = false;
    private bool isExecutingEnemies = false;
    private bool allActionsCompleted = false;
    private bool allEnemiesCompleted = false;

    // Collections for tracking
    private readonly List<ActionBase> registeredActions = new List<ActionBase>();
    public readonly List<EnemyBase> registeredEnemies = new List<EnemyBase>();
    private readonly List<EnemyBase> executedEnemies = new List<EnemyBase>();

    // Events
    public System.Action OnActionsExecutionStarted;
    public System.Action OnActionsExecutionCompleted;
    public System.Action OnEnemiesExecutionStarted;
    public System.Action OnEnemiesExecutionCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        RegisterAllActionsAndEnemies();
    }

    public void StartBattlePhaseExecution()
    {
        StartCoroutine(ExecuteBattlePhaseSequence());
    }

    private IEnumerator ExecuteBattlePhaseSequence()
    {
        // Step 1: Execute Actions
        yield return StartCoroutine(ExecuteActionsPhase());
        
        // Step 2: Execute Enemies
        yield return StartCoroutine(ExecuteEnemiesPhase());
        
        // Step 3: Reset all actions for next phase
        ResetAllActionsForNewPhase();
        
        // Notify GameManager to transition back to Prepare phase
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartPreparePhase();
        }
    }

    private IEnumerator ExecuteActionsPhase()
    {
        isExecutingActions = true;
        allActionsCompleted = false;
        
        OnActionsExecutionStarted?.Invoke();
        
        // Execute all registered actions
        foreach (var action in registeredActions)
        {
            if (action != null && action.gameObject.activeInHierarchy)
            {
                action.ExecuteAction();
            }
        }
        
        // Wait for all actions to complete
        yield return new WaitUntil(() => allActionsCompleted);
        
        OnActionsExecutionCompleted?.Invoke();
        
        // Small delay before next phase
        yield return new WaitForSeconds(phaseTransitionDelay);
        
        isExecutingActions = false;
    }

    private IEnumerator ExecuteEnemiesPhase()
    {
        isExecutingEnemies = true;
        allEnemiesCompleted = false;
        executedEnemies.Clear(); // Reset danh sách enemies đã execute
        
        OnEnemiesExecutionStarted?.Invoke();
        
        // Execute all registered enemies
        foreach (var enemy in registeredEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.ExecuteEnemy();
                executedEnemies.Add(enemy); // Track enemy đã execute
            }
        }
        
        yield return new WaitForSeconds(0.1f); // Cho phép thời gian để enemies mới được register
        CheckAndExecutePendingEnemies();
        
        // Wait for all enemies to complete
        yield return new WaitUntil(() => allEnemiesCompleted);
        
        OnEnemiesExecutionCompleted?.Invoke();
        
        // Small delay before phase completion
        yield return new WaitForSeconds(phaseTransitionDelay);
        
        isExecutingEnemies = false;
    }

    /// <summary>
    /// Register an action for centralized management
    /// </summary>
    public void RegisterAction(ActionBase action)
    {
        if (action != null && !registeredActions.Contains(action))
        {
            registeredActions.Add(action);
        }
    }

    /// <summary>
    /// Unregister an action
    /// </summary>
    public void UnregisterAction(ActionBase action)
    {
        if (action != null && registeredActions.Remove(action))
        {
            CheckActionsCompletion();
        }
    }

    /// <summary>
    /// Register an enemy for centralized management
    /// </summary>
    public void RegisterEnemy(EnemyBase enemy)
    {
        if (enemy != null && !registeredEnemies.Contains(enemy))
        {
            registeredEnemies.Add(enemy);
            
            // Kiểm tra phase hiện tại và quyết định có execute hay không
            if (isExecutingEnemies)
            {
                if (enemy.gameObject.activeInHierarchy)
                {
                    enemy.ExecuteEnemy();
                    executedEnemies.Add(enemy); // Track enemy đã execute
                }
            }
            else if (GameManager.Instance != null && GameManager.Instance.IsInBattlePhase())
            {
                // Nếu đang trong Battle Phase nhưng chưa execute enemies, 
                // có thể là đang trong Actions phase hoặc đã hoàn thành
                Debug.Log($"[PhaseManager] New enemy registered during Battle Phase but not executing: {enemy.gameObject.name}");
            }
        }
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (enemy != null && registeredEnemies.Remove(enemy))
        {
            CheckEnemiesCompletion();
        }
    }

    public void OnActionCompleted(ActionBase action)
    {
        if (isExecutingActions)
        {
            CheckActionsCompletion();
        }
    }

    public void OnEnemyCompleted(EnemyBase enemy)
    {
        if (isExecutingEnemies)
        {
            CheckEnemiesCompletion();
        }
    }

    private void CheckActionsCompletion()
    {
        if (!isExecutingActions) return;

        bool allCompleted = true;
        foreach (var action in registeredActions)
        {
            if (action != null && action.gameObject.activeInHierarchy && !action.IsActionCompleted())
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            allActionsCompleted = true;
        }
    }

    private void CheckEnemiesCompletion()
    {
        if (!isExecutingEnemies) return;

        bool allCompleted = true;
        foreach (var enemy in registeredEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsEnemyCompleted())
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            allEnemiesCompleted = true;
        }
    }
    
    private void CheckAndExecutePendingEnemies()
    {
        if (!isExecutingEnemies) return;
        
        foreach (var enemy in registeredEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                // Kiểm tra xem enemy đã được execute chưa
                if (!HasEnemyBeenExecuted(enemy))
                {
                    Debug.Log($"[PhaseManager] Executing pending enemy: {enemy.gameObject.name}");
                    enemy.ExecuteEnemy();
                    executedEnemies.Add(enemy); // Track enemy đã execute
                }
            }
        }
    }

    private bool HasEnemyBeenExecuted(EnemyBase enemy)
    {
        return executedEnemies.Contains(enemy);
    }

    private void RegisterAllActionsAndEnemies()
    {
        // Find and register all existing actions
        var actions = FindObjectsByType<ActionBase>(FindObjectsSortMode.None);
        foreach (var action in actions)
        {
            RegisterAction(action);
        }

        // Find and register all existing enemies
        var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            RegisterEnemy(enemy);
        }
    }

    public bool IsExecutingActions()
    {
        return isExecutingActions;
    }

    public bool IsExecutingEnemies()
    {
        return isExecutingEnemies;
    }

    private void ResetAllActionsForNewPhase()
    {   
        // Reset all actions
        foreach (var action in registeredActions)
        {
            if (action != null && action.gameObject.activeInHierarchy)
            {
                action.ResetForNewPhase();
            }
        }
        
        // Reset all enemies
        foreach (var enemy in registeredEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.ResetForNewPhase();
            }
        }
    }
}
