using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBarController : MonoBehaviour
{
    [Header("HP Bar Settings")]
    [SerializeField] private bool showHpBar = true;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float barWidth = 1.5f;
    [SerializeField] private float barHeight = 0.2f;

    [Header("HP Bar Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color healthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    [Header("Animation Settings")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private bool hideWhenFullHealth = false;
    [SerializeField] private bool hideWhenDead = true;

    // Components
    private EnemyStats enemyStats;
    private Canvas worldCanvas;
    private Image backgroundImage;
    public GameObject BackgroundImage => backgroundImage.gameObject;
    private Image healthFillImage;
    private RectTransform healthFillRect;

    // Animation
    private float currentHealthPercentage = 1f;
    private float targetHealthPercentage = 1f;
    private float velocity = 0f;

    // State
    private bool isInitialized;
    private bool isVisible = true;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        if (enemyStats == null)
        {
            Debug.LogError($"[EnemyHpBarController] EnemyStats component not found on {gameObject.name}");
            enabled = false;
        }
    }

    private void Start()
    {
        if (!showHpBar || enemyStats == null) return;
        InitializeHpBar();
        SubscribeToEvents();
    }

    private void OnDestroy() => UnsubscribeFromEvents();

    private void Update()
    {
        if (!isInitialized || !showHpBar) return;
        UpdateHpBarPosition();
        UpdateHpBarAnimation();
        UpdateHpBarVisibility();
    }

    private void InitializeHpBar()
    {
        CreateWorldCanvas();
        CreateHpBarUI();

        // Sử dụng event OnHpChange để đảm bảo HP bar được cập nhật chính xác
        // Điều này đảm bảo rằng HP bar được khởi tạo sau khi EnemyStats đã sẵn sàng
        if (enemyStats != null)
        {
            currentHealthPercentage = enemyStats.GetHealthPercentage();
            targetHealthPercentage = currentHealthPercentage;
        }
        else
        {
            currentHealthPercentage = 1f;
            targetHealthPercentage = 1f;
        }

        ApplyHealthFill(currentHealthPercentage);
        UpdateHealthColor();

        isInitialized = true;

        if (hideWhenFullHealth && currentHealthPercentage >= 1f)
            SetHpBarVisibility(false);
    }

    private void CreateWorldCanvas()
    {
        GameObject canvasObject = new GameObject($"{gameObject.name}_HPBarCanvas");
        canvasObject.transform.SetParent(transform);
        canvasObject.transform.localPosition = offset;

        worldCanvas = canvasObject.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 10;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);
    }

    private void CreateHpBarUI()
    {
        // Background
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(worldCanvas.transform, false);

        backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        RectTransform bgRect = backgroundObject.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Health Fill
        GameObject healthFillObject = new GameObject("HealthFill");
        healthFillObject.transform.SetParent(backgroundObject.transform, false);

        healthFillImage = healthFillObject.AddComponent<Image>();
        healthFillImage.color = healthColor;
        healthFillImage.type = Image.Type.Simple; // Dùng Simple thay vì Filled

        healthFillRect = healthFillObject.GetComponent<RectTransform>();
        healthFillRect.anchorMin = new Vector2(0f, 0f); // Bám trái
        healthFillRect.anchorMax = new Vector2(0f, 1f); // Co theo chiều cao
        healthFillRect.pivot = new Vector2(0f, 0.5f);   // Pivot bên trái
        healthFillRect.offsetMin = Vector2.zero;
        healthFillRect.offsetMax = Vector2.zero;

        ApplyHealthFill(currentHealthPercentage);
    }

    private void SubscribeToEvents()
    {
        enemyStats.OnDestroyed += OnEnemyDestroyed;
        enemyStats.OnHpChange += OnHpChanged;
    }

    private void UnsubscribeFromEvents()
    {
        enemyStats.OnDestroyed -= OnEnemyDestroyed;
        enemyStats.OnHpChange -= OnHpChanged;
    }

    private void OnHpChanged(int currentHealth, int maxHealth)
    {
        if (!isInitialized) return;

        targetHealthPercentage = (float)currentHealth / maxHealth;
        UpdateHealthColor();

        if (hideWhenFullHealth && !isVisible)
            SetHpBarVisibility(true);
    }

    private void OnEnemyDestroyed(GameObject enemy)
    {
        if (hideWhenDead) SetHpBarVisibility(false);
    }

    private void UpdateHpBarPosition()
    {
        if (worldCanvas != null)
            worldCanvas.transform.position = transform.position + offset;
    }

    private void UpdateHpBarAnimation()
    {
        if (healthFillRect == null) return;

        currentHealthPercentage = Mathf.SmoothDamp(currentHealthPercentage, targetHealthPercentage, ref velocity, smoothTime);
        ApplyHealthFill(currentHealthPercentage);
        UpdateHealthColor();
    }

    private void UpdateHealthColor()
    {
        if (healthFillImage == null) return;
        healthFillImage.color = currentHealthPercentage <= lowHealthThreshold ? lowHealthColor : healthColor;
    }

    private void UpdateHpBarVisibility()
    {
        if (worldCanvas == null) return;

        bool shouldBeVisible = isVisible && showHpBar;

        if (hideWhenFullHealth && targetHealthPercentage >= 1f) shouldBeVisible = false;
        if (hideWhenDead && enemyStats.IsDestroyed) shouldBeVisible = false;

        worldCanvas.gameObject.SetActive(shouldBeVisible);
    }

    public void SetHpBarVisibility(bool visible) => isVisible = visible;

    public void SetShowHpBar(bool show)
    {
        showHpBar = show;
        if (worldCanvas != null)
            worldCanvas.gameObject.SetActive(show && isVisible);
    }

    public void SetHpBarOffset(Vector3 newOffset) => offset = newOffset;

    public void SetHpBarSize(float width, float height)
    {
        barWidth = width;
        barHeight = height;

        if (worldCanvas != null)
        {
            RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(barWidth, barHeight);
        }
    }

    public void SetHpBarColors(Color background, Color health, Color lowHealth)
    {
        backgroundColor = background;
        healthColor = health;
        lowHealthColor = lowHealth;

        if (backgroundImage != null) backgroundImage.color = backgroundColor;
    }

    public bool IsHpBarVisible() =>
        showHpBar && isVisible && worldCanvas != null && worldCanvas.gameObject.activeInHierarchy;

    public void UpdateHpBarImmediately()
    {
        if (!isInitialized || enemyStats == null) return;

        targetHealthPercentage = enemyStats.GetHealthPercentage();
        currentHealthPercentage = targetHealthPercentage;

        ApplyHealthFill(currentHealthPercentage);
        UpdateHealthColor();
    }

    private void ApplyHealthFill(float percentage)
    {
        if (healthFillRect == null) return;

        float width = barWidth * percentage;
        healthFillRect.sizeDelta = new Vector2(width, 0f);
    }

    public float GetCurrentHealthPercentage() => currentHealthPercentage;
    public float GetTargetHealthPercentage() => targetHealthPercentage;
}
