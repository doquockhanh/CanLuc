using System.Collections;
using UnityEngine;

public class L1_1Conversation : MonoBehaviour
{
    [Header("Conversation")]
    public ActionBase action;
    public EnemyBase boss;
    public EnemyBase biNgo;

    void Start()
    {
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
        string chat = "Tôi biết 69% người chơi đều muốn \n skip đoạn hội thoại nhàm chán này. \n Nhưng tôi tin bạn là một tron 35% người đặc biệt đến đây để giải cứu con game Lmao này";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
        chat = "Nên để phòng trường hợp bạn \n chưa chơi hướng dẫn mà tôi đã mất cả ngày để tạo nó. Thì tôi sẽ hướng dẫn cơ bản.";
        UIChatManager.Instance.SendChat(chat, action.type.ToString());
        chat = "Mà khoan, Tôi phải nói chuyện với phản diện trước để xong cốt truyện đã. Cốt truyện chỉ phát một lần duy nhất nên ráng mà đọc nhé";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Middle, null, 2f, ChatWithBoss);
    }

    void ChatWithBoss()
    {
        StartCoroutine(DoChatWithBoss());
    }

    IEnumerator DoChatWithBoss()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(CameraController.Instance.ShakeCamera(2f, 0.5f));
        yield return new WaitForSeconds(2f);
        string chat = "Hahahahaha";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right, boss.transform, 2f);
        chat = "HAHAHAHAHAHAHAHAHAHAHAHAHAHAHHAHAHAHAHAHAHAHAHAHAHAHAHAHA ";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right);
        chat = "? ";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left);
        chat = "napj #$%^&*()_+ tieenf @$%#$%#& vaof #$^$%*%# doo ###$&$* neet @$^# cho $%&$%tao";
        UIChatManager.Instance.SendChat(chat, boss.type.ToString(), ChatPosition.Right);
        chat = "HAHAHAHAHAHAHAHAHAHAH khẹc khẹc";
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
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform, 1f, ChatToGuilde);
    }

    void ChatToGuilde()
    {
        StartCoroutine(DoChatToGuilde());
    }

    IEnumerator DoChatToGuilde()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(CameraController.Instance.ShakeCamera(2f, 0.5f));
        string chat = "Con boss bỏ đi chỉ để lại một con bí ngô. Ok xong cốt truyện";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
        yield return new WaitForSeconds(2f);
        chat = "Bây giờ quay lại vấn đề";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left);
        chat = "Tôi là Hào Nam, tính đẹp trai, người tốt bụng";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left);
        chat = " Vì súng của tôi có cấu tạo đặc biệt nên khi bắn sẽ bantumlum";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left);
        chat = "Nhưng tôi là người kỹ năng nên bantumlum vẫn trúng hết được";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left);
        chat = "Tôi đang cầm súng. Hãy click vào tôi";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
        yield return new WaitForSeconds(21f);
        chat = "A/D di chuyển!! W/S xoay súng!!";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
        chat = "Thanh lực trên là lực mà viên đạn bay ra. Thanh lực dưới đang đầy là sức lực của tôi, di chuyển khá tốn sức.";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform, 0, StartConversation);
    }

    void StartConversation()
    {
        Destroy(boss.gameObject);
        UIChatManager.Instance.SendChat("", action.type.ToString());
        string chat = "Bắn chết con bí ngô này thì thắng, đừng để nó đi tới vạch cuối cùng, đại khái là vậy";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, biNgo.transform);
        chat = "Nhào vô ăn tao";
        UIChatManager.Instance.SendChat(chat, biNgo.type.ToString(), ChatPosition.Right);
        chat = "CHiến thôi!";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), ChatPosition.Left, action.transform);
    }
}