using UnityEngine;

public class L1_1Conversation : MonoBehaviour
{
    [Header("Conversation")]
    public ActionBase action;

    void Start()
    {
        string chat = "Hello";
        UIChatManager.Instance.SendChat(chat, action.type.ToString(), action.transform);
    }
}