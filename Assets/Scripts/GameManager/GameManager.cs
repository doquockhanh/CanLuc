using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Prepare;
    [Header("Game Over")]
    [SerializeField] private bool gameOverTriggered = false;
    [SerializeField] private GameObject gameOverPanel;
    [HideInInspector] public GameResult gameResult = GameResult.Pass;

    // Events
    public System.Action<GamePhase> OnPhaseChanged;
    public System.Action OnBattlePhaseStarted;
    public System.Action OnPreparePhaseStarted;
    public System.Action<GameResult> OnGameOver; // GameResult.Pass / GameResult.Fail

    private List<IGamePhaseAware> gamePhaseAwareComponents = new List<IGamePhaseAware>();
    private readonly HashSet<EnemyStats> trackedEnemies = new HashSet<EnemyStats>();
    private EnemyBase[] enemies;
    private ActionBase[] actions;
    public EnemyBase[] Enemies => enemies;
    public ActionBase[] Actions => actions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        RegisterAllGamePhaseAwareComponents();

        InitializeEnemyTracking();

        StartCoroutine(CheckSceneManuallyToEndGame());
    }

    IEnumerator CheckSceneManuallyToEndGame()
    {
        while (true)
        {
            if (gameOverTriggered)
                yield break;

            enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            actions = FindObjectsByType<ActionBase>(FindObjectsSortMode.None);
            if (enemies.Length == 0 || actions.Length == 0)
            {
                TriggerGameOver();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void StartBattlePhase()
    {
        if (currentPhase == GamePhase.Battle) return;

        currentPhase = GamePhase.Battle;
        gameOverTriggered = false;
        OnPhaseChanged?.Invoke(currentPhase);
        OnBattlePhaseStarted?.Invoke();

        ScoreManager.Instance.ResetKillCount();

        NotifyAllGamePhaseAwareComponents();

        // Start battle phase execution through PhaseManager
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.StartBattlePhaseExecution();
        }

        Debug.Log("Game Phase: Prepare -> Battle");
    }

    public void StartPreparePhase()
    {
        if (currentPhase == GamePhase.Prepare) return;

        currentPhase = GamePhase.Prepare;
        gameOverTriggered = false;
        OnPhaseChanged?.Invoke(currentPhase);
        OnPreparePhaseStarted?.Invoke();

        NotifyAllGamePhaseAwareComponents();

        Debug.Log("Game Phase: Battle -> Prepare");
    }

    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }

    public bool IsInPreparePhase()
    {
        return currentPhase == GamePhase.Prepare;
    }

    public bool IsInBattlePhase()
    {
        return currentPhase == GamePhase.Battle;
    }

    public void TriggerGameOver(GameResult result = GameResult.Pass)
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        // Kiểm tra hệ thống nhiệm vụ: nếu còn nhiệm vụ chưa hoàn thành -> Fail
        var finalResult = result;
        if (MissionManager.Instance != null)
        {
            if (!MissionManager.Instance.AreAllMissionsDone())
            {
                finalResult = GameResult.Fail;
            }
        }

        gameResult = finalResult;
        Debug.Log($"Game Over -> {finalResult}");
        MarkLevelIfPassed(gameResult);
        gameOverPanel.SetActive(true);
        OnGameOver?.Invoke(finalResult);

    }

    private void MarkLevelIfPassed(GameResult result)
    {
        if (GameProgressManager.Instance != null && result == GameResult.Pass)
        {
            int currentFloorId = GameProgressManager.Instance.GetCurrentFloorId();
            int currentLevelId = GameProgressManager.Instance.GetCurrentLevelId();
            GameProgressManager.Instance.MarkLevelPassed(currentFloorId, currentLevelId);
        }
    }

    public void RegisterGamePhaseAwareComponent(IGamePhaseAware component)
    {
        if (component != null && !gamePhaseAwareComponents.Contains(component))
        {
            gamePhaseAwareComponents.Add(component);
        }
    }

    public void UnregisterGamePhaseAwareComponent(IGamePhaseAware component)
    {
        if (component != null)
        {
            gamePhaseAwareComponents.Remove(component);
        }
    }

    private void RegisterAllGamePhaseAwareComponents()
    {
        var components = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var component in components)
        {
            if (component is IGamePhaseAware gamePhaseAware)
            {
                RegisterGamePhaseAwareComponent(gamePhaseAware);
            }
        }
    }

    private void InitializeEnemyTracking()
    {
        trackedEnemies.Clear();

        var enemies = FindObjectsByType<EnemyStats>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            RegisterEnemy(enemy);
        }

        // Nếu đang ở battle phase và không có enemy nào -> Game Over (Pass)
        if (IsInBattlePhase() && trackedEnemies.Count == 0)
        {
            TriggerGameOver();
        }
    }

    public void RegisterEnemy(EnemyStats enemy)
    {
        if (enemy == null || trackedEnemies.Contains(enemy)) return;
        if (!enemy.gameObject.CompareTag("Enemy")) return;

        trackedEnemies.Add(enemy);
        enemy.OnDestroyed += HandleEnemyDestroyed;
    }

    public void UnregisterEnemy(EnemyStats enemy)
    {
        if (enemy == null) return;
        if (trackedEnemies.Remove(enemy))
        {
            enemy.OnDestroyed -= HandleEnemyDestroyed;
            CheckAllEnemiesCleared();
        }
    }

    private void HandleEnemyDestroyed(GameObject enemyGo)
    {
        EnemyStats stats = null;
        if (enemyGo != null)
        {
            stats = enemyGo.GetComponent<EnemyStats>();
        }

        if (stats != null)
        {
            UnregisterEnemy(stats);
        }
        else
        {
            CheckAllEnemiesCleared();
        }
    }

    private void CheckAllEnemiesCleared()
    {
        if (gameOverTriggered) return;

        if (trackedEnemies.Count == 0 && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            TriggerGameOver();
        }
    }

    private void NotifyAllGamePhaseAwareComponents()
    {
        foreach (var component in gamePhaseAwareComponents)
        {
            if (component != null)
            {
                component.OnPhaseChanged(currentPhase);

                if (currentPhase == GamePhase.Prepare)
                {
                    component.OnPreparePhaseStarted();
                }
                else if (currentPhase == GamePhase.Battle)
                {
                    component.OnBattlePhaseStarted();
                }
            }
        }
    }
}

public enum GamePhase
{
    Prepare,    // Phase chuẩn bị - người chơi setup màn chơi
    Battle      // Phase chiến đấu - các action được thực thi
}

public enum GameResult
{
    Pass,
    Fail
}
