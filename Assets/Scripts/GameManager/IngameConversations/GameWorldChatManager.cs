using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldChatManager : MonoBehaviour
{
    public static GameWorldChatManager Instance { get; private set; }
    
    [Header("Prefab")]
    public GameObject chatBoxPrefab;
    
    [Header("Canvas")]
    public Canvas worldSpaceCanvas;
    
    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float chatBoxAnimationDuration = 0.3f;
    public float chatBoxLifetime = 3f;
    
    private Dictionary<Transform, ChatBox> activeChatBoxes = new Dictionary<Transform, ChatBox>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SendChat(string message, Transform targetTransform, bool follow = true)
    {
        if (targetTransform == null) return;
        
        // Nếu transform đã có chatbox, thay thế message mới
        if (activeChatBoxes.ContainsKey(targetTransform))
        {
            ChatBox existingChatBox = activeChatBoxes[targetTransform];
            existingChatBox.UpdateMessage(message);
            return;
        }
        
        // Tạo chatbox mới
        GameObject chatBoxObj = Instantiate(chatBoxPrefab, targetTransform.position, Quaternion.identity);
        
        // Parent vào worldSpaceCanvas nếu có
        if (worldSpaceCanvas != null)
        {
            chatBoxObj.transform.SetParent(worldSpaceCanvas.transform, false);
        }
        
        ChatBox chatBox = chatBoxObj.GetComponent<ChatBox>();
        
        if (chatBox != null)
        {
            chatBox.Initialize(message, targetTransform, follow, chatBoxLifetime);
            activeChatBoxes[targetTransform] = chatBox;
            
            // Subscribe để remove khi chatbox bị destroy
            chatBox.OnDestroyed += () => RemoveChatBox(targetTransform);
        }
    }
    
    // Phương thức mới: SendChat với callback khi người chơi thực hiện hành động
    public void SendChatWithAction(string message, Transform targetTransform, System.Func<bool> actionCheck, System.Action onActionCompleted = null, bool follow = true)
    {
        if (targetTransform == null) return;
        
        // Nếu transform đã có chatbox, destroy cũ trước khi tạo mới
        if (activeChatBoxes.ContainsKey(targetTransform))
        {
            ChatBox existingChatBox = activeChatBoxes[targetTransform];
            if (existingChatBox != null)
            {
                Destroy(existingChatBox.gameObject);
            }
            activeChatBoxes.Remove(targetTransform);
        }
        
        // Tạo chatbox mới
        GameObject chatBoxObj = Instantiate(chatBoxPrefab, targetTransform.position, Quaternion.identity);
        
        // Parent vào worldSpaceCanvas nếu có
        if (worldSpaceCanvas != null)
        {
            chatBoxObj.transform.SetParent(worldSpaceCanvas.transform, false);
        }
        
        ChatBox chatBox = chatBoxObj.GetComponent<ChatBox>();
        
        if (chatBox != null)
        {
            // Sử dụng lifetime rất lớn để chat hiển thị liên tục cho đến khi action được thực hiện
            chatBox.Initialize(message, targetTransform, follow, 999f);
            activeChatBoxes[targetTransform] = chatBox;
            
            // Subscribe để remove khi chatbox bị destroy
            chatBox.OnDestroyed += () => RemoveChatBox(targetTransform);
            
            // Bắt đầu coroutine kiểm tra action
            StartCoroutine(CheckActionAndUpdateChat(chatBox, targetTransform, actionCheck, onActionCompleted));
        }
    }
    
    private IEnumerator CheckActionAndUpdateChat(ChatBox chatBox, Transform targetTransform, System.Func<bool> actionCheck, System.Action onActionCompleted)
    {
        while (chatBox != null && chatBox.gameObject.activeInHierarchy)
        {
            if (actionCheck != null && actionCheck())
            {
                // Action đã được thực hiện
                onActionCompleted?.Invoke();
                if (chatBox != null)
                {
                    // Remove khỏi dictionary trước khi destroy
                    if (targetTransform != null && activeChatBoxes.ContainsKey(targetTransform))
                    {
                        activeChatBoxes.Remove(targetTransform);
                    }
                    Destroy(chatBox.gameObject);
                }
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void RemoveChatBox(Transform targetTransform)
    {
        if (activeChatBoxes.ContainsKey(targetTransform))
        {
            activeChatBoxes.Remove(targetTransform);
        }
    }
    
    public void ClearAllChats()
    {
        foreach (var chatBox in activeChatBoxes.Values)
        {
            if (chatBox != null)
            {
                Destroy(chatBox.gameObject);
            }
        }
        activeChatBoxes.Clear();
    }
}
