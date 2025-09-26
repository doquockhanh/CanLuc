using System.Collections.Generic;
using UnityEngine;

public class ActiveConditionManager : MonoBehaviour
{
    public static ActiveConditionManager Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Check Settings")]
    [SerializeField] private float checkInterval = 0.1f; // Giảm tần suất kiểm tra
    
    private List<ActiveConditionBase> allConditions = new List<ActiveConditionBase>();
    private List<ActiveConditionBase> mustBeActiveConditions = new List<ActiveConditionBase>();
    
    // Phân loại conditions theo loại component
    private List<ActiveConditionBase> enemyConditions = new List<ActiveConditionBase>();
    private List<ActiveConditionBase> actionConditions = new List<ActiveConditionBase>();
    private List<ActiveConditionBase> otherConditions = new List<ActiveConditionBase>();
    
    private float lastCheckTime;
    
    public System.Action<GameObject> OnObjectActivated;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {   
        // Giảm tần suất kiểm tra
        if (Time.time - lastCheckTime < checkInterval) return;
        lastCheckTime = Time.time;
        
        // Kiểm tra conditions theo phase hiện tại
        CheckConditionsByPhase();
    }
    
    private void CheckConditionsByPhase()
    {
        // Lấy phase hiện tại
        bool isBattlePhase = GameManager.Instance != null && GameManager.Instance.IsInBattlePhase();
        bool isPreparePhase = GameManager.Instance != null && GameManager.Instance.IsInPreparePhase();
        
        // Kiểm tra enemy conditions chỉ từ khi OnEnemiesExecutionStarted đến hết Battle Phase
        if (isBattlePhase && PhaseManager.Instance != null && PhaseManager.Instance.IsExecutingEnemies())
        {
            CheckConditionsList(enemyConditions);
        }
        
        // Kiểm tra action conditions chỉ ở Prepare Phase
        if (isPreparePhase)
        {
            CheckConditionsList(actionConditions);
        }
        
        // Kiểm tra other conditions ở cả hai phase
        CheckConditionsList(otherConditions);
    }
    
    private void CheckConditionsList(List<ActiveConditionBase> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition != null && !condition.HasBeenActivated)
            {
                if (condition.CheckCondition())
                {
                    condition.ActivateObjectPublic();
                    
                    // Đăng ký các component mới với PhaseManager
                    RegisterNewComponents(condition.gameObject);
                }
            }
        }
    }
    
    private void RegisterNewComponents(GameObject activatedObject)
    {
        // Đăng ký ActionBase nếu có
        var actions = activatedObject.GetComponentsInChildren<ActionBase>();
        foreach (var action in actions)
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.RegisterAction(action);
            }
        }
        
        // Đăng ký EnemyBase nếu có
        var enemies = activatedObject.GetComponentsInChildren<EnemyBase>();
        foreach (var enemy in enemies)
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.RegisterEnemy(enemy);
            }
        }
    }
    
    public void RegisterCondition(ActiveConditionBase condition)
    {
        if (condition == null) return;
        
        if (!allConditions.Contains(condition))
        {
            allConditions.Add(condition);
            
            // Phân loại condition theo loại component
            CategorizeCondition(condition);
            
            if (condition.mustBeActive)
            {
                mustBeActiveConditions.Add(condition);
                if (showDebugLogs)
                {
                    Debug.Log($"Registered mustBeActive condition: {condition.name}");
                }
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"Registered condition: {condition.name}");
            }
        }
    }
    
    private void CategorizeCondition(ActiveConditionBase condition)
    {
        // Kiểm tra xem condition có chứa EnemyBase không
        var enemies = condition.GetComponentsInChildren<EnemyBase>();
        if (enemies.Length > 0)
        {
            enemyConditions.Add(condition);
            return;
        }
        
        // Kiểm tra xem condition có chứa ActionBase không
        var actions = condition.GetComponentsInChildren<ActionBase>();
        if (actions.Length > 0)
        {
            actionConditions.Add(condition);
            return;
        }
        
        // Các condition khác
        otherConditions.Add(condition);
    }
    
    public void UnregisterCondition(ActiveConditionBase condition)
    {
        if (condition == null) return;
        
        allConditions.Remove(condition);
        mustBeActiveConditions.Remove(condition);
        
        // Xóa khỏi các danh sách phân loại
        enemyConditions.Remove(condition);
        actionConditions.Remove(condition);
        otherConditions.Remove(condition);
        
        if (showDebugLogs)
        {
            Debug.Log($"Unregistered condition: {condition.name}");
        }
    }
    
    public bool CanEndGame()
    {
        // Kiểm tra tất cả mustBeActive conditions
        foreach (var condition in mustBeActiveConditions)
        {
            if (condition == null) continue;
            
            if (!condition.IsActive)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public List<ActiveConditionBase> GetAllConditions()
    {
        return new List<ActiveConditionBase>(allConditions);
    }
    
    public List<ActiveConditionBase> GetMustBeActiveConditions()
    {
        return new List<ActiveConditionBase>(mustBeActiveConditions);
    }
    
    public int GetActiveMustBeActiveCount()
    {
        int count = 0;
        foreach (var condition in mustBeActiveConditions)
        {
            if (condition != null && condition.IsActive)
            {
                count++;
            }
        }
        return count;
    }
    
    public int GetTotalMustBeActiveCount()
    {
        return mustBeActiveConditions.Count;
    }
    
    private void OnGUI()
    {
        if (!showDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Total Conditions: {allConditions.Count}");
        GUILayout.Label($"Must Be Active: {GetActiveMustBeActiveCount()}/{GetTotalMustBeActiveCount()}");
        GUILayout.Label($"Can End Game: {CanEndGame()}");
        GUILayout.EndArea();
    }
}
