using UnityEngine;

public class DestructionCondition : ActiveConditionBase
{
    [Header("Destruction Settings")]
    [SerializeField] private GameObject targetObject;
    
    private bool wasTargetDestroyed = false;
    private bool hasTargetBeenActivated = false; // Theo dõi target đã được active chưa
    
    protected override void Start()
    {
        base.Start();
        
        if (targetObject == null)
        {
            Debug.LogWarning($"DestructionCondition on {gameObject.name}: targetObject is null!");
        }
        else
        {
            // Chỉ theo dõi nếu targetObject có ActiveConditionBase
            if (HasActiveConditionBase(targetObject))
            {
                // Đăng ký với Manager để theo dõi
                RegisterForTargetMonitoring();
            }
            else
            {
                // Nếu không có ActiveConditionBase, coi như đã active
                hasTargetBeenActivated = true;
            }
        }
    }
    
    private bool HasActiveConditionBase(GameObject obj)
    {
        if (obj == null) return false;
        
        // Kiểm tra chính object đó
        if (obj.GetComponent<ActiveConditionBase>() != null) return true;
        
        // Kiểm tra các child objects
        var activeConditions = obj.GetComponentsInChildren<ActiveConditionBase>();
        return activeConditions.Length > 0;
    }
    
    private void RegisterForTargetMonitoring()
    {
        // Đăng ký callback với Manager để được thông báo khi target được active
        if (ActiveConditionManager.Instance != null)
        {
            ActiveConditionManager.Instance.OnObjectActivated += OnTargetObjectActivated;
        }
    }
    
    private void OnTargetObjectActivated(GameObject activatedObject)
    {
        // Kiểm tra xem object được active có phải là targetObject không
        if (targetObject != null && (activatedObject == targetObject || IsChildOfTarget(activatedObject)))
        {
            hasTargetBeenActivated = true;
            // Hủy đăng ký callback
            if (ActiveConditionManager.Instance != null)
            {
                ActiveConditionManager.Instance.OnObjectActivated -= OnTargetObjectActivated;
            }
        }
    }
    
    private bool IsChildOfTarget(GameObject obj)
    {
        if (targetObject == null || obj == null) return false;
        return obj.transform.IsChildOf(targetObject.transform);
    }
    
    public override bool CheckCondition()
    {
        // Nếu đã được kích hoạt rồi thì không cần kiểm tra nữa
        if (wasTargetDestroyed) return true;
        
        // Chỉ kiểm tra nếu targetObject đã được active ít nhất một lần
        if (!hasTargetBeenActivated) return false;
        
        // Kiểm tra nếu object bị destroy (null)
        if (targetObject == null)
        {
            wasTargetDestroyed = true;
            return true;
        }
        
        // Kiểm tra nếu object không active (có thể bị "destroy" logic)
        if (!targetObject.activeInHierarchy)
        {
            wasTargetDestroyed = true;
            return true;
        }
        
        return false;
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (!showGizmos) return;
        
        // Vẽ line đến target object
        if (targetObject != null)
        {
            Gizmos.color = wasTargetDestroyed ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);
            
            // Hiển thị target object
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(targetObject.transform.position, Vector3.one * 0.5f);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(transform.position + Vector3.up, Vector3.one * 0.3f);
        }
    }
    
    public void SetTargetObject(GameObject target)
    {
        // Hủy đăng ký callback cũ nếu có
        if (ActiveConditionManager.Instance != null)
        {
            ActiveConditionManager.Instance.OnObjectActivated -= OnTargetObjectActivated;
        }
        
        targetObject = target;
        wasTargetDestroyed = false;
        hasTargetBeenActivated = false;
        
        // Bắt đầu theo dõi target mới
        if (target != null)
        {
            if (HasActiveConditionBase(target))
            {
                RegisterForTargetMonitoring();
            }
            else
            {
                hasTargetBeenActivated = true;
            }
        }
    }
    
    protected override void OnDestroy()
    {
        // Hủy đăng ký callback khi destroy
        if (ActiveConditionManager.Instance != null)
        {
            ActiveConditionManager.Instance.OnObjectActivated -= OnTargetObjectActivated;
        }
        
        base.OnDestroy();
    }
}
