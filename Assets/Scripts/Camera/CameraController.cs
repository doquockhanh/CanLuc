using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý điều khiển camera độc lập, tách biệt khỏi FocusManager
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera worldCamera;

    [Header("Free Movement")]
    [SerializeField] private float freeMoveSpeed = 10f;
    [SerializeField] private bool cameraMovementLocked = false;

    [Header("Follow System")]
    [SerializeField] private bool followActive = false;
    [SerializeField] private bool freeCameraMode = false; // true: WASD control; false: follow target

    [Header("Follow Help UI")]
    [SerializeField] private GameObject followHelpPanel;
    [SerializeField] private TMPro.TextMeshProUGUI followHelpText;

    // Camera cycle state
    private List<GameObject> cameraCycleTargets;
    private int cameraCycleIndex = -1;

    // Events
    public System.Action OnFollowModeStarted;
    public System.Action OnFollowModeEnded;
    public System.Action<GameObject> OnTargetChanged;

    public static CameraController Instance { get; private set; }

    // Properties
    public bool IsFollowActive => followActive;
    public bool IsFreeCameraMode => freeCameraMode;
    public GameObject CurrentTarget => GetCurrentCameraTarget();
    public int CurrentTargetIndex => cameraCycleIndex;
    public int TotalTargets => cameraCycleTargets?.Count ?? 0;

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
    }

    void Update()
    {
        if (followActive)
        {
            HandleFollowModeInputs();
            UpdateFollowHelpPanel();
        }
    }

    void LateUpdate()
    {
        // WASD luôn hoạt động khi không follow
        if (!followActive)
        {
            UpdateCameraFreeMove();
            return;
        }

        // Khi đang follow: nếu free mode -> WASD, ngược lại -> follow target
        if (freeCameraMode)
        {
            UpdateCameraFreeMove();
        }
        else
        {
            UpdateCameraFollow();
        }
    }

    #region Follow Mode Input Handling

    private void HandleFollowModeInputs()
    {
        // Space: toggle giữa follow target và free camera (WASD)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleFreeCameraMode();
            return;
        }

        // ESC: thoát chế độ follow/cycle hoàn toàn
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitFollowMode();
            return;
        }

        // Left click: chuyển mục tiêu tiếp theo
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            AdvanceCameraCycle();
        }
    }

    #endregion

    #region Follow Mode Management

    /// <summary>
    /// Bắt đầu camera cycle với danh sách targets
    /// </summary>
    public void StartCameraCycle(List<GameObject> targets)
    {
        if (targets == null) return;

        // Lọc null
        targets.RemoveAll(t => t == null);
        if (targets.Count == 0)
        {
            ExitFollowMode();
            return;
        }

        cameraCycleTargets = targets;
        cameraCycleIndex = 0;
        followActive = true;
        freeCameraMode = false; // bắt đầu ở chế độ follow

        OnFollowModeStarted?.Invoke();
        JumpCameraToCurrentTarget();
    }

    /// <summary>
    /// Thoát khỏi chế độ follow
    /// </summary>
    public void ExitFollowMode()
    {
        followActive = false;
        freeCameraMode = false;
        cameraCycleTargets = null;
        cameraCycleIndex = -1;

        OnFollowModeEnded?.Invoke();
    }

    /// <summary>
    /// Toggle giữa free camera và follow mode
    /// </summary>
    public void ToggleFreeCameraMode()
    {
        freeCameraMode = !freeCameraMode;
    }

    /// <summary>
    /// Chuyển sang target tiếp theo trong cycle
    /// </summary>
    public void AdvanceCameraCycle()
    {
        if (!followActive || cameraCycleTargets == null || cameraCycleTargets.Count == 0)
            return;

        cameraCycleIndex = (cameraCycleIndex + 1) % cameraCycleTargets.Count;
        JumpCameraToCurrentTarget();

        OnTargetChanged?.Invoke(GetCurrentCameraTarget());
    }

    #endregion

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

    private void UpdateCameraFollow()
    {
        if (!followActive || worldCamera == null) return;

        var target = GetCurrentCameraTarget();
        if (target == null)
        {
            ExitFollowMode();
            return;
        }

        Vector3 pos = target.transform.position;
        pos.z = worldCamera.transform.position.z;
        worldCamera.transform.position = pos;
    }

    private void JumpCameraToCurrentTarget()
    {
        if (worldCamera == null) return;

        var target = GetCurrentCameraTarget();
        if (target == null)
        {
            ExitFollowMode();
            return;
        }

        Vector3 pos = target.transform.position;
        pos.z = worldCamera.transform.position.z;
        worldCamera.transform.position = pos;
    }

    #endregion

    #region Target Management

    private GameObject GetCurrentCameraTarget()
    {
        if (!followActive || cameraCycleTargets == null || cameraCycleTargets.Count == 0)
            return null;

        if (cameraCycleIndex < 0 || cameraCycleIndex >= cameraCycleTargets.Count)
            return null;

        // Bỏ qua mục tiêu null và tự động tiến tới mục tiêu hợp lệ tiếp theo
        int safeguard = 0;
        while (safeguard < cameraCycleTargets.Count)
        {
            var t = cameraCycleTargets[cameraCycleIndex];
            if (t != null) return t;
            cameraCycleIndex = (cameraCycleIndex + 1) % cameraCycleTargets.Count;
            safeguard++;
        }
        return null;
    }

    #endregion

    #region Camera Lock Management

    /// <summary>
    /// Khóa di chuyển camera (WASD controls)
    /// </summary>
    public void LockCameraMovement()
    {
        cameraMovementLocked = true;
    }

    /// <summary>
    /// Mở khóa di chuyển camera (WASD controls)
    /// </summary>
    public void UnlockCameraMovement()
    {
        cameraMovementLocked = false;
    }

    #endregion

    #region UI Updates

    private void UpdateFollowHelpPanel()
    {
        if (followHelpPanel == null) return;

        if (followHelpPanel.activeSelf != followActive)
        {
            followHelpPanel.SetActive(followActive);
        }

        if (!followActive) return;

        if (followHelpText != null)
        {
            string status;
            if (freeCameraMode)
            {
                status = "Free Camera (WASD)";
            }
            else
            {
                var target = GetCurrentCameraTarget();
                int total = cameraCycleTargets != null ? cameraCycleTargets.Count : 0;
                int index = cameraCycleIndex >= 0 ? (cameraCycleIndex + 1) : 0;
                string targetName = target != null ? target.name : "<none>";
                status = $"Follow: {targetName} ({index}/{total})";
            }
            followHelpText.text = status;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Thiết lập tốc độ di chuyển camera
    /// </summary>
    public void SetFreeMoveSpeed(float speed)
    {
        freeMoveSpeed = speed;
    }

    /// <summary>
    /// Thiết lập camera reference
    /// </summary>
    public void SetCamera(Camera camera)
    {
        worldCamera = camera;
    }

    /// <summary>
    /// Thiết lập follow help UI
    /// </summary>
    public void SetFollowHelpUI(GameObject panel, TMPro.TextMeshProUGUI text)
    {
        followHelpPanel = panel;
        followHelpText = text;
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion
}
