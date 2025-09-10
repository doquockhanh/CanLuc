using UnityEngine;

public class SettingsInputHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode settingsKey = KeyCode.Escape;
    [SerializeField] private SettingsPanelController settingsPanelController;
    
    private void Awake()
    {
        // Tự động tìm SettingsPanelController nếu chưa được assign
        if (settingsPanelController == null)
        {
            settingsPanelController = FindFirstObjectByType<SettingsPanelController>();
        }
    }
    
    private void Update()
    {
        // Kiểm tra phím tắt để mở/đóng settings
        if (Input.GetKeyDown(settingsKey))
        {
            if (settingsPanelController != null)
            {
                settingsPanelController.ToggleSettings();
            }
        }
    }
}
