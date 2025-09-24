using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class GhostController : EnemyBase
{
    [Header("Energy System")]
    [SerializeField] private int currentEnergy = 0;
    [SerializeField] private int maxEnergy = 2;

    [Header("Movement Settings")]
    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float moveDistance = 2f; // Khoảng cách di chuyển cố định
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float retreatDistance = 3f;

    [Header("Hide Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private EnemyHpBarController hpBar;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip skillSound;

    [Header("Skill Weights & Limits")]
    [SerializeField] private int moveWeight = 3; // Trọng số kỹ năng di chuyển (cao nhất)
    [SerializeField] private int hideWeight = 1; // Trọng số kỹ năng trốn
    [SerializeField] private int hideMaxUsesPerPhase = 1; // Số lần tối đa dùng hide mỗi phase

    private bool hasAttackedThisPhase = false; // Để track đã tấn công trong phase này
    private bool isInvisible = false; // Trạng thái tàng hình

    // Skill queue system for UI preview
    private List<int> skillQueue = new List<int>(); // Queue các skill sẽ thực hiện (để UI preview)
    private int currentSkillIndex = 0; // Index của skill hiện tại trong queue
    private EnemyStats enemyStats;

    #region Energy System

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
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
        // Nếu đang tàng hình, hiện lại khi bắt đầu phase mới
        if (isInvisible)
        {
            BecomeVisible();
        }

        // Tạo skill queue ngay từ đầu
        CreateSkillQueue();

        // Sử dụng tất cả năng lượng trong 1 lần ExecuteEnemy
        StartCoroutine(ExecuteAllSkills());
    }

    public override void ResetForNewPhase()
    {
        base.ResetForNewPhase();
        hasAttackedThisPhase = false; // Reset trạng thái tấn công

        // Reset skill queue
        skillQueue.Clear();
        currentSkillIndex = 0;
    }

    #endregion

    #region Skills System

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

    private IEnumerator ExecuteSingleSkill()
    {
        if (currentEnergy <= 0 || currentSkillIndex >= skillQueue.Count) yield break;

        // Trừ năng lượng ngay khi bắt đầu sử dụng kỹ năng
        ConsumeEnergy();

        // Lấy skill từ queue
        int selectedSkill = skillQueue[currentSkillIndex];

        switch (selectedSkill)
        {
            case 0:
                yield return StartCoroutine(ExecuteMoveSkillCoroutine());
                break;
            case 1:
                yield return StartCoroutine(ExecuteHideSkillCoroutine());
                break;
        }

        PlaySkillSound();
    }

    private void CreateSkillQueue()
    {
        skillQueue.Clear();
        currentSkillIndex = 0;

        // Track số lần đã chọn mỗi skill trong queue này
        int hideSelected = 0;

        // Tạo queue với số lượng skills = currentEnergy
        for (int i = 0; i < currentEnergy; i++)
        {
            // Tạo danh sách skills có thể chọn với weights (cập nhật theo số lần đã chọn)
            List<int> availableSkills = new List<int>();
            List<int> weights = new List<int>();

            // Kỹ năng 0: Move (luôn có)
            availableSkills.Add(0);
            weights.Add(moveWeight);

            // Kỹ năng 1: Hide (chỉ thêm nếu chưa đạt giới hạn)
            if (hideSelected < hideMaxUsesPerPhase)
            {
                availableSkills.Add(1);
                weights.Add(hideWeight);
            }

            // Nếu không có skill nào khả dụng, break
            if (availableSkills.Count == 0) break;

            // Chọn skill random dựa trên weight
            int selectedSkill = SelectWeightedSkillFromList(availableSkills, weights);
            skillQueue.Add(selectedSkill);

            // Tăng counter cho skill đã chọn
            switch (selectedSkill)
            {
                case 1: hideSelected++; break;
            }
        }

        // Sắp xếp lại queue theo priority: Move -> Hide
        skillQueue.Sort((a, b) => GetSkillPriority(a).CompareTo(GetSkillPriority(b)));

        // Log skill queue để debug
        string queueString = "Skill Queue: ";
        foreach (int skill in skillQueue)
        {
            string skillName = skill switch
            {
                0 => "Move",
                1 => "Hide",
                _ => "Unknown"
            };
            queueString += skillName + " -> ";
        }
        Debug.Log($"[{gameObject.name}] {queueString.TrimEnd(' ', '-', '>')}");
    }

    private int GetSkillPriority(int skillId)
    {
        return skillId switch
        {
            0 => 1, // Move - ưu tiên cao nhất
            1 => 2, // Hide - ưu tiên thấp nhất
            _ => 999
        };
    }

    private int SelectWeightedSkillFromList(List<int> availableSkills, List<int> weights)
    {
        if (availableSkills.Count == 0) return 0; // Fallback to Move

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

    #endregion

    #region Individual Skills

    private IEnumerator ExecuteMoveSkillCoroutine()
    {
        if (hasAttackedThisPhase)
        {
            yield return StartCoroutine(ExecuteHideSkillCoroutine());
            yield break;
        }
        // Tìm action gần nhất
        ActionBase nearestAction = FindNearestAction();
        if (nearestAction == null)
        {
            Debug.Log($"[{gameObject.name}] No actions found, skipping move");
            yield break;
        }

        // Di chuyển theo hướng action với khoảng cách cố định
        yield return StartCoroutine(MoveTowardsTarget(nearestAction.transform.position));

        // Kiểm tra xem có trong tầm tấn công không và chưa tấn công trong phase này
        float distanceToTarget = Vector3.Distance(transform.position, nearestAction.transform.position);
        if (distanceToTarget <= attackRange && !hasAttackedThisPhase)
        {
            // Tấn công
            yield return StartCoroutine(AttackTarget(nearestAction));

            // Nếu tấn công thành công, bay ra xa (hit and run)
            if (hasAttackedThisPhase && nearestAction != null)
            {
                yield return StartCoroutine(RetreatFromTarget(nearestAction.transform.position));
            }
        }
    }
    private IEnumerator ExecuteHideSkillCoroutine()
    {
        enemyStats.Heal(healAmount);

        // Trở nên tàng hình hoàn toàn
        BecomeInvisible();

        // Chat với tỉ lệ 1/2
        if (Random.Range(0, 2) == 0)
        {
            if (GameWorldChatManager.Instance != null)
                GameWorldChatManager.Instance.SendChat("Đố anh bắt được em", transform);
        }

        yield return null; // Thêm yield để method trả về IEnumerator
    }

    #endregion

    #region Skill Coroutines

    private IEnumerator MoveTowardsTarget(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 direction = (targetPosition - startPosition).normalized;
        Vector3 targetMovePosition = startPosition + direction * moveDistance;

        float elapsedTime = 0f;
        float moveDuration = moveDistance / flySpeed;
        float distanceCheckTimer = 0f;
        const float distanceCheckInterval = 0.1f; // Kiểm tra khoảng cách mỗi 0.1 giây

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;

            Vector3 newPosition = Vector3.Lerp(startPosition, targetMovePosition, progress);
            transform.position = newPosition;

            // Kiểm tra khoảng cách mỗi 0.1 giây để tối ưu hiệu suất
            distanceCheckTimer += Time.deltaTime;
            if (distanceCheckTimer >= distanceCheckInterval)
            {
                distanceCheckTimer = 0f;
                float currentDistance = Vector3.Distance(transform.position, targetPosition);
                if (currentDistance <= attackRange)
                {
                    // Đã đủ gần để tấn công, dừng di chuyển
                    break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator AttackTarget(ActionBase target)
    {
        // Gây sát thương cho target
        if (target.TryGetComponent<ActionStats>(out var actionStats))
        {
            actionStats.TakeDamage(enemyStats.Damage, gameObject);
            hasAttackedThisPhase = true;
            if (Random.Range(0, 2) == 0)
            {
                if (GameWorldChatManager.Instance != null)
                    GameWorldChatManager.Instance.SendChat("Tao đánh mày nè", transform);
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator RetreatFromTarget(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 retreatDirection = (startPosition - targetPosition).normalized;

        // Thêm xu hướng bay lên (70% cơ hội bay lên, 30% bay xuống)
        float upBias = Random.Range(0f, 1f) < 0.7f ? 1f : -0.5f;
        retreatDirection.y += upBias * 0.3f; // Thêm thành phần Y để bay lên/xuống
        retreatDirection = retreatDirection.normalized;

        Vector3 retreatTarget = startPosition + retreatDirection * retreatDistance;

        float elapsedTime = 0f;
        float retreatDuration = retreatDistance / flySpeed;

        while (elapsedTime < retreatDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / retreatDuration;

            transform.position = Vector3.Lerp(startPosition, retreatTarget, progress);
            yield return null;
        }

        transform.position = retreatTarget;
    }

    private void BecomeInvisible()
    {
        isInvisible = true;

        // Làm sprite renderer hoàn toàn trong suốt (chỉ ảnh hưởng sprite chính, không ảnh hưởng HP bar)
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            Color invisibleColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            spriteRenderer.color = invisibleColor;
        }

        if (hpBar != null) hpBar.BackgroundImage.SetActive(false);
    }

    private void BecomeVisible()
    {
        isInvisible = false;

        // Trở lại bình thường cho sprite chính
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        if (hpBar != null) hpBar.BackgroundImage.SetActive(true);
    }

    #endregion

    #region Helper Methods

    private ActionBase FindNearestAction()
    {
        ActionBase[] allActions = GameManager.Instance.Actions;
        if (allActions.Length == 0) return null;

        ActionBase nearestAction = null;
        float nearestDistance = float.MaxValue;

        foreach (ActionBase action in allActions)
        {
            if (action == null) continue;

            float distance = Vector3.Distance(transform.position, action.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestAction = action;
            }
        }

        return nearestAction;
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
