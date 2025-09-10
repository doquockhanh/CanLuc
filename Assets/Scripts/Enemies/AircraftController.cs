using UnityEngine;

public class AircraftController : MonoBehaviour, IGamePhaseAware
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector3 moveDirection = Vector3.right; // Di chuyển về phía phải (trục X)
    [SerializeField] private bool normalizeDirection = true;

    [Header("Phase Control")]
    [SerializeField] private bool stopInPreparePhase = true;
    [SerializeField] private bool moveInBattlePhase = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip engineSound;
    [SerializeField] private bool turnOnSound = false;

    // Trạng thái di chuyển
    private bool isMoving = false;
    private Vector3 normalizedMoveDirection;

    private void Start()
    {

        // Chuẩn hóa hướng di chuyển nếu cần
        if (normalizeDirection)
        {
            normalizedMoveDirection = moveDirection.normalized;
        }
        else
        {
            normalizedMoveDirection = moveDirection;
        }

        // Tự động đăng ký với GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterGamePhaseAwareComponent(this);
        }

        // Kiểm tra phase hiện tại và thiết lập trạng thái di chuyển
        CheckCurrentPhaseAndSetMovement();
    }

    private void OnDestroy()
    {
        // Hủy đăng ký khi component bị destroy
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterGamePhaseAwareComponent(this);
        }
    }

    private void Update()
    {
        // Chỉ di chuyển khi isMoving = true
        if (isMoving)
        {
            MoveForward();
        }
    }

    private void MoveForward()
    {
        Vector3 movement = normalizedMoveDirection * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    public void StartMoving()
    {
        isMoving = true;
        if (turnOnSound)
        {
            audioSource.clip = engineSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    #region IGamePhaseAware Implementation

    public virtual void OnPreparePhaseStarted()
    {

        // Dừng di chuyển khi vào prepare phase
        if (stopInPreparePhase)
        {
            StopMoving();
        }
    }

    public virtual void OnBattlePhaseStarted()
    {

        // Bắt đầu di chuyển khi vào battle phase
        if (moveInBattlePhase)
        {
            StartMoving();
        }
    }

    public void OnPhaseChanged(GamePhase newPhase)
    {

        // Xử lý logic chung khi phase thay đổi
        switch (newPhase)
        {
            case GamePhase.Prepare:
                break;
            case GamePhase.Battle:
                break;
        }
    }

    #endregion


    /// <summary>
    /// Kiểm tra phase hiện tại và thiết lập trạng thái di chuyển
    /// </summary>
    private void CheckCurrentPhaseAndSetMovement()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsInPreparePhase())
        {
            if (stopInPreparePhase)
            {
                StopMoving();
            }
        }
        else if (GameManager.Instance.IsInBattlePhase())
        {
            if (moveInBattlePhase)
            {
                StartMoving();
            }
        }
    }
}
