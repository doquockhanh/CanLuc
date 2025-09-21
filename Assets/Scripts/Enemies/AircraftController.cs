using UnityEngine;

public class AircraftController : EnemyBase
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveDuration = 2f;
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

        // Registration is now handled by EnemyBase.Awake()

        // Kiểm tra phase hiện tại và thiết lập trạng thái di chuyển
        CheckCurrentPhaseAndSetMovement();
    }

    // OnDestroy is now handled by EnemyBase

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

    #region Enemy Execution Override

    protected override void OnEnemyExecuted()
    {
        // Start moving when enemy execution begins
        if (moveInBattlePhase)
        {
            StartMoving();
        }

        // For aircraft, we consider movement as the main action
        // Mark as completed after a certain time or distance
        StartCoroutine(CompleteAfterMovement());
    }

    public override void ResetForNewPhase()
    {
        base.ResetForNewPhase();

        // Reset Aircraft-specific state
        StopMoving();
    }

    private System.Collections.IEnumerator CompleteAfterMovement()
    {
        yield return new WaitForSeconds(moveDuration);

        // Stop moving and mark as completed
        if (stopInPreparePhase)
        {
            StopMoving();
        }

        MarkEnemyCompleted();
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
