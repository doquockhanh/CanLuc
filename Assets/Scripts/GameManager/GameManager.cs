using UnityEngine;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Prepare;

    // FocusManager đã được loại bỏ

    // Events
    public System.Action<GamePhase> OnPhaseChanged;
    public System.Action OnBattlePhaseStarted;
    public System.Action OnPreparePhaseStarted;

    // Danh sách các component implement IGamePhaseAware
    private List<IGamePhaseAware> gamePhaseAwareComponents = new List<IGamePhaseAware>();

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
    }

    /// <summary>
    /// Chuyển sang battle phase
    /// </summary>
    public void StartBattlePhase()
    {
        if (currentPhase == GamePhase.Battle) return;

        currentPhase = GamePhase.Battle;
        OnPhaseChanged?.Invoke(currentPhase);
        OnBattlePhaseStarted?.Invoke();

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
