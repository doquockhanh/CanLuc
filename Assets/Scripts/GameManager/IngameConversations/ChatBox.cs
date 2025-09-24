using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ChatBox : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI messageText;
    public GameObject chatBoxPanel;
    
    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float animationDuration = 0.3f;
    public float offsetY = 2f;
    public float additionalDelay = 1f;
    
    private Transform targetTransform;
    private bool shouldFollow = false;
    private Coroutine typingCoroutine;
    private Coroutine lifetimeCoroutine;
    
    public System.Action OnDestroyed;
    
    void Start()
    {
        // Animation xuất hiện
        chatBoxPanel.transform.localScale = Vector3.zero;
        chatBoxPanel.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
    }
    
    void Update()
    {
        if (shouldFollow && targetTransform != null)
        {
            // Follow transform với offset dựa trên sprite height
            float spriteHeight = GetSpriteHeight(targetTransform);
            float totalOffset = spriteHeight + offsetY;
            Vector3 worldPos = targetTransform.position + Vector3.up * totalOffset;
            transform.position = worldPos;
        }
    }
    
    public void Initialize(string message, Transform target, bool follow, float lifetime)
    {
        targetTransform = target;
        shouldFollow = follow;
        
        // Tính offset dựa trên sprite height
        float spriteHeight = GetSpriteHeight(target);
        float totalOffset = spriteHeight + offsetY;
        
        // Đặt vị trí ban đầu
        if (!follow)
        {
            transform.position = target.position + Vector3.up * totalOffset;
        }
        
        // Bắt đầu typing animation
        StartTyping(message);
    }
    
    public void UpdateMessage(string newMessage)
    {
        // Dừng typing cũ
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Bắt đầu typing mới
        StartTyping(newMessage);
        
        // Reset lifetime dựa trên thời gian typing
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
        float typingTime = newMessage.Length * typingSpeed;
        lifetimeCoroutine = StartCoroutine(AutoDestroy(typingTime + additionalDelay));
    }
    
    private void StartTyping(string message)
    {
        typingCoroutine = StartCoroutine(TypeText(message));
    }
    
    private IEnumerator TypeText(string text)
    {
        messageText.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            messageText.text = text.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // Bắt đầu auto destroy sau khi typing xong
        float typingTime = text.Length * typingSpeed;
        lifetimeCoroutine = StartCoroutine(AutoDestroy(typingTime + additionalDelay));
    }
    
    private IEnumerator AutoDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyChatBox();
    }
    
    private void DestroyChatBox()
    {
        // Animation ẩn
        chatBoxPanel.transform.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack)
            .OnComplete(() => {
                OnDestroyed?.Invoke();
                Destroy(gameObject);
            });
    }
    
    private float GetSpriteHeight(Transform target)
    {
        // Tìm SpriteRenderer hoặc Image component
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            return spriteRenderer.bounds.size.y;
        }
        
        // Fallback: tìm trong children
        SpriteRenderer childSprite = target.GetComponentInChildren<SpriteRenderer>();
        if (childSprite != null && childSprite.sprite != null)
        {
            return childSprite.bounds.size.y;
        }
        
        // Default height nếu không tìm thấy sprite
        return 1f;
    }
    
    void OnDestroy()
    {
        // Cleanup coroutines
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (lifetimeCoroutine != null)
            StopCoroutine(lifetimeCoroutine);
    }
}