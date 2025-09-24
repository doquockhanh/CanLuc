using System.Collections.Generic;
using UnityEngine;

public class ActiveConditionManager : MonoBehaviour
{
    public static ActiveConditionManager Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private List<ActiveConditionBase> allConditions = new List<ActiveConditionBase>();
    private List<ActiveConditionBase> mustBeActiveConditions = new List<ActiveConditionBase>();
    
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
        // Kiểm tra tất cả conditions chưa được kích hoạt
        foreach (var condition in allConditions)
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
    
    public void UnregisterCondition(ActiveConditionBase condition)
    {
        if (condition == null) return;
        
        allConditions.Remove(condition);
        mustBeActiveConditions.Remove(condition);
        
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
