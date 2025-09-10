using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings Asset")]
    [SerializeField] private GameSettings gameSettings;
    
    private static SettingsManager _instance;
    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SettingsManager>();
                
                if (_instance == null)
                {
                    GameObject settingsManagerObject = new GameObject("SettingsManager");
                    _instance = settingsManagerObject.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(settingsManagerObject);
                }
            }
            return _instance;
        }
    }
    
    public GameSettings Settings
    {
        get
        {
            if (gameSettings == null)
            {
                Debug.LogError("GameSettings asset is not assigned in SettingsManager!");
                return null;
            }
            return gameSettings;
        }
    }
    
    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettings();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSettings()
    {
        if (gameSettings == null)
        {
            Debug.LogWarning("GameSettings asset is not assigned. Please assign it in the inspector.");
            return;
        }
        
        // Apply initial settings
        ApplyCameraSettings();
    }
    
    private void ApplyCameraSettings()
    {
        // Apply camera settings to any camera controllers in the scene
        var cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            cameraController.ApplySettings(gameSettings);
        }
    }
    
    
    // Public methods to update settings
    public void UpdateCameraMode(CameraMode newMode)
    {
        if (gameSettings != null)
        {
            gameSettings.CameraMode = newMode;
            gameSettings.OnCameraModeChanged?.Invoke(newMode);
            ApplyCameraSettingsImmediately();
        }
    }
    
    /// <summary>
    /// Apply camera settings immediately (for real-time settings changes)
    /// </summary>
    public void ApplyCameraSettingsImmediately()
    {
        var cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            cameraController.ApplySettingsImmediately();
        }
    }
    
    // Method to save settings (if you want to persist them)
    public void SaveSettings()
    {
        if (gameSettings != null)
        {
            // Unity automatically saves ScriptableObject changes in editor
            // For runtime persistence, you might want to use PlayerPrefs or JSON
            Debug.Log("Settings saved!");
        }
    }
    
    // Method to load settings from PlayerPrefs (optional)
    public void LoadSettingsFromPlayerPrefs()
    {
        if (gameSettings == null) return;
        
        // Load settings from PlayerPrefs if they exist
        if (PlayerPrefs.HasKey("CameraMode"))
        {
            gameSettings.CameraMode = (CameraMode)PlayerPrefs.GetInt("CameraMode");
        }
        
        ApplyCameraSettings();
    }
    
    // Method to save settings to PlayerPrefs (optional)
    public void SaveSettingsToPlayerPrefs()
    {
        if (gameSettings == null) return;
        
        PlayerPrefs.SetInt("CameraMode", (int)gameSettings.CameraMode);
        
        PlayerPrefs.Save();
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}

// Interface for camera controllers to implement
public interface ICameraController
{
    void ApplySettings(GameSettings settings);
}
