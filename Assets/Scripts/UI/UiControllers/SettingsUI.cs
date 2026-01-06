using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Camera Settings UI")]
    [SerializeField] private TMP_Dropdown cameraModeDropdown;
    
    [Header("Hotkey Settings UI")]
    [SerializeField] private Button startPhaseKeyButton;
    [SerializeField] private Button cameraKeyButton;
    [SerializeField] private TextMeshProUGUI startPhaseKeyText;
    [SerializeField] private TextMeshProUGUI cameraKeyText;
    [SerializeField] private TextMeshProUGUI conflictWarningText;
    
    [Header("Buttons")]
    [SerializeField] private Button resetToDefaultsButton;
    [SerializeField] private Button saveSettingsButton;
    [SerializeField] private Button closeSettingsButton;
    
    private GameSettings gameSettings;
    private SettingsManager settingsManager;
    private HotkeyManager hotkeyManager;
    
    private bool isWaitingForKeyInput = false;
    private string currentKeyBeingSet = "";
    
    private void Awake()
    {
        settingsManager = SettingsManager.Instance;
        gameSettings = settingsManager.Settings;
        hotkeyManager = FindFirstObjectByType<HotkeyManager>();
        
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
                "Normal", "Zoom Out"
            });
        }
    }
    
    private void SetupEventListeners()
    {
        // Camera settings
        if (cameraModeDropdown != null)
            cameraModeDropdown.onValueChanged.AddListener(OnCameraModeChanged);
        
        // Hotkey settings
        if (startPhaseKeyButton != null)
            startPhaseKeyButton.onClick.AddListener(() => StartKeyInput("StartPhase"));
        
        if (cameraKeyButton != null)
            cameraKeyButton.onClick.AddListener(() => StartKeyInput("Camera"));
        
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
        
        // Load hotkey settings
        UpdateHotkeyDisplay();
    }
    
    private void Update()
    {
        if (isWaitingForKeyInput)
        {
            HandleKeyInput();
        }
    }
    
    private void UpdateHotkeyDisplay()
    {
        if (hotkeyManager == null) return;
        
        if (startPhaseKeyText != null)
            startPhaseKeyText.text = hotkeyManager.GetStartPhaseKeyText();
        
        if (cameraKeyText != null)
            cameraKeyText.text = hotkeyManager.GetCameraKeyText();
    }
    
    private void StartKeyInput(string keyType)
    {
        isWaitingForKeyInput = true;
        currentKeyBeingSet = keyType;
        
        if (conflictWarningText != null)
            conflictWarningText.text = $"Nhấn phím mới cho {keyType}...";
    }
    
    private void HandleKeyInput()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    SetNewKey(keyCode);
                    break;
                }
            }
        }
    }
    
    private void SetNewKey(KeyCode newKey)
    {
        isWaitingForKeyInput = false;
        bool success = false;
        
        if (currentKeyBeingSet == "StartPhase")
        {
            success = hotkeyManager.TrySetStartPhaseKey(newKey);
        }
        else if (currentKeyBeingSet == "Camera")
        {
            success = hotkeyManager.TrySetCameraKey(newKey);
        }
        
        if (success)
        {
            UpdateHotkeyDisplay();
            if (conflictWarningText != null)
                conflictWarningText.text = "";
        }
        else
        {
            string conflictInfo = hotkeyManager.GetConflictInfo(newKey);
            if (conflictWarningText != null)
                conflictWarningText.text = $"Phím {newKey} đã được dùng cho {conflictInfo}";
        }
        
        currentKeyBeingSet = "";
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
            if (conflictWarningText != null)
                conflictWarningText.text = "";
            Debug.Log("Settings reset to defaults!");
        }
    }
    
    private void OnSaveSettingsClicked()
    {
        settingsManager.SaveSettings();
        settingsManager.SaveSettingsToPlayerPrefs();
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
