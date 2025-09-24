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
