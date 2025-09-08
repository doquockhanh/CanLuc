using UnityEngine;
using Gameplay.Core;
using Gameplay.Interfaces;

namespace Gameplay.Enemies
{
    /// <summary>
    /// Controller cho máy bay enemy - di chuyển về phía trước với tốc độ cố định
    /// Chỉ di chuyển khi game ở battle phase, có hệ thống máu và va chạm với Player
    /// </summary>
    public class AircraftController : MonoBehaviour, IGamePhaseAware
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Vector3 moveDirection = Vector3.right; // Di chuyển về phía phải (trục X)
        [SerializeField] private bool normalizeDirection = true;

        [Header("Health & Combat")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private int currentHealth;
        [SerializeField] private int scoreValue = 100; // Điểm khi bị phá hủy
        [SerializeField] private bool isDestroyed = false;

        [Header("Phase Control")]
        [SerializeField] private bool enablePhaseLogging = true;
        [SerializeField] private bool stopInPreparePhase = true;
        [SerializeField] private bool moveInBattlePhase = true;

        [Header("Collision Settings")]
        [SerializeField] private LayerMask playerLayerMask = 1 << 8; // Layer "Player" (mặc định layer 8)
        [SerializeField] private bool enableCollisionLogging = true;
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip engineSound;
        [SerializeField] private bool turnOnSound = false;

        // Trạng thái di chuyển
        private bool isMoving = false;
        private Vector3 normalizedMoveDirection;

        private void Start()
        {
            // Khởi tạo máu
            currentHealth = maxHealth;

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
            if (turnOnSound) {
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
            if (enablePhaseLogging)
            {
                Debug.Log($"[{gameObject.name}] Aircraft entered Prepare Phase");
            }

            // Dừng di chuyển khi vào prepare phase
            if (stopInPreparePhase)
            {
                StopMoving();
            }
        }

        public virtual void OnBattlePhaseStarted()
        {
            if (enablePhaseLogging)
            {
                Debug.Log($"[{gameObject.name}] Aircraft entered Battle Phase");
            }

            // Bắt đầu di chuyển khi vào battle phase
            if (moveInBattlePhase)
            {
                StartMoving();
            }
        }

        public void OnPhaseChanged(GamePhase newPhase)
        {
            if (enablePhaseLogging)
            {
                Debug.Log($"[{gameObject.name}] Aircraft Game Phase Changed to: {newPhase}");
            }

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

        /// </summary>
        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            currentHealth -= damage;

            if (enableCollisionLogging)
            {
                Debug.Log($"[{gameObject.name}] Took {damage} damage. Health: {currentHealth}/{maxHealth}");
            }

            // Kiểm tra xem có bị phá hủy không
            if (currentHealth <= 0)
            {
                DestroyAircraft();
            }
        }

        /// <summary>
        /// Phá hủy máy bay
        /// </summary>
        private void DestroyAircraft()
        {
            if (isDestroyed) return;

            isDestroyed = true;

            if (enableCollisionLogging)
            {
                Debug.Log($"[{gameObject.name}] Aircraft destroyed! Awarding {scoreValue} points");
            }

            // Cộng điểm cho màn chơi
            AwardScore();

            // Tắt movement
            StopMoving();

            // Có thể thêm effect hoặc animation ở đây
            // Ví dụ: explosion effect, sound, etc.

            // Destroy GameObject sau một khoảng thời gian ngắn
            Destroy(gameObject, 0.1f);
        }

        /// <summary>
        /// Cộng điểm cho màn chơi
        /// </summary>
        private void AwardScore()
        {
            // Tìm ScoreManager để cộng điểm
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreValue);
            }
            else
            {
                // Fallback: log warning nếu không có ScoreManager
                Debug.LogWarning($"[{gameObject.name}] ScoreManager not found! Cannot award {scoreValue} points");
            }
        }
    }
}
