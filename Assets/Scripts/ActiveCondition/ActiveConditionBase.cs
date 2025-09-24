using UnityEngine;

public abstract class ActiveConditionBase : MonoBehaviour
{
    [Header("Must Be Active")]
    [Tooltip("Nếu true, gameobject này phải active để có thể end game")]
    public bool mustBeActive = false;
    
    [Header("Debug")]
    [Tooltip("Hiển thị gizmos trong Scene View")]
    public bool showGizmos = true;
    
    [Header("Condition Settings")]
    [Tooltip("Tự động kiểm tra điều kiện mỗi frame")]
    public bool autoCheck = true;
    
    public bool IsActive { get; protected set; }
    public System.Action<GameObject> OnConditionMet;
    
    protected bool hasBeenActivated = false;
    
    public bool HasBeenActivated => hasBeenActivated;
    
    protected virtual void Start()
    {
        // Đăng ký với Manager
        if (ActiveConditionManager.Instance != null)
        {
            ActiveConditionManager.Instance.RegisterCondition(this);
        }
        
        // Ẩn gameobject ban đầu
        gameObject.SetActive(false);
    }
    
    protected virtual void Update()
    {
        // Logic kiểm tra đã được chuyển sang Manager
        // Update chỉ để xử lý các logic khác nếu cần
    }
    
    protected virtual void OnDestroy()
    {
        // Hủy đăng ký với Manager
        if (ActiveConditionManager.Instance != null)
        {
            ActiveConditionManager.Instance.UnregisterCondition(this);
        }
    }
    
    protected virtual void CheckAndActivate()
    {
        Debug.Log("CheckAndActivate: " + CheckCondition());
        if (CheckCondition())
        {
            ActivateObject();
        }
    }
    
    protected virtual void ActivateObject()
    {
        if (hasBeenActivated) return;
        
        hasBeenActivated = true;
        IsActive = true;
        gameObject.SetActive(true);
        
        // Phát event
        OnConditionMet?.Invoke(gameObject);
        ActiveConditionManager.Instance?.OnObjectActivated?.Invoke(gameObject);
    }
    
    public void ActivateObjectPublic()
    {
        ActivateObject();
    }
    
    public abstract bool CheckCondition();
    
    protected virtual void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Làm mờ gameobject trong Scene View
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        // Hiển thị trạng thái
        Gizmos.color = IsActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        
        // Hiển thị mustBeActive
        if (mustBeActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
        }
    }
}
