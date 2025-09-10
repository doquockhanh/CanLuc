using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Camera Settings UI")]
    [SerializeField] private TMP_Dropdown cameraModeDropdown;
    
    [Header("Buttons")]
    [SerializeField] private Button resetToDefaultsButton;
    [SerializeField] private Button saveSettingsButton;
    [SerializeField] private Button closeSettingsButton;
    
    private GameSettings gameSettings;
    private SettingsManager settingsManager;
    
    private void Awake()
    {
        settingsManager = SettingsManager.Instance;
        gameSettings = settingsManager.Settings;
        
        if (gameSettings == null)
        {
            Debug.LogError("GameSettings not found! Please assign it to SettingsManager.");
            return;
        }
        
        InitializeUI();
        SetupEventListeners();
    }
    
    private void OnEnable()
    {
        LoadCurrentSettings();
    }
    
    private void InitializeUI()
    {
        // Initialize dropdowns
        if (cameraModeDropdown != null)
        {
            cameraModeDropdown.ClearOptions();
            cameraModeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Normal", "Follow Projectiles", "Zoom Out"
            });
        }
    }
    
    private void SetupEventListeners()
    {
        // Camera settings
        if (cameraModeDropdown != null)
            cameraModeDropdown.onValueChanged.AddListener(OnCameraModeChanged);
        
        // Buttons
        if (resetToDefaultsButton != null)
            resetToDefaultsButton.onClick.AddListener(OnResetToDefaultsClicked);
        
        if (saveSettingsButton != null)
            saveSettingsButton.onClick.AddListener(OnSaveSettingsClicked);
        
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
    }
    
    private void LoadCurrentSettings()
    {
        if (gameSettings == null) return;
        
        // Load camera settings
        if (cameraModeDropdown != null)
            cameraModeDropdown.value = (int)gameSettings.CameraMode;
    }
    
    private void UpdateSliderValueText(TextMeshProUGUI textComponent, float value)
    {
        if (textComponent != null)
        {
            textComponent.text = value.ToString("F2");
        }
    }
    
    // Event handlers for UI changes
    private void OnCameraModeChanged(int value)
    {
        if (gameSettings != null)
        {
            gameSettings.CameraMode = (CameraMode)value;
            settingsManager.UpdateCameraMode((CameraMode)value);
        }
    }
    
    private void OnResetToDefaultsClicked()
    {
        if (gameSettings != null)
        {
            gameSettings.ResetToDefaults();
            LoadCurrentSettings();
            Debug.Log("Settings reset to defaults!");
        }
    }
    
    private void OnSaveSettingsClicked()
    {
        settingsManager.SaveSettings();
        settingsManager.SaveSettingsToPlayerPrefs();
        // Apply settings immediately after saving
        settingsManager.ApplyCameraSettingsImmediately();
        Debug.Log("Settings saved and applied!");
    }
    
    private void OnCloseSettingsClicked()
    {
        // Tìm SettingsPanelController và đóng settings
        var panelController = FindFirstObjectByType<SettingsPanelController>();
        if (panelController != null)
        {
            panelController.CloseSettings();
        }
        else
        {
            // Fallback: tự đóng panel
            gameObject.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // Remove event listeners to prevent memory leaks
        if (cameraModeDropdown != null)
            cameraModeDropdown.onValueChanged.RemoveListener(OnCameraModeChanged);
        
        if (resetToDefaultsButton != null)
            resetToDefaultsButton.onClick.RemoveListener(OnResetToDefaultsClicked);
        
        if (saveSettingsButton != null)
            saveSettingsButton.onClick.RemoveListener(OnSaveSettingsClicked);
        
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.RemoveListener(OnCloseSettingsClicked);
    }
}
