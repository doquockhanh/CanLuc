using UnityEngine;

public class HotkeyManager : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    
    // Events for hotkey actions
    public System.Action OnStartPhasePressed;
    public System.Action OnCameraPressed;
    
    private void Update()
    {
        if (gameSettings == null) return;
        
        // Check Start Phase hotkey
        if (Input.GetKeyDown(gameSettings.StartPhaseKey))
        {
            OnStartPhasePressed?.Invoke();
        }
        
        // Check Camera hotkey
        if (Input.GetKeyDown(gameSettings.CameraKey))
        {
            OnCameraPressed?.Invoke();
        }
    }
    
    // Method to get current hotkey display text
    public string GetStartPhaseKeyText()
    {
        return gameSettings?.StartPhaseKey.ToString() ?? "B";
    }
    
    public string GetCameraKeyText()
    {
        return gameSettings?.CameraKey.ToString() ?? "C";
    }
    
    // Method to safely set hotkeys with conflict checking
    public bool TrySetStartPhaseKey(KeyCode newKey)
    {
        if (gameSettings == null) return false;
        return gameSettings.SetStartPhaseKey(newKey);
    }
    
    public bool TrySetCameraKey(KeyCode newKey)
    {
        if (gameSettings == null) return false;
        return gameSettings.SetCameraKey(newKey);
    }
    
    // Method to check if key is available
    public bool IsKeyAvailable(KeyCode key)
    {
        return gameSettings != null && !gameSettings.IsKeyInUse(key);
    }
    
    // Method to get conflict info
    public string GetConflictInfo(KeyCode key)
    {
        if (gameSettings == null) return "";
        return gameSettings.GetConflictingKeyName(key);
    }
}
