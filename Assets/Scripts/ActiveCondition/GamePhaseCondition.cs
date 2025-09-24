using UnityEngine;

public class GamePhaseCondition : ActiveConditionBase
{
    [Header("Game Phase Settings")]
    [SerializeField] private int targetGamePhase = 4;
    [SerializeField] private bool useGreaterOrEqual = false;
    
    public override bool CheckCondition()
    {
        if (GameManager.Instance == null) return false;
        
        int currentPhase = GameManager.Instance.GetCurrentGamePhase();
        
        if (useGreaterOrEqual)
        {
            return currentPhase >= targetGamePhase;
        }
        else
        {
            return currentPhase == targetGamePhase;
        }
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (!showGizmos) return;
        
        // Hiển thị thông tin game phase
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.right, Vector3.one * 0.2f);
        
        // Hiển thị target phase
        Gizmos.color = Color.blue;
        for (int i = 0; i < targetGamePhase; i++)
        {
            Gizmos.DrawWireSphere(transform.position + Vector3.right * (i + 1) * 0.5f, 0.1f);
        }
    }
}
