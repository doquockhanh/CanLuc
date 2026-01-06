using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Camera Settings")]
    [SerializeField] private CameraMode cameraMode = CameraMode.Normal;
    
    [Header("Hotkey Settings")]
    [SerializeField] private KeyCode startPhaseKey = KeyCode.B;
    [SerializeField] private KeyCode cameraKey = KeyCode.C;
    
    // Camera Properties
    public CameraMode CameraMode
    {
        get => cameraMode;
        set => cameraMode = value;
    }
    
    // Hotkey Properties
    public KeyCode StartPhaseKey
    {
        get => startPhaseKey;
        set => startPhaseKey = value;
    }
    
    public KeyCode CameraKey
    {
        get => cameraKey;
        set => cameraKey = value;
    }
    
    // Events for when settings change
    public System.Action<CameraMode> OnCameraModeChanged;
    public System.Action<UIStyle> OnUIStyleChanged;
    public System.Action<float> OnMasterVolumeChanged;
    public System.Action<float> OnGameSpeedChanged;
    public System.Action<KeyCode> OnStartPhaseKeyChanged;
    public System.Action<KeyCode> OnCameraKeyChanged;
    
    private void OnValidate()
    {

    }
    
    // Method to reset to default values
    public void ResetToDefaults()
    {
        cameraMode = CameraMode.Normal;
        startPhaseKey = KeyCode.B;
        cameraKey = KeyCode.C;
    }
    
    // Method to set hotkey and trigger event
    public bool SetStartPhaseKey(KeyCode newKey)
    {
        if (newKey == cameraKey)
        {
            Debug.LogWarning($"Phím {newKey} đã được sử dụng cho Camera. Vui lòng chọn phím khác.");
            return false;
        }
        
        startPhaseKey = newKey;
        OnStartPhaseKeyChanged?.Invoke(newKey);
        return true;
    }
    
    public bool SetCameraKey(KeyCode newKey)
    {
        if (newKey == startPhaseKey)
        {
            Debug.LogWarning($"Phím {newKey} đã được sử dụng cho Start Phase. Vui lòng chọn phím khác.");
            return false;
        }
        
        cameraKey = newKey;
        OnCameraKeyChanged?.Invoke(newKey);
        return true;
    }
    
    // Method to check if key is already used
    public bool IsKeyInUse(KeyCode key)
    {
        return key == startPhaseKey || key == cameraKey;
    }
    
    // Method to get conflicting key name
    public string GetConflictingKeyName(KeyCode key)
    {
        if (key == startPhaseKey) return "Start Phase";
        if (key == cameraKey) return "Camera";
        return "";
    }
}

// Enums for settings
public enum CameraMode
{
    Normal,
    ZoomOut
}

public enum UIStyle
{
    Classic,
    Modern,
    Minimal
}
