using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Settings Button")]
    [SerializeField] private Button settingsButton;
    
    [Header("Close Button (Optional)")]
    [SerializeField] private Button closeButton;
    
    private bool isSettingsOpen = false;
    
    private void Awake()
    {
        // Tự động tìm settings panel nếu chưa được assign
        if (settingsPanel == null)
        {
            settingsPanel = GameObject.Find("SettingsPanel");
        }
        
        // Tự động tìm settings button nếu chưa được assign
        if (settingsButton == null)
        {
            settingsButton = GetComponent<Button>();
        }
        
        // Tự động tìm close button nếu chưa được assign
        if (closeButton == null && settingsPanel != null)
        {
            closeButton = settingsPanel.GetComponentInChildren<Button>();
        }
    }
    
    private void Start()
    {
        // Ẩn settings panel khi bắt đầu
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Setup event listeners
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettings);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
    }
    
    public void ToggleSettings()
    {
        if (settingsPanel == null) return;
        
        isSettingsOpen = !isSettingsOpen;
        settingsPanel.SetActive(isSettingsOpen);
    }
    
    public void OpenSettings()
    {
        if (settingsPanel == null) return;
        
        isSettingsOpen = true;
        settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        if (settingsPanel == null) return;
        
        isSettingsOpen = false;
        settingsPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // Remove event listeners
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(ToggleSettings);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettings);
        }
    }
}
