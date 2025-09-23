using UnityEngine;

[RequireComponent(typeof(ActionStats))]
public class HaoNamResponse : MonoBehaviour
{
    public string[] responses = { "Á hự", "*_*", "Đau quá", "Né bọn nó" };
    public string lastWarn = "Bố mày sắp cút rồi, chơi cẩn thận vào";
    public string firstWarn = "Từ lúc sinh ra chưa ngán ai";
    private ActionStats actionStats;

    void Start()
    {
        actionStats = GetComponent<ActionStats>();
        actionStats.OnDamageTaken += OnDamageTaken;
        if (GameWorldChatManager.Instance != null)
            GameWorldChatManager.Instance.SendChat(firstWarn, transform);
    }

    void OnDamageTaken(int damage, GameObject damageSource)
    {
        string response = responses[Random.Range(0, responses.Length)];

        if (actionStats.CurrentHealth <= 1)
        {
            response = lastWarn;
        }
        
        if (GameWorldChatManager.Instance != null)
            GameWorldChatManager.Instance.SendChat(response, transform);
    }
}