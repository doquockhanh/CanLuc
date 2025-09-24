using UnityEngine;

public class ScoreCondition : ActiveConditionBase
{
    [Header("Score Settings")]
    [SerializeField] private int targetScore = 100;
    [SerializeField] private bool useGreaterOrEqual = true;
    
    public override bool CheckCondition()
    {
        if (GameManager.Instance == null) return false;
        
        int currentScore = GameManager.Instance.GetCurrentScore();
        
        if (useGreaterOrEqual)
        {
            return currentScore >= targetScore;
        }
        else
        {
            return currentScore == targetScore;
        }
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if (!showGizmos) return;
        
        // Hiển thị thông tin score
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position + Vector3.left, Vector3.one * 0.2f);
        
        // Hiển thị target score bằng số lượng sphere
        Gizmos.color = Color.yellow;
        int scoreDisplay = Mathf.Min(targetScore, 20); // Giới hạn hiển thị
        for (int i = 0; i < scoreDisplay; i++)
        {
            Vector3 pos = transform.position + Vector3.left * (i + 1) * 0.1f;
            Gizmos.DrawWireSphere(pos, 0.05f);
        }
    }
}
