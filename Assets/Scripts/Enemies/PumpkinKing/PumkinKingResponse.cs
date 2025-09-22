using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActionStats))]
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
        if (GameWorldOpenChat.Instance != null)
            GameWorldOpenChat.Instance.WriteChat(transform, "Hahahaha mày cản được tao không nhóc");
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
            if (GameWorldOpenChat.Instance != null)
                GameWorldOpenChat.Instance.WriteChat(transform, lastWarn);
            yield break;
        }

        if (Random.Range(0, 4) <= 1)
        {
            if (GameWorldOpenChat.Instance != null)
                GameWorldOpenChat.Instance.WriteChat(transform, responses[Random.Range(0, responses.Length)]);
        }
    }
}