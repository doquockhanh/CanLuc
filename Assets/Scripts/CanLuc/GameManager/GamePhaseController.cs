using UnityEngine;


public class GamePhaseController : MonoBehaviour, IGamePhaseAware
{
    [Header("Game Phase Settings")]
    [SerializeField] private bool enablePhaseLogging = true;
    [SerializeField] private bool pauseInPreparePhase = false;
    [SerializeField] private bool resumeInBattlePhase = true;

    private void Start()
    {
        // Tự động đăng ký với GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterGamePhaseAwareComponent(this);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký khi component bị destroy
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterGamePhaseAwareComponent(this);
        }
    }

    #region IGamePhaseAware Implementation

    public void OnPreparePhaseStarted()
    {
        if (enablePhaseLogging)
        {
            Debug.Log($"[{gameObject.name}] Entered Prepare Phase");
        }

        // Có thể thêm logic đặc biệt cho prepare phase
        if (pauseInPreparePhase)
        {
            // Pause các component cần thiết
            Time.timeScale = 0f;
        }
    }

    public void OnBattlePhaseStarted()
    {
        if (enablePhaseLogging)
        {
            Debug.Log($"[{gameObject.name}] Entered Battle Phase");
        }

        // Có thể thêm logic đặc biệt cho battle phase
        if (resumeInBattlePhase)
        {
            // Resume các component đã bị pause
            Time.timeScale = 1f;
        }
    }

    public void OnPhaseChanged(GamePhase newPhase)
    {
        if (enablePhaseLogging)
        {
            Debug.Log($"[{gameObject.name}] Game Phase Changed to: {newPhase}");
        }

        // Xử lý logic chung khi phase thay đổi
        switch (newPhase)
        {
            case GamePhase.Prepare:
                HandlePreparePhase();
                break;
            case GamePhase.Battle:
                HandleBattlePhase();
                break;
        }
    }

    #endregion

    #region Private Methods

    private void HandlePreparePhase()
    {
        // Logic xử lý khi vào prepare phase
        // Ví dụ: bật UI setup, cho phép di chuyển object, etc.
    }

    private void HandleBattlePhase()
    {
        // Logic xử lý khi vào battle phase
        // Ví dụ: ẩn UI setup, khóa di chuyển object, etc.
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Kiểm tra xem có đang ở prepare phase không
    /// </summary>
    public bool IsInPreparePhase()
    {
        return GameManager.Instance != null && GameManager.Instance.IsInPreparePhase();
    }

    /// <summary>
    /// Kiểm tra xem có đang ở battle phase không
    /// </summary>
    public bool IsInBattlePhase()
    {
        return GameManager.Instance != null && GameManager.Instance.IsInBattlePhase();
    }

    /// <summary>
    /// Lấy game phase hiện tại
    /// </summary>
    public GamePhase GetCurrentGamePhase()
    {
        return GameManager.Instance != null ? GameManager.Instance.GetCurrentPhase() : GamePhase.Prepare;
    }

    #endregion
}
