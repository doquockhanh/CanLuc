using UnityEngine;

namespace Gameplay.Focus
{
    /// <summary>
    /// Script theo dõi vị trí của enemy để hiển thị indicator khi nằm ngoài camera
    /// Gắn script này vào GameObject enemy để kích hoạt tính năng off-screen indicator
    /// </summary>
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
        
        /// <summary>
        /// Bật/tắt tracking cho object này
        /// </summary>
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
        
        /// <summary>
        /// Cập nhật trạng thái hiển thị của object
        /// </summary>
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
        
        /// <summary>
        /// Thiết lập màu sắc tùy chỉnh cho indicator
        /// </summary>
        public void SetCustomIndicatorColor(Color color)
        {
            customIndicatorColor = color;
        }
        
        /// <summary>
        /// Thiết lập kích thước tùy chỉnh cho indicator
        /// </summary>
        public void SetCustomIndicatorScale(float scale)
        {
            customIndicatorScale = Mathf.Clamp(scale, 0.1f, 5f);
        }
        
        /// <summary>
        /// Lấy màu sắc tùy chỉnh cho indicator
        /// </summary>
        public Color GetCustomIndicatorColor()
        {
            return customIndicatorColor;
        }
        
        /// <summary>
        /// Lấy kích thước tùy chỉnh cho indicator
        /// </summary>
        public float GetCustomIndicatorScale()
        {
            return customIndicatorScale;
        }
        
        /// <summary>
        /// Kiểm tra xem object có nằm ngoài camera không
        /// </summary>
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
        
        /// <summary>
        /// Lấy khoảng cách từ object đến camera
        /// </summary>
        public float GetDistanceToCamera()
        {
            if (Camera.main == null) return float.MaxValue;
            
            return Vector3.Distance(transform.position, Camera.main.transform.position);
        }
    }
}
