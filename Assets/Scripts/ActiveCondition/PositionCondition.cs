using UnityEngine;

public class PositionCondition : ActiveConditionBase
{
    [Header("Position Settings")]
    [SerializeField] private Transform targetObject;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float checkRadius = 1f;
    [SerializeField] private bool useTargetObjectPosition = false;
    
    protected override void Start()
    {
        base.Start();
        
        if (useTargetObjectPosition && targetObject == null)
        {
            Debug.LogWarning($"PositionCondition on {gameObject.name}: targetObject is null but useTargetObjectPosition is true!");
        }
    }
    
    public override bool CheckCondition()
    {
        Vector3 checkPosition;
        
        if (useTargetObjectPosition && targetObject != null)
        {
            checkPosition = targetObject.position;
        }
        else
        {
            checkPosition = targetPosition;
        }
        
        float distance = Vector3.Distance(transform.position, checkPosition);
        return distance <= checkRadius;
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (!showGizmos) return;
        
        Vector3 checkPosition;
        
        if (useTargetObjectPosition && targetObject != null)
        {
            checkPosition = targetObject.position;
            
            // Vẽ line đến target object
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, checkPosition);
            
            // Hiển thị target object
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(checkPosition, Vector3.one * 0.5f);
        }
        else
        {
            checkPosition = targetPosition;
            
            // Hiển thị target position
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(checkPosition, Vector3.one * 0.5f);
        }
        
        // Hiển thị check radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(checkPosition, checkRadius);
        
        // Hiển thị khoảng cách hiện tại
        float currentDistance = Vector3.Distance(transform.position, checkPosition);
        Gizmos.color = currentDistance <= checkRadius ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, checkPosition);
    }
    
    public void SetTargetObject(Transform target)
    {
        targetObject = target;
        useTargetObjectPosition = true;
    }
    
    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        useTargetObjectPosition = false;
    }
}
