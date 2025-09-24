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
        chat = "*Ma tí nị có thể nhìn vô hại nhưng đừng xem thường nó*";
        UIChatManager.Instance.SendChat(chat, "System", ChatPosition.Middle);
        chat = "*Tôi sẽ xem thường bạn nếu bạn thua..*";
        UIChatManager.Instance.SendChat(chat, "System", ChatPosition.Middle);
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
            string chat = "*Ồ thực sự để thua một con ma máu ít hơn, dame nhỏ hơn*";
            UIChatManager.Instance.SendChat(chat, "System", ChatPosition.Middle);
            chat = "*EM quê ở đâu, sao em quê thế?*";
            UIChatManager.Instance.SendChat(chat, "System", ChatPosition.Middle);
        }
    }

    void OnDestroy()
    {
        GameManager.Instance.OnGameOver -= OnGameOver;
    }
    }