using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PumpingKingController : EnemyBase
{
    [Header("Energy System")]
    [SerializeField] private int currentEnergy = 0;
    [SerializeField] private int maxEnergy = 3;

    [Header("Movement")]
    [SerializeField] private Vector3 moveDirection = Vector3.right; // Hướng di chuyển của pumpkin
    [SerializeField] private float moveDistance = 1f; // Khoảng cách di chuyển

    [Header("Skills")]
    [SerializeField] private GameObject projectilePrefab; // Đạn để bắn tạo con
    [SerializeField] private GameObject childEnemyPrefab; // Enemy con được tạo ra
    [SerializeField] private GameObject shieldPrefab; // Shield che chắn

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private LayerMask groundLayerMask = 1; // Layer của ground
    [SerializeField] private float projectileAngle = 30f; // Góc bắn đạn (độ)
    [SerializeField] private float projectileAngleVariation = 10f; // Biến thiên góc (±10 độ)

    [Header("Shield Settings")]
    [SerializeField] private float shieldMoveSpeed = 2f;
    [SerializeField] private float shieldMaxHeight = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip skillSound;

    [Header("Skill Weights & Limits")]
    [SerializeField] private int spawnChildWeight = 1; // Trọng số kỹ năng bắn đạn tạo con
    [SerializeField] private int moveWeight = 3; // Trọng số kỹ năng di chuyển (cao nhất)
    [SerializeField] private int shieldWeight = 1; // Trọng số kỹ năng shield
    [SerializeField] private int shieldMaxUsesPerPhase = 1; // Số lần tối đa dùng shield mỗi phase
    [SerializeField] private int spawnChildMaxUsesPerPhase = 1; // Số lần tối đa dùng spawn child mỗi phase

    private bool isExecutingSkill = false;
    private int shieldUsesThisPhase = 0; // Số lần đã dùng shield trong phase này
    private int spawnChildUsesThisPhase = 0; // Số lần đã dùng spawn child trong phase này
    private bool isWaitingForProjectile = false; // Đang chờ projectile kết thúc

    // Skill queue system for UI preview
    private List<int> skillQueue = new List<int>(); // Queue các skill sẽ thực hiện (để UI preview)
    private int currentSkillIndex = 0; // Index của skill hiện tại trong queue

    #region Energy System

    private bool CanUseSkill()
    {
        return currentEnergy > 0 && !isExecutingSkill;
    }

    private void ConsumeEnergy()
    {
        if (currentEnergy > 0)
        {
            currentEnergy--;
        }
    }

    private void RestoreFullEnergy()
    {
        currentEnergy = maxEnergy;
    }

    #endregion

    #region Enemy Execution Override

    protected override void OnEnemyExecuted()
    {
        // Hồi toàn bộ năng lượng TRƯỚC khi kiểm tra
        RestoreFullEnergy();

        // Tạo skill queue ngay từ đầu
        CreateSkillQueue();

        // Sử dụng tất cả năng lượng trong 1 lần ExecuteEnemy
        StartCoroutine(ExecuteAllSkills());
    }

    public override void ResetForNewPhase()
    {
        base.ResetForNewPhase();
        isExecutingSkill = false;
        isWaitingForProjectile = false; // Reset trạng thái chờ projectile
        shieldUsesThisPhase = 0; // Reset số lần dùng shield
        spawnChildUsesThisPhase = 0; // Reset số lần dùng spawn child

        // Reset skill queue
        skillQueue.Clear();
        currentSkillIndex = 0;

        // Không cần hồi năng lượng ở đây nữa vì đã hồi trong OnEnemyExecuted
    }

    #endregion

    #region Skills System

    /// <summary>
    /// Thực hiện tất cả kỹ năng theo queue đã tạo
    /// </summary>
    private IEnumerator ExecuteAllSkills()
    {
        // Thực hiện từng skill trong queue
        for (int i = 0; i < skillQueue.Count; i++)
        {
            if (currentEnergy <= 0) break;

            currentSkillIndex = i;
            yield return StartCoroutine(ExecuteSingleSkill());

            // Khoảng cách giữa các kỹ năng
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"[{gameObject.name}] All skills executed, marking as completed");
        MarkEnemyCompleted();
    }

    /// <summary>
    /// Thực hiện 1 kỹ năng duy nhất từ queue
    /// </summary>
    private IEnumerator ExecuteSingleSkill()
    {
        if (currentEnergy <= 0 || currentSkillIndex >= skillQueue.Count) yield break;

        isExecutingSkill = true;

        // Trừ năng lượng ngay khi bắt đầu sử dụng kỹ năng
        ConsumeEnergy();

        // Lấy skill từ queue
        int selectedSkill = skillQueue[currentSkillIndex];

        switch (selectedSkill)
        {
            case 0:
                yield return StartCoroutine(ExecuteSpawnChildSkillCoroutine());
                spawnChildUsesThisPhase++; // Tăng số lần dùng spawn child
                break;
            case 1:
                yield return StartCoroutine(ExecuteMoveSkillCoroutine());
                break;
            case 2:
                yield return StartCoroutine(ExecuteShieldSkillCoroutine());
                shieldUsesThisPhase++; // Tăng số lần dùng shield
                break;
        }

        PlaySkillSound();
        isExecutingSkill = false;
    }

    /// <summary>
    /// Tạo skill queue với random selection nhưng có priority: Move -> Spawn Child -> Shield
    /// </summary>
    private void CreateSkillQueue()
    {
        skillQueue.Clear();
        currentSkillIndex = 0;

        // Track số lần đã chọn mỗi skill trong queue này
        int spawnChildSelected = 0;
        int shieldSelected = 0;

        // Tạo queue với số lượng skills = currentEnergy
        for (int i = 0; i < currentEnergy; i++)
        {
            // Tạo danh sách skills có thể chọn với weights (cập nhật theo số lần đã chọn)
            List<int> availableSkills = new List<int>();
            List<int> weights = new List<int>();

            // Kỹ năng 0: Spawn Child (chỉ thêm nếu chưa đạt giới hạn)
            if (spawnChildSelected < spawnChildMaxUsesPerPhase)
            {
                availableSkills.Add(0);
                weights.Add(spawnChildWeight);
            }

            // Kỹ năng 1: Move (luôn có)
            availableSkills.Add(1);
            weights.Add(moveWeight);

            // Kỹ năng 2: Shield (chỉ thêm nếu chưa đạt giới hạn)
            if (shieldSelected < shieldMaxUsesPerPhase)
            {
                availableSkills.Add(2);
                weights.Add(shieldWeight);
            }

            // Nếu không có skill nào khả dụng, break
            if (availableSkills.Count == 0) break;

            // Chọn skill random dựa trên weight
            int selectedSkill = SelectWeightedSkillFromList(availableSkills, weights);
            skillQueue.Add(selectedSkill);

            // Tăng counter cho skill đã chọn
            switch (selectedSkill)
            {
                case 0: spawnChildSelected++; break;
                case 2: shieldSelected++; break;
            }
        }

        // Sắp xếp lại queue theo priority: Move -> Spawn Child -> Shield
        skillQueue.Sort((a, b) => GetSkillPriority(a).CompareTo(GetSkillPriority(b)));

        // Log skill queue để debug
        string queueString = "Skill Queue: ";
        foreach (int skill in skillQueue)
        {
            string skillName = skill switch
            {
                0 => "Spawn Child",
                1 => "Move",
                2 => "Shield",
                _ => "Unknown"
            };
            queueString += skillName + " -> ";
        }
        Debug.Log($"[{gameObject.name}] {queueString.TrimEnd(' ', '-', '>')}");
    }

    /// <summary>
    /// Lấy priority của skill (số nhỏ hơn = ưu tiên cao hơn)
    /// </summary>
    private int GetSkillPriority(int skillId)
    {
        return skillId switch
        {
            1 => 1, // Move - ưu tiên cao nhất
            0 => 2, // Spawn Child - ưu tiên thứ 2
            2 => 3, // Shield - ưu tiên thấp nhất
            _ => 999
        };
    }

    /// <summary>
    /// Chọn skill random từ danh sách có sẵn dựa trên weight
    /// </summary>
    private int SelectWeightedSkillFromList(List<int> availableSkills, List<int> weights)
    {
        if (availableSkills.Count == 0) return 1; // Fallback to Move

        // Weighted random selection
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < availableSkills.Count; i++)
        {
            currentWeight += weights[i];
            if (randomValue < currentWeight)
            {
                return availableSkills[i];
            }
        }

        // Fallback
        return availableSkills[0];
    }

    /// <summary>
    /// Tính toán hướng bắn đạn dựa trên hướng di chuyển và góc
    /// </summary>
    private Vector3 CalculateProjectileDirection()
    {
        // Lấy hướng di chuyển chuẩn hóa
        Vector3 normalizedMoveDir = moveDirection.normalized;

        // Tính góc random trong khoảng [angle - variation, angle + variation]
        float randomAngle = projectileAngle + Random.Range(-projectileAngleVariation, projectileAngleVariation);

        // Chuyển đổi từ độ sang radian
        float angleInRadians = randomAngle * Mathf.Deg2Rad;

        // Tính toán hướng bắn dựa trên hướng di chuyển và góc
        // Giả sử hướng di chuyển là hướng ngang, ta sẽ xoay một góc để tạo hướng bắn
        Vector3 shootDirection;

        if (Mathf.Abs(normalizedMoveDir.x) > Mathf.Abs(normalizedMoveDir.y))
        {
            // Di chuyển chủ yếu theo trục X (ngang)
            if (normalizedMoveDir.x > 0)
            {
                // Di chuyển sang phải, bắn xuống dưới với góc
                shootDirection = new Vector3(Mathf.Sin(angleInRadians), -Mathf.Cos(angleInRadians), 0);
            }
            else
            {
                // Di chuyển sang trái, bắn xuống dưới với góc
                shootDirection = new Vector3(-Mathf.Sin(angleInRadians), -Mathf.Cos(angleInRadians), 0);
            }
        }
        else
        {
            // Di chuyển chủ yếu theo trục Y (dọc)
            if (normalizedMoveDir.y > 0)
            {
                // Di chuyển lên trên, bắn xuống dưới với góc
                shootDirection = new Vector3(Mathf.Sin(angleInRadians), -Mathf.Cos(angleInRadians), 0);
            }
            else
            {
                // Di chuyển xuống dưới, bắn xuống dưới với góc
                shootDirection = new Vector3(Mathf.Sin(angleInRadians), -Mathf.Cos(angleInRadians), 0);
            }
        }

        return shootDirection.normalized;
    }

    #endregion

    #region Public Methods for UI

    /// <summary>
    /// Lấy skill queue hiện tại (để hiển thị UI)
    /// </summary>
    public List<int> GetSkillQueue()
    {
        return new List<int>(skillQueue);
    }

    /// <summary>
    /// Lấy tên skill từ ID
    /// </summary>
    public string GetSkillName(int skillId)
    {
        return skillId switch
        {
            0 => "Spawn Child",
            1 => "Move",
            2 => "Shield",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Kiểm tra xem skill queue đã được tạo chưa
    /// </summary>
    public bool IsSkillQueueReady()
    {
        return skillQueue.Count > 0;
    }

    /// <summary>
    /// Lấy skill hiện tại đang thực hiện
    /// </summary>
    public int GetCurrentSkill()
    {
        if (currentSkillIndex < skillQueue.Count)
        {
            return skillQueue[currentSkillIndex];
        }
        return -1; // Không có skill nào
    }

    #endregion

    #region Individual Skills

    /// <summary>
    /// Kỹ năng 1: Bắn đạn tạo con
    /// </summary>
    private IEnumerator ExecuteSpawnChildSkillCoroutine()
    {
        Debug.Log($"[{gameObject.name}] Executing Spawn Child Skill");

        if (projectilePrefab == null || childEnemyPrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Missing projectile or child enemy prefab!");
            yield break;
        }

        // Tạo projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        ProjectileController projectileController = projectile.GetComponent<ProjectileController>();

        if (projectileController == null)
        {
            projectileController = projectile.AddComponent<ProjectileController>();
        }

        // Tính toán hướng bắn dựa trên hướng di chuyển và góc
        Vector3 shootDirection = CalculateProjectileDirection();

        // Thiết lập trạng thái chờ projectile
        isWaitingForProjectile = true;

        // Thiết lập projectile với hướng bắn đã tính toán và callback khi kết thúc
        projectileController.Initialize(shootDirection, projectileSpeed, OnProjectileHit, OnProjectileDestroyed);

        // Chờ đến khi projectile kết thúc (va chạm hoặc bị hủy)
        yield return new WaitUntil(() => !isWaitingForProjectile);

        if (Random.Range(0, 3) <= 1)
        {
            if (GameWorldOpenChat.Instance != null)
                GameWorldOpenChat.Instance.WriteChat(transform, "Anh em tao đông");
        }

        Debug.Log($"[{gameObject.name}] Spawn Child Skill completed - projectile finished");
    }

    /// <summary>
    /// Kỹ năng 2: Di chuyển về phía trước
    /// </summary>
    private IEnumerator ExecuteMoveSkillCoroutine()
    {
        yield return StartCoroutine(MoveForwardCoroutine());
    }

    /// <summary>
    /// Kỹ năng 3: Tạo shield che chắn
    /// </summary>
    private IEnumerator ExecuteShieldSkillCoroutine()
    {
        if (shieldPrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Missing shield prefab!");
            yield break;
        }

        // Tạo shield tại vị trí hiện tại
        GameObject shield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);

        // Di chuyển shield lên trên
        yield return StartCoroutine(MoveShieldUp(shield));

        if (Random.Range(0, 3) <= 1)
        {
            if (GameWorldOpenChat.Instance != null)
                GameWorldOpenChat.Instance.WriteChat(transform, "Tao có khiên");
        }

        // Chờ một khoảng thời gian ngắn
        yield return new WaitForSeconds(0.3f);
    }

    #endregion

    #region Skill Coroutines

    private IEnumerator MoveForwardCoroutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + moveDirection.normalized * moveDistance;

        float moveDuration = 1f; // Thời gian di chuyển
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        transform.position = targetPosition;
    }

    private IEnumerator MoveShieldUp(GameObject shield)
    {
        Vector3 startPosition = shield.transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * shieldMaxHeight + moveDirection * 2f;

        while (Vector3.Distance(shield.transform.position, targetPosition) > 0.1f)
        {
            shield.transform.position = Vector3.MoveTowards(
                shield.transform.position,
                targetPosition,
                shieldMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        shield.transform.position = targetPosition;
    }

    #endregion

    #region Projectile Hit Handling

    private void OnProjectileHit(Vector3 hitPosition)
    {
        // Tạo enemy con tại vị trí va chạm
        if (childEnemyPrefab != null)
        {
            GameObject childEnemy = Instantiate(childEnemyPrefab, hitPosition + Vector3.up * 2f, Quaternion.identity);
        }

        // Kết thúc chờ projectile
        isWaitingForProjectile = false;
    }

    /// <summary>
    /// Được gọi khi projectile bị hủy (hết thời gian sống)
    /// </summary>
    private void OnProjectileDestroyed()
    {
        Debug.Log($"[{gameObject.name}] Projectile destroyed without hitting target");

        // Kết thúc chờ projectile
        isWaitingForProjectile = false;
    }

    #endregion

    #region Audio

    private void PlaySkillSound()
    {
        if (audioSource != null && skillSound != null)
        {
            audioSource.PlayOneShot(skillSound);
        }
    }

    #endregion
}
