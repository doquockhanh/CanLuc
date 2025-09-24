using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UIChatManager : MonoBehaviour
{
    public static UIChatManager Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject mainPanel;
    public TextMeshProUGUI messageText;
    public Image characterImage;
    public Transform imageFrame;
    
    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float panelAnimationDuration = 0.3f;
    public float imageMoveDuration = 0.2f;
    public Sprite defaultSprite;
    
    [Header("Panel Position Settings")]
    public float panelStartY = -300f;
    public float panelEndY = 0f;
    public float panelSlideDistance = 20f;
    
    private Queue<ChatData> chatQueue = new Queue<ChatData>();
    private bool isDisplaying = false;
    private Coroutine currentTypingCoroutine;
    
    private struct ChatData
    {
        public string message;
        public string characterName;
        public bool isLeft;
        public Transform targetTransform;
    }
    
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
    
    void Start()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);
    }
    
    public void SendChat(string message, string characterName, bool left = true, Transform targetTransform = null)
    {
        ChatData newChat = new ChatData
        {
            message = message,
            characterName = characterName,
            isLeft = left,
            targetTransform = targetTransform
        };
        
        chatQueue.Enqueue(newChat);
        
        if (!isDisplaying)
        {
            StartCoroutine(ProcessChatQueue());
        }
    }
    
    private IEnumerator ProcessChatQueue()
    {
        isDisplaying = true;
        
        while (chatQueue.Count > 0)
        {
            ChatData chatData = chatQueue.Dequeue();
            yield return StartCoroutine(DisplayChat(chatData));
        }
        
        isDisplaying = false;
    }
    
    private IEnumerator DisplayChat(ChatData chatData)
    {
        // Chỉ hiển thị panel nếu chưa hiển thị
        if (!mainPanel.activeInHierarchy)
        {
            mainPanel.SetActive(true);
            // Animation từ dưới lên
            mainPanel.transform.localPosition = new Vector3(0, panelStartY, 0);
            mainPanel.transform.localScale = Vector3.zero;
            mainPanel.transform.DOMoveY(panelEndY, panelAnimationDuration).SetEase(Ease.OutBack);
            mainPanel.transform.DOScale(Vector3.one, panelAnimationDuration).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(panelAnimationDuration);
        }
        
        // Cập nhật text
        messageText.text = chatData.characterName;
        
        // Load sprite
        LoadCharacterSprite(chatData.characterName);
        
        // Hiệu ứng chuyển message cho panel
        Vector3 panelTargetPos = chatData.isLeft ? Vector3.left * panelSlideDistance : Vector3.right * panelSlideDistance;
        mainPanel.transform.DOLocalMoveX(panelTargetPos.x, imageMoveDuration).SetEase(Ease.OutQuad);
        
        // Di chuyển image frame
        Vector3 targetPosition = chatData.isLeft ? Vector3.left * 300f : Vector3.right * 300f;
        imageFrame.DOLocalMoveX(targetPosition.x, imageMoveDuration).SetEase(Ease.OutQuad);
        
        // Typing animation
        if (currentTypingCoroutine != null)
            StopCoroutine(currentTypingCoroutine);
        
        currentTypingCoroutine = StartCoroutine(TypeText($"{chatData.characterName}: {chatData.message}"));
        yield return currentTypingCoroutine;
        
        // Đợi một chút trước khi chuyển message tiếp theo
        yield return new WaitForSeconds(1f);
        
        // Chỉ ẩn panel nếu không còn message trong queue
        if (chatQueue.Count == 0)
        {
            // Animation xuống dưới
            mainPanel.transform.DOMoveY(panelStartY, panelAnimationDuration).SetEase(Ease.InBack);
            mainPanel.transform.DOScale(Vector3.zero, panelAnimationDuration).SetEase(Ease.InBack)
                .OnComplete(() => mainPanel.SetActive(false));
            yield return new WaitForSeconds(panelAnimationDuration);
        }
    }
    
    private IEnumerator TypeText(string text)
    {
        messageText.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            messageText.text = text.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    private void LoadCharacterSprite(string spriteName)
    {
        Sprite targetSprite = Resources.Load<Sprite>($"Characters/{spriteName}");
        
        if (targetSprite != null)
        {
            characterImage.sprite = targetSprite;
        }
        else
        {
            characterImage.sprite = defaultSprite;
        }
    }
}
