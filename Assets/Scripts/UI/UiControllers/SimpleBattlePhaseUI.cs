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
    [SerializeField] private float fadeOutDuration = 2.0f;
    [SerializeField] private Color preparePhaseColor = Color.yellow;
    [SerializeField] private Color battlePhaseColor = Color.red;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip battleStartSound;
    
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;
    private Coroutine fadeOutCoroutine;
    
    private void Start()
    {
        SetupUI();
        UpdatePhaseDisplay();
        
        // Đăng ký với GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
            GameManager.Instance.OnBattlePhaseStarted += OnBattlePhaseStarted;
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
    
    private IEnumerator FadeOutCoroutine()
    {
        if (phaseStatusText == null) yield break;
        
        Color originalColor = phaseStatusText.color;
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            Color color = originalColor;
            color.a = Mathf.Lerp(1.0f, 0.0f, t);
            phaseStatusText.color = color;
            
            yield return null;
        }
        
        // Ẩn text hoàn toàn
        phaseStatusText.gameObject.SetActive(false);
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
        // Bắt đầu fade out effect
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        fadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
    }
    
    private void UpdateButtonVisibility()
    {
        if (startBattleButton == null) return;
        
        // Button chỉ hiện khi:
        // 1. Đang ở Prepare Phase
        // 2. Có ít nhất 1 Focusable đã tích lực
        bool shouldShow = GameManager.Instance != null && 
                         GameManager.Instance.IsInPreparePhase() && 
                         CheckIfAnyFocusableHasForce();
        
        startBattleButton.gameObject.SetActive(shouldShow);
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
