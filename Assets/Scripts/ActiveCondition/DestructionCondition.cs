using UnityEngine;

public class DestructionCondition : ActiveConditionBase
{
    [Header("Destruction Settings")]
    [SerializeField] private GameObject targetObject;
    
    private bool wasTargetDestroyed = false;
    
    protected override void Start()
    {
        base.Start();
        
        if (targetObject == null)
        {
            Debug.LogWarning($"DestructionCondition on {gameObject.name}: targetObject is null!");
        }
    }
    
    public override bool CheckCondition()
    {
        // Nếu đã được kích hoạt rồi thì không cần kiểm tra nữa
        if (wasTargetDestroyed) return true;
        
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
        targetObject = target;
        wasTargetDestroyed = false;
    }
}
