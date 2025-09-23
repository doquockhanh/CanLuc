using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class PumkinKingResponse : MonoBehaviour
{
    public string[] responses = { "Đau đó nhóc", "Gruuuurrrr" };
    public string lastWarn = "Anh có thể cho em giảng hòa không";
    private EnemyStats enemyStats;
    private bool lastWarnUsed = false;

    void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
        enemyStats.OnDamageTaken += OnDamageTaken;
        if (GameWorldChatManager.Instance != null)
            GameWorldChatManager.Instance.SendChat("Hahahaha mày cản được tao không nhóc", transform);
    }

    void OnDamageTaken(int damage, GameObject damageSource)
    {
        if (lastWarnUsed || damageSource == null) return;

        StartCoroutine(Chat());
    }

    IEnumerator Chat()
    {
        yield return new WaitForSeconds(2f);
        if (enemyStats.CurrentHealth <= 3 && !lastWarnUsed)
        {
            lastWarnUsed = true;
            if (GameWorldChatManager.Instance != null)
                GameWorldChatManager.Instance.SendChat(lastWarn, transform);
            yield break;
        }

        if (Random.Range(0, 4) <= 1)
        {
            if (GameWorldChatManager.Instance != null)
                GameWorldChatManager.Instance.SendChat(responses[Random.Range(0, responses.Length)], transform);
        }
    }
}