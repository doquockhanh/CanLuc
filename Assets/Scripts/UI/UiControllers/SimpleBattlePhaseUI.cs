using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimpleBattlePhaseUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI phaseStatusText;
    public Button startBattleButton;
    public AudioSource audioSource;
    
    [Header("Animation Settings")]
    [SerializeField] private float blinkSpeed = 1.0f;
    [SerializeField] private Color preparePhaseColor = Color.yellow;
    [SerializeField] private Color battlePhaseColor = Color.red;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip battleStartSound;
    
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;
    
    private void Start()
    {
        SetupUI();
        UpdatePhaseDisplay();
        
        // Đăng ký với GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
            GameManager.Instance.OnBattlePhaseStarted += OnBattlePhaseStarted;
            GameManager.Instance.OnPreparePhaseStarted += OnPreparePhaseStarted;
        }
        
        // Đăng ký với PhaseManager events để theo dõi execution phases
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnActionsExecutionStarted += OnActionsExecutionStarted;
            PhaseManager.Instance.OnActionsExecutionCompleted += OnActionsExecutionCompleted;
            PhaseManager.Instance.OnEnemiesExecutionStarted += OnEnemiesExecutionStarted;
            PhaseManager.Instance.OnEnemiesExecutionCompleted += OnEnemiesExecutionCompleted;
        }
        
        // Đăng ký với ActionBase events để theo dõi khi có Focusable tích lực
        ActionBase.OnFocusChanged += OnFocusChanged;
    }
    
    private float lastCheckTime = 0f;
    private const float CHECK_INTERVAL = 0.5f; // Kiểm tra mỗi 0.5 giây
    
    private void Update()
    {
        // Kiểm tra và cập nhật button visibility định kỳ khi ở Prepare Phase
        // Sử dụng Update với tần suất thấp để tránh lag
        if (GameManager.Instance != null && GameManager.Instance.IsInPreparePhase())
        {
            if (Time.time - lastCheckTime >= CHECK_INTERVAL)
            {
                UpdateButtonVisibility();
                lastCheckTime = Time.time;
            }
        }
    }
    
    private void OnDestroy()
    {
        // Hủy đăng ký events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            GameManager.Instance.OnBattlePhaseStarted -= OnBattlePhaseStarted;
            GameManager.Instance.OnPreparePhaseStarted -= OnPreparePhaseStarted;
        }
        
        // Hủy đăng ký PhaseManager events
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnActionsExecutionStarted -= OnActionsExecutionStarted;
            PhaseManager.Instance.OnActionsExecutionCompleted -= OnActionsExecutionCompleted;
            PhaseManager.Instance.OnEnemiesExecutionStarted -= OnEnemiesExecutionStarted;
            PhaseManager.Instance.OnEnemiesExecutionCompleted -= OnEnemiesExecutionCompleted;
        }
        
        // Hủy đăng ký ActionBase events
        ActionBase.OnFocusChanged -= OnFocusChanged;
    }
    
    private void SetupUI()
    {
        // Setup button click event
        if (startBattleButton != null)
        {
            startBattleButton.onClick.AddListener(OnStartBattleClicked);
        }
        
        // Ẩn button ban đầu
        UpdateButtonVisibility();
    }
    
    private void UpdatePhaseDisplay()
    {
        if (phaseStatusText == null || GameManager.Instance == null) return;
        
        GamePhase currentPhase = GameManager.Instance.GetCurrentPhase();
        
        // Ensure text is visible
        phaseStatusText.gameObject.SetActive(true);
        
        switch (currentPhase)
        {
            case GamePhase.Prepare:
                phaseStatusText.text = "Giai đoạn chuẩn bị";
                phaseStatusText.color = preparePhaseColor;
                StartBlinking();
                break;
                
            case GamePhase.Battle:
                phaseStatusText.text = "CHIẾN ĐẤU";
                phaseStatusText.color = battlePhaseColor;
                StopBlinking();
                break;
        }
    }
    
    private void StartBlinking()
    {
        if (isBlinking) return;
        
        isBlinking = true;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }
    
    private void StopBlinking()
    {
        isBlinking = false;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        // Đảm bảo text hiển thị đầy đủ
        if (phaseStatusText != null)
        {
            phaseStatusText.alpha = 1.0f;
        }
    }
    
    private IEnumerator BlinkCoroutine()
    {
        while (isBlinking)
        {
            // Fade out
            yield return StartCoroutine(FadeText(1.0f, 0.3f, blinkSpeed * 0.5f));
            
            // Fade in
            yield return StartCoroutine(FadeText(0.3f, 1.0f, blinkSpeed * 0.5f));
        }
    }
    
    private IEnumerator FadeText(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = phaseStatusText.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            phaseStatusText.color = color;
            yield return null;
        }
        
        color.a = toAlpha;
        phaseStatusText.color = color;
    }
    
    
    private void OnStartBattleClicked()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsInPreparePhase()) return;
        
        // Phát âm thanh
        if (audioSource != null && battleStartSound != null)
        {
            audioSource.PlayOneShot(battleStartSound);
        }
        
        // Chuyển sang Battle Phase
        GameManager.Instance.StartBattlePhase();
        
        // Ẩn button
        if (startBattleButton != null)
        {
            startBattleButton.gameObject.SetActive(false);
        }
    }
    
    private void OnPhaseChanged(GamePhase newPhase)
    {
        // Ensure phase status text is visible when phase changes
        if (phaseStatusText != null)
        {
            phaseStatusText.gameObject.SetActive(true);
        }
        
        UpdatePhaseDisplay();
        UpdateButtonVisibility();
    }
    
    private void OnFocusChanged(ActionBase previous, ActionBase current)
    {
        // Cập nhật button visibility khi focus thay đổi
        // Vì có thể có Focusable mới được tích lực
        UpdateButtonVisibility();
    }
    
    private void OnBattlePhaseStarted()
    {
        // Don't fade out completely, just update the display
        // The phase display will be updated by OnPhaseChanged
        UpdatePhaseDisplay();
    }
    
    private void OnPreparePhaseStarted()
    {
        // Reset UI state when returning to prepare phase
        UpdatePhaseDisplay();
        UpdateButtonVisibility();
    }
    
    private void OnActionsExecutionStarted()
    {
        if (phaseStatusText != null)
        {
            phaseStatusText.text = "Thực thi hành động...";
            phaseStatusText.color = Color.cyan;
            StopBlinking();
        }
    }
    
    private void OnActionsExecutionCompleted()
    {
        if (phaseStatusText != null)
        {
            phaseStatusText.text = "Hành động hoàn thành";
            phaseStatusText.color = Color.green;
        }
    }
    
    private void OnEnemiesExecutionStarted()
    {
        if (phaseStatusText != null)
        {
            phaseStatusText.text = "Kẻ thù di chuyển...";
            phaseStatusText.color = Color.red;
        }
    }
    
    private void OnEnemiesExecutionCompleted()
    {
        if (phaseStatusText != null)
        {
            phaseStatusText.text = "Kẻ thù hoàn thành";
            phaseStatusText.color = Color.magenta;
        }
    }
    
    private void UpdateButtonVisibility()
    {
        if (startBattleButton == null) return;
        
        // Button chỉ hiện khi:
        // 1. Đang ở Prepare Phase
        // 2. Có ít nhất 1 Focusable đã tích lực
        // 3. Không đang trong quá trình execution
        bool shouldShow = GameManager.Instance != null && 
                         GameManager.Instance.IsInPreparePhase() && 
                         CheckIfAnyFocusableHasForce() &&
                         !IsCurrentlyExecuting();
              
        startBattleButton.gameObject.SetActive(shouldShow);
    }
    
    private bool IsCurrentlyExecuting()
    {
        // Check if PhaseManager is currently executing actions or enemies
        if (PhaseManager.Instance != null)
        {
            return PhaseManager.Instance.IsExecutingActions() || PhaseManager.Instance.IsExecutingEnemies();
        }
        return false;
    }
    
    private bool CheckIfAnyFocusableHasForce()
    {
        // Tìm tất cả ActionBase trong scene
        ActionBase[] allFocusables = FindObjectsByType<ActionBase>(FindObjectsSortMode.None);
        
        foreach (var focusable in allFocusables)
        {
            if (focusable == null) continue;
            
            // Kiểm tra xem Focusable này có đã tích lực chưa
            if (HasAccumulatedForce(focusable))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool HasAccumulatedForce(ActionBase focusable)
    {
        // Kiểm tra thông qua ForceAccumulator
        var forceAccumulator = focusable.GetComponent<ForceAccumulator>();
        if (forceAccumulator != null)
        {
            // Có lực nếu tổng lực > 0 hoặc đã hoàn thành ít nhất 1 thanh
            return forceAccumulator.CurrentForce > 0f || forceAccumulator.CompletedBarsCount > 0;
        }
        
        return false;
    }
}
