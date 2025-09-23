using UnityEngine;

public class KillTargetMission : MissionBase
{
    [Header("Mission Settings")]
    [SerializeField] private EnemyStats targetStats;
    private bool isDone = false;

    private void Start()
    {
        if (targetStats != null)
        {
            RegisterToPumpkinKingEvents();
        }
    }

    private void RegisterToPumpkinKingEvents()
    {
        if (targetStats == null) return;

        if (targetStats != null)
        {
            targetStats.OnDestroyedByAction += OnTargetDestroyed;
        }
    }

    private void OnTargetDestroyed(GameObject target)
    {
        isDone = true;
    }

    public override bool IsDone()
    {
        return isDone;
    }
}
