using UnityEngine;

public class KillPumpkinKingMission : MissionBase
{
    [Header("Mission Settings")]
    [SerializeField] private PumpingKingController targetPumpkinKing;
    private bool isDone = false;

    private void Start()
    {
        if (targetPumpkinKing != null)
        {
            RegisterToPumpkinKingEvents();
        }
    }

    private void RegisterToPumpkinKingEvents()
    {
        if (targetPumpkinKing == null) return;

        // Lấy EnemyStats component để đăng ký event
        EnemyStats enemyStats = targetPumpkinKing.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            enemyStats.OnDestroyedByAction += OnPumpkinKingDestroyed;
        }
    }

    /// <summary>
    /// Callback khi PumpkinKing bị tiêu diệt
    /// </summary>
    private void OnPumpkinKingDestroyed(GameObject destroyedPumpkinKing)
    {
        isDone = true;
    }

    /// <summary>
    /// Kiểm tra xem nhiệm vụ đã hoàn thành chưa
    /// </summary>
    public override bool IsDone()
    {
        return isDone;
    }

    /// <summary>
    /// Lấy thông tin trạng thái nhiệm vụ
    /// </summary>
    public string GetMissionStatus()
    {
        if (targetPumpkinKing == null)
        {
            return "PumpkinKing not found";
        }

        EnemyStats enemyStats = targetPumpkinKing.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            if (enemyStats.IsAlive)
            {
                return $"PumpkinKing HP: {enemyStats.CurrentHealth}/{enemyStats.MaxHealth}";
            }
            else
            {
                return "PumpkinKing destroyed - Mission Complete!";
            }
        }

        return "PumpkinKing status unknown";
    }

}
