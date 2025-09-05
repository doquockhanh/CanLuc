using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Focus
{
    public class OffScreenIndicatorManager : MonoBehaviour
    {
        [Header("Indicator Settings")]
        [SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private float screenBorderOffset = 50f;
        [SerializeField] private float indicatorScale = 1f;
        [SerializeField] private Color indicatorColor = Color.red;
        
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas targetCanvas;
        
        private List<OffScreenTracker> trackedObjects = new List<OffScreenTracker>();
        private Dictionary<OffScreenTracker, GameObject> activeIndicators = new Dictionary<OffScreenTracker, GameObject>();
        
        private RectTransform canvasRectTransform;
        private Vector2 screenCenter;
        private Vector2 screenBounds;
        
        public static OffScreenIndicatorManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeManager();
        }
        
        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            if (targetCanvas == null)
                targetCanvas = FindFirstObjectByType<Canvas>();
                
            if (targetCanvas != null)
                canvasRectTransform = targetCanvas.GetComponent<RectTransform>();
        }
        
        private void InitializeManager()
        {
            if (indicatorPrefab == null)
            {
                // Tìm indicator prefab trong Resources
                indicatorPrefab = Resources.Load<GameObject>("Prefabs/Indicator_0");
                if (indicatorPrefab == null)
                {
                    Debug.LogError("OffScreenIndicatorManager: Không thể tìm thấy indicator prefab!");
                }
            }
        }
        
        private void Update()
        {
            UpdateScreenBounds();
            UpdateAllIndicators();
        }
        
        private void UpdateScreenBounds()
        {
            if (mainCamera == null) return;
            
            screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            screenBounds = new Vector2(Screen.width * 0.5f - screenBorderOffset, Screen.height * 0.5f - screenBorderOffset);
        }
        
        public void RegisterTracker(OffScreenTracker tracker)
        {
            if (!trackedObjects.Contains(tracker))
            {
                trackedObjects.Add(tracker);
                CreateIndicatorForTracker(tracker);
            }
        }
        
        public void UnregisterTracker(OffScreenTracker tracker)
        {
            if (trackedObjects.Contains(tracker))
            {
                trackedObjects.Remove(tracker);
                RemoveIndicatorForTracker(tracker);
            }
        }
        
        private void CreateIndicatorForTracker(OffScreenTracker tracker)
        {
            if (indicatorPrefab == null || targetCanvas == null) return;
            
            GameObject indicator = Instantiate(indicatorPrefab, targetCanvas.transform);
            indicator.name = $"Indicator_{tracker.name}";
            
            // Thiết lập OffScreenIndicator component
            var offScreenIndicator = indicator.GetComponent<OffScreenIndicator>();
            if (offScreenIndicator == null)
            {
                offScreenIndicator = indicator.AddComponent<OffScreenIndicator>();
            }
            
            // Thiết lập target tracker
            offScreenIndicator.SetTargetTracker(tracker);
            
            // Thiết lập màu sắc và kích thước mặc định
            offScreenIndicator.SetIndicatorColor(indicatorColor);
            offScreenIndicator.SetIndicatorScale(indicatorScale);
            
            activeIndicators[tracker] = indicator;
        }
        
        private void RemoveIndicatorForTracker(OffScreenTracker tracker)
        {
            if (activeIndicators.TryGetValue(tracker, out GameObject indicator))
            {
                if (indicator != null)
                    Destroy(indicator);
                activeIndicators.Remove(tracker);
            }
        }
        
        private void UpdateAllIndicators()
        {
            foreach (var kvp in activeIndicators)
            {
                OffScreenTracker tracker = kvp.Key;
                GameObject indicator = kvp.Value;
                
                if (tracker == null || indicator == null)
                {
                    continue;
                }
                
                UpdateIndicatorPosition(tracker, indicator);
            }
        }
        
        private void UpdateIndicatorPosition(OffScreenTracker tracker, GameObject indicator)
        {
            if (mainCamera == null) return;
            
            Vector3 targetPosition = tracker.transform.position;
            Vector2 screenPoint = mainCamera.WorldToScreenPoint(targetPosition);
            
            // Kiểm tra xem object có nằm ngoài màn hình không
            bool isOffScreen = IsOffScreen(screenPoint);
            
            if (isOffScreen)
            {
                // Hiển thị indicator
                var offScreenIndicator = indicator.GetComponent<OffScreenIndicator>();
                if (offScreenIndicator != null)
                {
                    offScreenIndicator.Show();
                    
                    // Tính toán vị trí indicator trên viền màn hình
                    Vector2 indicatorPosition = CalculateIndicatorPosition(screenPoint);
                    
                    // Chuyển đổi từ screen space sang canvas space
                    Vector2 canvasPosition = ScreenToCanvasPosition(indicatorPosition);
                    
                    // Thiết lập vị trí và rotation
                    offScreenIndicator.SetPosition(canvasPosition);
                    
                    Vector2 direction = (screenPoint - screenCenter).normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    offScreenIndicator.SetRotation(angle);
                }
            }
            else
            {
                // Ẩn indicator nếu object đã vào màn hình
                var offScreenIndicator = indicator.GetComponent<OffScreenIndicator>();
                if (offScreenIndicator != null)
                {
                    offScreenIndicator.Hide();
                }
            }
        }
        
        private bool IsOffScreen(Vector3 screenPoint)
        {
            return screenPoint.x < screenBorderOffset || 
                   screenPoint.x > Screen.width - screenBorderOffset ||
                   screenPoint.y < screenBorderOffset || 
                   screenPoint.y > Screen.height - screenBorderOffset ||
                   screenPoint.z < 0; // Object ở phía sau camera
        }
        
        private Vector2 CalculateIndicatorPosition(Vector2 screenPoint)
        {
            Vector2 direction = (screenPoint - screenCenter).normalized;
            
            // Tính toán vị trí trên viền màn hình
            float slope = direction.y / direction.x;
            
            Vector2 indicatorPosition;
            
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Indicator nằm trên cạnh trái hoặc phải
                float x = direction.x > 0 ? screenBounds.x : -screenBounds.x;
                float y = slope * x;
                
                if (Mathf.Abs(y) > screenBounds.y)
                {
                    // Indicator nằm trên cạnh trên hoặc dưới
                    y = direction.y > 0 ? screenBounds.y : -screenBounds.y;
                    x = y / slope;
                }
                
                indicatorPosition = new Vector2(x, y);
            }
            else
            {
                // Indicator nằm trên cạnh trên hoặc dưới
                float y = direction.y > 0 ? screenBounds.y : -screenBounds.y;
                float x = y / slope;
                
                if (Mathf.Abs(x) > screenBounds.x)
                {
                    // Indicator nằm trên cạnh trái hoặc phải
                    x = direction.x > 0 ? screenBounds.x : -screenBounds.x;
                    y = slope * x;
                }
                
                indicatorPosition = new Vector2(x, y);
            }
            
            return indicatorPosition + screenCenter;
        }
        
        private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
        {
            if (canvasRectTransform == null) return screenPosition;
            
            Vector2 viewportPosition = new Vector2(
                screenPosition.x / Screen.width,
                screenPosition.y / Screen.height
            );
            
            Vector2 canvasPosition = new Vector2(
                (viewportPosition.x - 0.5f) * canvasRectTransform.sizeDelta.x,
                (viewportPosition.y - 0.5f) * canvasRectTransform.sizeDelta.y
            );
            
            return canvasPosition;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
