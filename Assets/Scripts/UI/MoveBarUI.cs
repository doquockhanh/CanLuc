using UnityEngine;
using UnityEngine.UI;

public class MoveBarUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform barContainer;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject barTemplatePrefab;
    [SerializeField] private string defaultTemplateResourcePath = "Prefabs/MoveBarTemplate";
    
    private Image barFill;
    private GameObject barInstance;
    private IMoveAction observedMoveAction;
    private ActionBase currentFocusable;
    
    void Awake()
    {
        // Tự tìm container nếu không gán
        if (barContainer == null)
        {
            barContainer = GetComponent<RectTransform>();
        }
        // Tự load template nếu không gán
        if (barTemplatePrefab == null && !string.IsNullOrEmpty(defaultTemplateResourcePath))
        {
            barTemplatePrefab = Resources.Load<GameObject>(defaultTemplateResourcePath);
        }
    }
    
    void OnEnable()
    {
        ActionBase.OnFocusChanged += HandleFocusChanged;
        // Initialize theo focus hiện tại nếu có
        if (ActionBase.Current != null)
        {
            HandleFocusChanged(null, ActionBase.Current);
        }
        else
        {
            BindMoveAction(null);
        }
    }
    
    void OnDisable()
    {
        ActionBase.OnFocusChanged -= HandleFocusChanged;
        BindMoveAction(null);
    }
    
    private void HandleFocusChanged(ActionBase previous, ActionBase current)
    {
        currentFocusable = current;
        // Tìm bất kỳ Action nào có khả năng di chuyển bằng power
        IMoveAction moveAction = null;
        if (current != null)
        {
            moveAction = current.GetComponent<IMoveAction>();
        }
        BindMoveAction(moveAction);
    }
    
    private void BindMoveAction(IMoveAction moveAction)
    {
        observedMoveAction = moveAction;
        RebuildBar();
        UpdateVisibility(observedMoveAction != null);
    }
    
    private void UpdateVisibility(bool visible)
    {
        if (barInstance != null)
        {
            barInstance.SetActive(visible);
        }
    }
    
    private void RebuildBar()
    {
        // Chỉ xóa thanh của riêng mình, không xóa tất cả children
        if (barInstance != null)
        {
            barInstance.transform.SetParent(null);
            Destroy(barInstance);
            barInstance = null;
            barFill = null;
        }
        
        if (observedMoveAction == null || barTemplatePrefab == null || barContainer == null)
        {
            return;
        }
        
        // Tạo thanh sức mạnh
        barInstance = Instantiate(barTemplatePrefab, barContainer);
        barInstance.name = "MovePowerBar";
        barFill = FindFillImage(barInstance);
        
        if (barFill == null)
        {
            Debug.LogError("Không tìm thấy Fill Image trong MoveBar template!");
        }
    }
    
    private Image FindFillImage(GameObject bar)
    {
        // Tìm Image có kiểu Filled (thường là phần fill của thanh)
        var images = bar.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].type == Image.Type.Filled)
                return images[i];
        }
        // fallback: lấy Image con đầu tiên
        return images != null && images.Length > 0 ? images[0] : null;
    }
    
    void Update()
    {
        if (observedMoveAction == null || barFill == null)
        {
            return;
        }
        
        // Cập nhật thanh sức mạnh
        float currentPower = observedMoveAction.GetCurrentPower();
        float maxPower = observedMoveAction.GetMaxPower();
        float fillAmount = Mathf.Clamp01(currentPower / maxPower);
        barFill.fillAmount = fillAmount;
    }
    
}
