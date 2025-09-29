using UnityEngine;

public class L1_2Conversation : MonoBehaviour
{
    [Header("Conversation")]
    public ActionBase action;
    public EnemyBase ghost;
    private bool isGameOver = false;
    void Start()
    {
        if (UIChatManager.Instance == null) return;
        string chat = "#$&$%& #@^#$^$&%$%&";
        UIChatManager.Instance.SendChat(chat, ghost.type.ToString(), ChatPosition.Right, ghost.transform);
        chat = "Ồ! bé ma nhỏ đang làm gì ở đây thế";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
        chat = "Ở đây để dọa chết ngươi";
        UIChatManager.Instance.SendChat(chat, ghost.type.ToString(), ChatPosition.Right, ghost.transform);
        chat = "Chiến thôi !";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
        chat = "*Ma tí nị có thể nhìn vô hại nhưng đừng xem thường nó. Sẽ rất buồn cười nếu bạn thua*";
        UIChatManager.Instance.SendChat(chat, "System", ChatPosition.Middle);

        GameManager.Instance.OnGameOver += OnGameOver;
    }

    void OnGameOver(GameResult result)
    {
        if (isGameOver) return;
        isGameOver = true;
        if (result == GameResult.Fail)
        {
            GameManager.Instance.DelayResultPanel = 6f;
            string chat = "Hahaha, ta dọa chết ngươi";
            UIChatManager.Instance.SendChat(chat, ghost.type.ToString(), ChatPosition.Middle, ghost.transform);
        }
    }

    void OnDestroy()
    {
        UIChatManager.Instance.SkipAllChats();
        GameManager.Instance.OnGameOver -= OnGameOver;
    }
}