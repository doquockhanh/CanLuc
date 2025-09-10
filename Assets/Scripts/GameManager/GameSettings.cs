using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Camera Settings")]
    [SerializeField] private CameraMode cameraMode = CameraMode.FollowProjectiles;
    
    // Camera Properties
    public CameraMode CameraMode
    {
        get => cameraMode;
        set => cameraMode = value;
    }    
    // Events for when settings change
    public System.Action<CameraMode> OnCameraModeChanged;
    public System.Action<UIStyle> OnUIStyleChanged;
    public System.Action<float> OnMasterVolumeChanged;
    public System.Action<float> OnGameSpeedChanged;
    
    private void OnValidate()
    {

    }
    
    // Method to reset to default values
    public void ResetToDefaults()
    {
        cameraMode = CameraMode.FollowProjectiles;
    }
}

// Enums for settings
public enum CameraMode
{
    Normal,
    FollowProjectiles,
    ZoomOut
}

public enum UIStyle
{
    Classic,
    Modern,
    Minimal
}
