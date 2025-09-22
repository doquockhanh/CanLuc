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
    private readonly List<EnemyBase> registeredEnemies = new List<EnemyBase>();

    // Events
    public System.Action OnActionsExecutionStarted;
    public System.Action OnActionsExecutionCompleted;
    public System.Action OnEnemiesExecutionStarted;
    public System.Action OnEnemiesExecutionCompleted;
    public System.Action OnPhaseExecutionCompleted;

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

    /// <summary>
    /// Called by GameManager when transitioning from Prepare to Battle phase
    /// </summary>
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
        
        // Step 4: Complete phase and transition back to Prepare
        OnPhaseExecutionCompleted?.Invoke();
        
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
        
        OnEnemiesExecutionStarted?.Invoke();
        
        // Execute all registered enemies
        foreach (var enemy in registeredEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.ExecuteEnemy();
            }
        }
        
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
            
            // If we're currently executing enemies phase, execute the newly registered enemy immediately
            if (isExecutingEnemies)
            {
                Debug.Log($"[PhaseManager] New enemy registered during execution phase: {enemy.gameObject.name}");
                if (enemy.gameObject.activeInHierarchy)
                {
                    enemy.ExecuteEnemy();
                }
            }
        }
    }

    /// <summary>
    /// Unregister an enemy
    /// </summary>
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (enemy != null && registeredEnemies.Remove(enemy))
        {
            CheckEnemiesCompletion();
        }
    }

    /// <summary>
    /// Called by ActionBase when an action completes
    /// </summary>
    public void OnActionCompleted(ActionBase action)
    {
        if (isExecutingActions)
        {
            CheckActionsCompletion();
        }
    }

    /// <summary>
    /// Called by EnemyBase when an enemy completes
    /// </summary>
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

    /// <summary>
    /// Check if currently executing actions phase
    /// </summary>
    public bool IsExecutingActions()
    {
        return isExecutingActions;
    }

    /// <summary>
    /// Check if currently executing enemies phase
    /// </summary>
    public bool IsExecutingEnemies()
    {
        return isExecutingEnemies;
    }

    /// <summary>
    /// Get count of registered actions
    /// </summary>
    public int GetActionCount()
    {
        return registeredActions.Count;
    }

    /// <summary>
    /// Get count of registered enemies
    /// </summary>
    public int GetEnemyCount()
    {
        return registeredEnemies.Count;
    }

    /// <summary>
    /// Reset all actions and enemies for the next phase cycle
    /// </summary>
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
