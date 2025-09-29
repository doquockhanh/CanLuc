using System.Collections;
using UnityEngine;

public class L1_1Conversation : MonoBehaviour
{
    [Header("Conversation")]
    public ActionBase action;
    public EnemyBase boss;
    public EnemyBase biNgo;
    
    // Event khi conversation hoàn thành
    public System.Action OnConversationCompleted;

    void Start()
    {
        if (UIChatManager.Instance == null) return;
        if (PlayerPrefs.GetInt("Scene1TutorialShown", 0) == 0)
        {
            StartConversationForFirstTime();

            // Đánh dấu là đã hiện
            PlayerPrefs.SetInt("Scene1TutorialShown", 1);
            PlayerPrefs.Save();
        }
        else
        {
            StartConversation();
        }

        // PlayerPrefs.DeleteKey("Scene1TutorialShown");
        // PlayerPrefs.Save();
    }

    void StartConversationForFirstTime()
    {
        StartCoroutine(CameraController.Instance.ShakeCamera(2f, 0.5f));
        // Delay 2 giây sau khi shake xong
        Invoke(nameof(StartBossChat), 2f);
    }

    void StartBossChat()
    {
        string chat = "Hahahahaha";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right, boss.transform);
        chat = "? ";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left);
        chat = "napj #$%^&*()_+ tieenf @$%#$%#& vaof #$^$%*%# doo ###$&$* neet @$^# cho $%&$%tao HAHAHAHAHAHAHAHAHAHAH";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right);
        chat = "Phù thủy điên, bà ta sẽ lây bệnh điên cho tất cả mọi người.";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left);
        chat = "Có mày bị điên, cả nhà mày bị điên.";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right);
        chat = "Ta là phù thủy tối thượng, ta sẽ đem tất cả hài dón trên thế giới này nướng trên ngọn lửa, bỏ vào thùng rác rồi vứt xuống biển cho cá ăn.";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right);
        chat = "Loài người sẽ bị tiêu diệt. HAHAHAHAHAHAHA";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right);
        chat = "OK vậy ta sẽ giết ngươi để giải cứu thế giới này.";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform, ChatToGuilde);
    }

    void ChatToGuilde()
    {
        // Delay 2 giây trước khi bắt đầu
        Invoke(nameof(StartGuideShake), 2f);
    }

    void StartGuideShake()
    {
        StartCoroutine(CameraController.Instance.ShakeCamera(2f, 0.5f));
        string chat = "Con boss bỏ đi chỉ để lại một con bí ngô.";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, biNgo.transform);
        // Delay 2 giây sau khi chat xong
        Invoke(nameof(StartConversation), 2f);
    }
    void StartConversation()
    {
        Destroy(boss.gameObject);
        string chat = "Bắn chết con bí ngô này thì thắng, đừng để nó đi tới vạch cuối cùng";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, biNgo.transform);
        chat = "Nhào vô ăn tao";
        UIChatManager.Instance.SendChat(chat, biNgo.type.ToString(), ChatPosition.Right);
        chat = "CHiến thôi!";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform, OnConversationFinish);
    }
    
    void OnConversationFinish()
    {
        // Fire event khi conversation hoàn thành
        OnConversationCompleted?.Invoke();
    }

    void OnDestroy()
    {
        UIChatManager.Instance.SkipAllChats();
    }
}