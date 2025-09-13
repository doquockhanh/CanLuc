using UnityEngine;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Prepare;

    [Header("Game Over")]
    [SerializeField] private bool gameOverTriggered = false;

    // FocusManager đã được loại bỏ

    // Events
    public System.Action<GamePhase> OnPhaseChanged;
    public System.Action OnBattlePhaseStarted;
    public System.Action OnPreparePhaseStarted;
    public System.Action<GameResult> OnGameOver; // GameResult.Pass / GameResult.Fail

    // Danh sách các component implement IGamePhaseAware
    private List<IGamePhaseAware> gamePhaseAwareComponents = new List<IGamePhaseAware>();

    // Theo dõi enemy trong scene
    private readonly HashSet<EnemyStats> trackedEnemies = new HashSet<EnemyStats>();

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

        // Không còn phụ thuộc FocusManager
    }

    private void Start()
    {
        // Tìm và đăng ký tất cả component implement IGamePhaseAware
        RegisterAllGamePhaseAwareComponents();

        // Khởi tạo theo dõi enemy hiện có trong scene
        InitializeEnemyTracking();
    }

    private void Update()
    {
        // Phòng hờ: nếu vì lý do nào đó event không bắn, kiểm tra rỗng tag "Enemy"
        if (!gameOverTriggered && IsInBattlePhase())
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                TriggerGameOver(GameResult.Pass);
            }
        }
    }

    /// <summary>
    /// Chuyển sang battle phase
    /// </summary>
    public void StartBattlePhase()
    {
        if (currentPhase == GamePhase.Battle) return;

        currentPhase = GamePhase.Battle;
        gameOverTriggered = false;
        OnPhaseChanged?.Invoke(currentPhase);
        OnBattlePhaseStarted?.Invoke();

        // Reset kill count khi bắt đầu battle phase
        ResetKillCount();

        // Thông báo cho tất cả component implement IGamePhaseAware
        NotifyAllGamePhaseAwareComponents();

        Debug.Log("Game Phase: Prepare -> Battle");
    }

    /// <summary>
    /// Chuyển về prepare phase
    /// </summary>
    public void StartPreparePhase()
    {
        if (currentPhase == GamePhase.Prepare) return;

        currentPhase = GamePhase.Prepare;
        gameOverTriggered = false;
        OnPhaseChanged?.Invoke(currentPhase);
        OnPreparePhaseStarted?.Invoke();

        // Thông báo cho tất cả component implement IGamePhaseAware
        NotifyAllGamePhaseAwareComponents();

        Debug.Log("Game Phase: Battle -> Prepare");
    }

    /// <summary>
    /// Lấy trạng thái phase hiện tại
    /// </summary>
    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }

    /// <summary>
    /// Kiểm tra xem có đang ở prepare phase không
    /// </summary>
    public bool IsInPreparePhase()
    {
        return currentPhase == GamePhase.Prepare;
    }

    /// <summary>
    /// Kiểm tra xem có đang ở battle phase không
    /// </summary>
    public bool IsInBattlePhase()
    {
        return currentPhase == GamePhase.Battle;
    }

    /// <summary>
    /// Gọi khi Game Over. Mặc định: Pass theo yêu cầu.
    /// </summary>
    public void TriggerGameOver(GameResult result = GameResult.Pass)
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        Debug.Log($"Game Over -> {result}");
        OnGameOver?.Invoke(result);
    }


    /// <summary>
    /// Reset game về prepare phase (có thể dùng cho restart level)
    /// </summary>
    public void ResetToPreparePhase()
    {
        StartPreparePhase();
    }

    /// <summary>
    /// Đăng ký component implement IGamePhaseAware
    /// </summary>
    public void RegisterGamePhaseAwareComponent(IGamePhaseAware component)
    {
        if (component != null && !gamePhaseAwareComponents.Contains(component))
        {
            gamePhaseAwareComponents.Add(component);
        }
    }

    /// <summary>
    /// Hủy đăng ký component implement IGamePhaseAware
    /// </summary>
    public void UnregisterGamePhaseAwareComponent(IGamePhaseAware component)
    {
        if (component != null)
        {
            gamePhaseAwareComponents.Remove(component);
        }
    }

    /// <summary>
    /// Tìm và đăng ký tất cả component implement IGamePhaseAware trong scene
    /// </summary>
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

    /// <summary>
    /// Tìm và đăng ký tất cả EnemyStats đang tồn tại trong scene
    /// </summary>
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
            TriggerGameOver(GameResult.Pass);
        }
    }

    /// <summary>
    /// Đăng ký một enemy để theo dõi
    /// </summary>
    public void RegisterEnemy(EnemyStats enemy)
    {
        if (enemy == null || trackedEnemies.Contains(enemy)) return;
        if (!enemy.gameObject.CompareTag("Enemy")) return;

        trackedEnemies.Add(enemy);
        enemy.OnDestroyed += HandleEnemyDestroyed;
    }

    /// <summary>
    /// Hủy đăng ký một enemy
    /// </summary>
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
        // Khi enemy bị phá hủy qua EnemyStats, event này sẽ bắn
        // Cố gắng lấy EnemyStats nếu còn
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
            // Không lấy được component, kiểm tra lại toàn cục
            CheckAllEnemiesCleared();
        }
    }

    private void CheckAllEnemiesCleared()
    {
        if (gameOverTriggered) return;

        // Khi trackedEnemies rỗng, double-check trong scene theo tag để đảm bảo chắc chắn
        if (trackedEnemies.Count == 0 && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            // Phát âm thanh chiến thắng trước khi trigger game over
            PlayVictorySounds();
            TriggerGameOver(GameResult.Pass);
        }
    }

    /// <summary>
    /// Phát âm thanh chiến thắng khi tất cả enemy bị tiêu diệt
    /// </summary>
    private void PlayVictorySounds()
    {
        if (KillSoundManager.Instance != null)
        {
            KillSoundManager.Instance.PlayVictorySounds();
        }
        else
        {
            Debug.LogWarning("[GameManager] KillSoundManager không tìm thấy! Không thể phát âm thanh chiến thắng.");
        }
    }

    /// <summary>
    /// Reset kill count về 0
    /// </summary>
    private void ResetKillCount()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetKillCount();
        }
        else
        {
            Debug.LogWarning("[GameManager] ScoreManager không tìm thấy! Không thể reset kill count.");
        }
    }

    /// <summary>
    /// Thông báo cho tất cả component implement IGamePhaseAware
    /// </summary>
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

/// <summary>
/// Các trạng thái của game
/// </summary>
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
