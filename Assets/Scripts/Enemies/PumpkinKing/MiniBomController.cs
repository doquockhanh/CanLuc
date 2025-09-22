using UnityEngine;
using System.Collections;

public class MiniBomController : EnemyBase
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.right; // Hướng di chuyển
    [SerializeField] private float moveDistance = 2f; // Khoảng cách di chuyển
    [SerializeField] private float moveSpeed = 3f; // Tốc độ di chuyển

    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionEffectPrefab; // Effect phát nổ
    [SerializeField] private float explosionRadius = 1.5f; // Bán kính vụ nổ
    [SerializeField] private int explosionDamage = 1; // Sát thương vụ nổ (tạm thời)
    [SerializeField] private LayerMask actionLayerMask = 1; // Layer của Action

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;

    private bool hasMoved = false; // Đã di chuyển chưa
    private bool isMoving = false; // Đang di chuyển
    private Vector3 startPosition; // Vị trí bắt đầu

    protected override void Awake()
    {
        base.Awake(); // Đăng ký với PhaseManager
    }

    #region Enemy Execution Override

    protected override void OnEnemyExecuted()
    {
        // Lần đầu tiên được tạo ra: không làm gì cả
        if (!hasMoved)
        {
            Debug.Log($"[{gameObject.name}] Child enemy spawned, waiting for next phase");
            MarkEnemyCompleted();
            return;
        }

        // Các phase tiếp theo: di chuyển
        if (!isMoving)
        {
            StartCoroutine(MoveToTarget());
        }
    }

    public override void ResetForNewPhase()
    {
        base.ResetForNewPhase();

        // Đánh dấu đã có thể di chuyển ở phase tiếp theo
        if (!hasMoved)
        {
            hasMoved = true;
            Debug.Log($"[{gameObject.name}] Child enemy ready to move in next phase");
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// Di chuyển đến vị trí đích
    /// </summary>
    private IEnumerator MoveToTarget()
    {
        isMoving = true;
        startPosition = transform.position;
        float moveDuration = moveDistance / moveSpeed; // thời gian cần để đi hết khoảng cách
        float elapsedTime = 0f;

        // Di chuyển smooth đến vị trí đích
        while (elapsedTime < moveDuration)
        {
            transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isMoving = false;

        Debug.Log($"[{gameObject.name}] Reached target position");
        MarkEnemyCompleted();
    }

    #endregion

    #region Collision Detection

    // private void OnTriggerEnter(Collider other)
    // {
    //     // Kiểm tra va chạm với Action
    //     if (IsInLayerMask(other.gameObject.layer, actionLayerMask))
    //     {
    //         Debug.Log($"[{gameObject.name}] Collided with Action: {other.gameObject.name}");
    //         Explode(other.gameObject);
    //     }
    // }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     // Kiểm tra va chạm với Action (nếu sử dụng Collision thay vì Trigger)
    //     if (IsInLayerMask(collision.gameObject.layer, actionLayerMask))
    //     {
    //         Debug.Log($"[{gameObject.name}] Collided with Action: {collision.gameObject.name}");
    //         Explode(collision.gameObject);
    //     }
    // }

    /// <summary>
    /// Kiểm tra xem layer có trong LayerMask không
    /// </summary>
    // private bool IsInLayerMask(int layer, LayerMask layerMask)
    // {
    //     return (layerMask.value & (1 << layer)) != 0;
    // }

    #endregion


    // private void Explode(GameObject targetAction)
    // {

    // }

    // private void DealDamageToAction(GameObject action)
    // {
    // }
}
