using UnityEngine;
using UnityEngine.UI;


public class OffScreenIndicator : MonoBehaviour
{
    [Header("Indicator Components")]
    [SerializeField] private Image indicatorImage;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;

    [Header("Visual Effects")]
    [SerializeField] private bool enablePulsing = true;
    [SerializeField] private bool enableFadeInOut = true;
    [SerializeField] private bool enableRotation = true;

    private OffScreenTracker targetTracker;
    private Vector3 targetWorldPosition;
    private bool isVisible = false;
    private float currentAlpha = 0f;
    private float pulseTimer = 0f;

    // Animation states
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    private void Awake()
    {
        // Tìm các component cần thiết
        if (indicatorImage == null)
            indicatorImage = GetComponent<Image>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Tạo CanvasGroup nếu chưa có
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Thiết lập ban đầu
        SetAlpha(0f);
    }

    private void Update()
    {
        if (targetTracker == null) return;

        // Cập nhật vị trí target
        targetWorldPosition = targetTracker.transform.position;

        // Cập nhật animation
        UpdateAnimation();
    }

    /// <summary>
    /// Thiết lập tracker target cho indicator này
    /// </summary>
    public void SetTargetTracker(OffScreenTracker tracker)
    {
        targetTracker = tracker;

        if (tracker != null)
        {
            // Thiết lập màu sắc tùy chỉnh nếu có
            Color customColor = tracker.GetCustomIndicatorColor();
            if (customColor != Color.clear)
            {
                SetIndicatorColor(customColor);
            }

            // Thiết lập kích thước tùy chỉnh nếu có
            float customScale = tracker.GetCustomIndicatorScale();
            if (customScale != 1f)
            {
                SetIndicatorScale(customScale);
            }
        }
    }

    /// <summary>
    /// Hiển thị indicator
    /// </summary>
    public void Show()
    {
        if (isVisible) return;

        isVisible = true;

        if (enableFadeInOut)
        {
            StartFadeIn();
        }
        else
        {
            SetAlpha(1f);
        }
    }

    /// <summary>
    /// Ẩn indicator
    /// </summary>
    public void Hide()
    {
        if (!isVisible) return;

        isVisible = false;

        if (enableFadeInOut)
        {
            StartFadeOut();
        }
        else
        {
            SetAlpha(0f);
        }
    }

    /// <summary>
    /// Thiết lập vị trí của indicator
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }
    }

    /// <summary>
    /// Thiết lập rotation của indicator
    /// </summary>
    public void SetRotation(float angle)
    {
        if (enableRotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// Thiết lập màu sắc của indicator
    /// </summary>
    public void SetIndicatorColor(Color color)
    {
        if (indicatorImage != null)
        {
            indicatorImage.color = color;
        }
    }

    /// <summary>
    /// Thiết lập kích thước của indicator
    /// </summary>
    public void SetIndicatorScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// Bật/tắt hiệu ứng pulsing
    /// </summary>
    public void SetPulsingEnabled(bool enabled)
    {
        enablePulsing = enabled;
    }

    /// <summary>
    /// Bật/tắt hiệu ứng fade in/out
    /// </summary>
    public void SetFadeInOutEnabled(bool enabled)
    {
        enableFadeInOut = enabled;
    }

    /// <summary>
    /// Bật/tắt hiệu ứng rotation
    /// </summary>
    public void SetRotationEnabled(bool enabled)
    {
        enableRotation = enabled;
    }

    private void StartFadeIn()
    {
        if (isFadingIn) return;

        isFadingIn = true;
        isFadingOut = false;

        StartCoroutine(FadeCoroutine(0f, 1f, fadeInDuration, () =>
        {
            isFadingIn = false;
        }));
    }

    private void StartFadeOut()
    {
        if (isFadingOut) return;

        isFadingOut = true;
        isFadingIn = false;

        StartCoroutine(FadeCoroutine(1f, 0f, fadeOutDuration, () =>
        {
            isFadingOut = false;
        }));
    }

    private System.Collections.IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            SetAlpha(currentAlpha);

            yield return null;
        }

        SetAlpha(endAlpha);
        onComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        currentAlpha = alpha;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
        else if (indicatorImage != null)
        {
            Color color = indicatorImage.color;
            color.a = alpha;
            indicatorImage.color = color;
        }
    }

    private void UpdateAnimation()
    {
        if (!isVisible) return;

        // Cập nhật pulsing effect
        if (enablePulsing)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulseValue = 1f + Mathf.Sin(pulseTimer) * pulseIntensity;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = currentAlpha * pulseValue;
            }
        }
    }

    /// <summary>
    /// Lấy thông tin về indicator
    /// </summary>
    public string GetIndicatorInfo()
    {
        if (targetTracker == null)
            return "No target tracker";

        return $"Target: {targetTracker.name}, Position: {targetWorldPosition}, Visible: {isVisible}";
    }

    private void OnDestroy()
    {
        // Dọn dẹp khi destroy
        StopAllCoroutines();
    }
}
