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

        void OnCollisionEnter2D(Collision2D other)
        {
            // Kiểm tra va chạm với Player
            if (IsPlayerCollision(other.gameObject))
            {
                HandlePlayerCollision(other.gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Kiểm tra va chạm trigger với Player
            if (IsPlayerCollision(other.gameObject))
            {
                HandlePlayerCollision(other.gameObject);
            }
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

        #region Movement Logic

        /// <summary>
        /// Di chuyển máy bay về phía trước
        /// </summary>
        private void MoveForward()
        {
            Vector3 movement = normalizedMoveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }

        /// <summary>
        /// Bắt đầu di chuyển
        /// </summary>
        public void StartMoving()
        {
            isMoving = true;
            if (turnOnSound) {
                audioSource.clip = engineSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Dừng di chuyển
        /// </summary>
        public void StopMoving()
        {
            isMoving = false;
        }

        /// <summary>
        /// Thiết lập tốc độ di chuyển
        /// </summary>
        public void SetMoveSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
        }

        /// <summary>
        /// Thiết lập hướng di chuyển
        /// </summary>
        public void SetMoveDirection(Vector3 newDirection)
        {
            moveDirection = newDirection;
            if (normalizeDirection)
            {
                normalizedMoveDirection = moveDirection.normalized;
            }
            else
            {
                normalizedMoveDirection = moveDirection;
            }
        }

        /// <summary>
        /// Kiểm tra xem máy bay có đang di chuyển không
        /// </summary>
        public bool IsMoving()
        {
            return isMoving;
        }

        /// <summary>
        /// Lấy tốc độ di chuyển hiện tại
        /// </summary>
        public float GetCurrentMoveSpeed()
        {
            return moveSpeed;
        }

        /// <summary>
        /// Lấy hướng di chuyển hiện tại
        /// </summary>
        public Vector3 GetCurrentMoveDirection()
        {
            return normalizedMoveDirection;
        }

        /// <summary>
        /// Reset máy bay về vị trí ban đầu
        /// </summary>
        public void ResetPosition()
        {
            // Có thể override trong class con để thiết lập vị trí cụ thể
            transform.position = Vector3.zero;
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

        #endregion

        #region Collision & Combat Logic

        /// <summary>
        /// Kiểm tra xem object có phải là Player không
        /// </summary>
        private bool IsPlayerCollision(GameObject other)
        {
            // Kiểm tra layer
            if (((1 << other.layer) & playerLayerMask) != 0)
            {
                return true;
            }

            // Kiểm tra tag
            if (other.CompareTag("Player"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Xử lý va chạm với Player
        /// </summary>
        private void HandlePlayerCollision(GameObject player)
        {
            if (isDestroyed) return; // Đã bị phá hủy

            if (enableCollisionLogging)
            {
                Debug.Log($"[{gameObject.name}] Collided with Player: {player.name}");
            }

            // Giảm máu
            TakeDamage(1);
            Destroy(player);
        }

        /// <summary>
        /// Nhận sát thương
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Lấy máu hiện tại
        /// </summary>
        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        /// <summary>
        /// Lấy máu tối đa
        /// </summary>
        public int GetMaxHealth()
        {
            return maxHealth;
        }

        /// <summary>
        /// Kiểm tra xem máy bay có bị phá hủy không
        /// </summary>
        public bool IsDestroyed()
        {
            return isDestroyed;
        }

        /// <summary>
        /// Thiết lập điểm số khi bị phá hủy
        /// </summary>
        public void SetScoreValue(int newScoreValue)
        {
            scoreValue = newScoreValue;
        }

        /// <summary>
        /// Lấy điểm số khi bị phá hủy
        /// </summary>
        public int GetScoreValue()
        {
            return scoreValue;
        }

        #endregion

        #region Editor Gizmos

        private void OnDrawGizmosSelected()
        {
            // Vẽ hướng di chuyển trong editor
            if (normalizedMoveDirection != Vector3.zero)
            {
                Gizmos.color = Color.red; // Màu đỏ cho enemy
                Gizmos.DrawRay(transform.position, normalizedMoveDirection * 2f);

                // Vẽ mũi tên
                Vector3 arrowEnd = transform.position + normalizedMoveDirection * 2f;
                Vector3 arrowRight = Vector3.Cross(normalizedMoveDirection, Vector3.forward).normalized * 0.3f;
                Gizmos.DrawLine(arrowEnd, arrowEnd - normalizedMoveDirection * 0.5f + arrowRight);
                Gizmos.DrawLine(arrowEnd, arrowEnd - normalizedMoveDirection * 0.5f - arrowRight);
            }

            // Vẽ health bar
            if (Application.isPlaying && currentHealth > 0)
            {
                Gizmos.color = Color.green;
                Vector3 healthBarStart = transform.position + Vector3.up * 1.5f;
                Vector3 healthBarEnd = healthBarStart + Vector3.right * (currentHealth / (float)maxHealth) * 2f;
                Gizmos.DrawLine(healthBarStart, healthBarEnd);

                // Vẽ khung health bar
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f + Vector3.right, new Vector3(2f, 0.2f, 0.1f));
            }
        }

        #endregion
    }
}
