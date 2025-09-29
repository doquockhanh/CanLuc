using UnityEngine;

public class L1_3Conversation : MonoBehaviour
{
    [Header("Conversation")]
    public ActionBase action;
    public EnemyBase bingo;
    private bool isGameOver = false;

    void Start()
    {
        if (UIChatManager.Instance == null) return;
        string chat = "#$&$%& #@^#$^$&%$%&";
        UIChatManager.Instance.SendChat(chat, bingo.type.ToString(), ChatPosition.Right, bingo.transform);
        chat = "Lại gặp lại bí ngoo, nhưng hình như nó mạnh hơn thì phải.. À không, nhanh hơn";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
        chat = "Lần này mày không cản nổi tao đâu nhóc. HAHAHA";
        UIChatManager.Instance.SendChat(chat, bingo.type.ToString(), ChatPosition.Right, bingo.transform);
        chat = "Chiến thôi !";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);

        GameManager.Instance.OnGameOver += OnGameOver;
    }

    void OnGameOver(GameResult result)
    {
        if (isGameOver) return;
        isGameOver = true;
        if (result == GameResult.Fail)
        {
            GameManager.Instance.DelayResultPanel = 6f;
            string chat = "Nhóc hư đốn biết thua chưa.";
            UIChatManager.Instance.SendChat(chat, bingo.type.ToString(), ChatPosition.Right);
            chat = "Hahahaha";
            UIChatManager.Instance.SendChat(chat, bingo.type.ToString(), ChatPosition.Right);
        }
    }

    void OnDestroy()
    {
        UIChatManager.Instance.SkipAllChats();
        GameManager.Instance.OnGameOver -= OnGameOver;
    }
}