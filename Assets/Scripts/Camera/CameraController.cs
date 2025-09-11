using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraController : MonoBehaviour, ICameraController, IGamePhaseAware
{
    [Header("Camera References")]
    [SerializeField] private Camera worldCamera;

    [Header("Free Movement")]
    [SerializeField] private float freeMoveSpeed = 10f;
    [SerializeField] private bool cameraMovementLocked = false;

    [Header("Camera Settings")]
    [SerializeField] private int normalAssetsPPU = 100;
    [SerializeField] private int zoomOutAssetsPPU = 60; // Giá trị nhỏ hơn = zoom out nhiều hơn
    private int originalAssetsPPU;
    private PixelPerfectCamera pixelPerfectCamera;
    private CameraMode currentCameraMode = CameraMode.Normal;

    public static CameraController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        // Lấy PixelPerfectCamera và lưu PPU gốc
        if (worldCamera != null)
        {
            pixelPerfectCamera = worldCamera.GetComponent<PixelPerfectCamera>();
            if (pixelPerfectCamera != null)
            {
                originalAssetsPPU = pixelPerfectCamera.assetsPPU;
            }
        }
    }

    void Start()
    {
        // Đăng ký lắng nghe GamePhase
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterGamePhaseAwareComponent(this);
        }
    }

    void LateUpdate() { UpdateCameraFreeMove(); }

    #region Camera Movement

    private void UpdateCameraFreeMove()
    {
        if (worldCamera == null || cameraMovementLocked) return;

        Vector3 delta = Vector3.zero;
        if (Input.GetKey(KeyCode.A)) delta.x -= 1f;
        if (Input.GetKey(KeyCode.D)) delta.x += 1f;
        if (Input.GetKey(KeyCode.S)) delta.y -= 1f;
        if (Input.GetKey(KeyCode.W)) delta.y += 1f;

        delta = delta.normalized * freeMoveSpeed * Time.deltaTime;
        Vector3 pos = worldCamera.transform.position + delta;
        pos.z = worldCamera.transform.position.z;
        worldCamera.transform.position = pos;
    }


    #endregion


    #region Camera Lock Management
    public void LockCameraMovement()
    {
        cameraMovementLocked = true;
    }
    public void UnlockCameraMovement()
    {
        cameraMovementLocked = false;
    }

    #endregion


    #region ICameraController Implementation

    /// <summary>
    /// Apply camera settings from GameSettings
    /// </summary>
    public void ApplySettings(GameSettings settings)
    {
        if (settings == null || worldCamera == null) return;

        switch (settings.CameraMode)
        {
            case CameraMode.Normal:
                ApplyNormalCameraMode();
                break;
            case CameraMode.ZoomOut:
                ApplyZoomOutMode();
                break;
        }
    }

    private void ApplyNormalCameraMode()
    {
        // Normal mode: camera stays at normal size, no special behavior
        currentCameraMode = CameraMode.Normal;
        if (pixelPerfectCamera != null)
        {
            StartCoroutine(SmoothZoom(originalAssetsPPU));
        }
    }

    // removed follow projectiles mode

    private void ApplyZoomOutMode()
    {
        // Zoom Out mode: increase camera size for better overview
        currentCameraMode = CameraMode.ZoomOut;
        if (pixelPerfectCamera != null)
        {
            StartCoroutine(SmoothZoom(zoomOutAssetsPPU));
        }
    }

    private IEnumerator SmoothZoom(int zoomLevel)
    {
        int currentZoom = pixelPerfectCamera.assetsPPU;
        while (currentZoom != zoomLevel)
        {
            if (currentZoom < zoomLevel)
            {
                currentZoom++;
            }
            else
            {
                currentZoom--;
            }
            pixelPerfectCamera.assetsPPU = currentZoom;
            yield return new WaitForSeconds(0.02f);
        }
    }

    private void ApplyCurrentCameraSettings()
    {
        var settingsManager = SettingsManager.Instance;
        if (settingsManager?.Settings != null)
        {
            ApplySettings(settingsManager.Settings);
        }
    }

    #endregion

    #region IGamePhaseAware Implementation

    public void OnPreparePhaseStarted()
    {
        ApplyCurrentCameraSettings();
    }

    public void OnBattlePhaseStarted()
    {
        ApplyCurrentCameraSettings();
    }

    public void OnPhaseChanged(GamePhase newPhase)
    {
        // Không cần xử lý thêm ở đây
    }

    #endregion
}
