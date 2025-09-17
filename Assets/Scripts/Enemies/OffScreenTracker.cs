using UnityEngine;


public class OffScreenTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    [SerializeField] private bool enableTracking = true;
    [SerializeField] private bool showDebugInfo = false;

    [Header("Indicator Customization")]
    [SerializeField] private Color customIndicatorColor = Color.red;
    [SerializeField] private float customIndicatorScale = 1f;

    private OffScreenIndicatorManager indicatorManager;
    private bool isRegistered = false;

    // Events
    public System.Action<bool> OnVisibilityChanged;

    public bool IsVisibleOnScreen { get; private set; }
    public bool IsBeingTracked => isRegistered;

    private void Start()
    {
        if (!enableTracking) return;

        // Tìm OffScreenIndicatorManager
        indicatorManager = OffScreenIndicatorManager.Instance;
        if (indicatorManager == null)
        {
            Debug.LogWarning($"OffScreenTracker on {gameObject.name}: Không thể tìm thấy OffScreenIndicatorManager!");
            return;
        }

        // Đăng ký với manager
        RegisterWithManager();
    }

    private void OnEnable()
    {
        if (enableTracking && indicatorManager != null && !isRegistered)
        {
            RegisterWithManager();
        }
    }

    private void OnDisable()
    {
        if (isRegistered)
        {
            UnregisterFromManager();
        }
    }

    private void OnDestroy()
    {
        if (isRegistered)
        {
            UnregisterFromManager();
        }
    }

    private void RegisterWithManager()
    {
        if (indicatorManager != null && !isRegistered)
        {
            indicatorManager.RegisterTracker(this);
            isRegistered = true;

            if (showDebugInfo)
            {
                Debug.Log($"OffScreenTracker: {gameObject.name} đã được đăng ký với OffScreenIndicatorManager");
            }
        }
    }

    private void UnregisterFromManager()
    {
        if (indicatorManager != null && isRegistered)
        {
            indicatorManager.UnregisterTracker(this);
            isRegistered = false;

            if (showDebugInfo)
            {
                Debug.Log($"OffScreenTracker: {gameObject.name} đã được hủy đăng ký khỏi OffScreenIndicatorManager");
            }
        }
    }

    public void SetTrackingEnabled(bool enabled)
    {
        if (enableTracking == enabled) return;

        enableTracking = enabled;

        if (enabled)
        {
            if (indicatorManager != null && !isRegistered)
            {
                RegisterWithManager();
            }
        }
        else
        {
            if (isRegistered)
            {
                UnregisterFromManager();
            }
        }
    }


    public void UpdateVisibility(bool isVisible)
    {
        if (IsVisibleOnScreen != isVisible)
        {
            IsVisibleOnScreen = isVisible;
            OnVisibilityChanged?.Invoke(isVisible);

            if (showDebugInfo)
            {
                Debug.Log($"OffScreenTracker: {gameObject.name} - Visibility changed to: {isVisible}");
            }
        }
    }

    public void SetCustomIndicatorColor(Color color)
    {
        customIndicatorColor = color;
    }

    public void SetCustomIndicatorScale(float scale)
    {
        customIndicatorScale = Mathf.Clamp(scale, 0.1f, 5f);
    }

    public Color GetCustomIndicatorColor()
    {
        return customIndicatorColor;
    }

    public float GetCustomIndicatorScale()
    {
        return customIndicatorScale;
    }

    public bool IsOffScreen()
    {
        if (Camera.main == null) return false;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        return screenPoint.x < 0 ||
               screenPoint.x > Screen.width ||
               screenPoint.y < 0 ||
               screenPoint.y > Screen.height ||
               screenPoint.z < 0;
    }

    public float GetDistanceToCamera()
    {
        if (Camera.main == null) return float.MaxValue;

        return Vector3.Distance(transform.position, Camera.main.transform.position);
    }
}

