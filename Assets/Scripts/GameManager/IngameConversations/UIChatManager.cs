using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public enum ChatPosition
{
    Left,
    Middle,
    Right
}

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

    [Header("Camera Focus Settings")]
    public float focusDuration = 1f;
    public float focusZoom = 80f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip typingSound;

    [Header("Typing Settings")]
    public float punctuationDelay = 0.3f;

    [Header("Panel Position Settings")]
    public float panelStartY = -300f;
    public float panelEndY = 0f;
    public float panelSlideDistance = 20f;

    private Queue<ChatData> chatQueue = new Queue<ChatData>();
    private bool isDisplaying = false;
    private Coroutine currentTypingCoroutine;
    private CameraController cameraController;
    private Vector3 originalCameraPosition;
    private int originalCameraPPU;

    // Events
    public System.Action<string, string> OnChatCompleted;
    public System.Action OnAllChatsCompleted;

    private struct ChatData
    {
        public string message;
        public string characterName;
        public ChatPosition position;
        public Transform targetTransform;
        public float delay;
        public System.Action onComplete;
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

        // Tìm CameraController
        FindCameraController();

        // Setup AudioSource nếu chưa có
        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void FindCameraController()
    {
        cameraController = CameraController.Instance;
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }
    }

    public void SendChat(string message, string characterName, ChatPosition position = ChatPosition.Left, Transform targetTransform = null, float delay = 0f, System.Action onComplete = null)
    {
        ChatData newChat = new ChatData
        {
            message = message,
            characterName = characterName,
            position = position,
            targetTransform = targetTransform,
            delay = delay,
            onComplete = onComplete
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
        // Focus camera nếu có targetTransform
        if (chatData.targetTransform != null)
        {
            yield return StartCoroutine(FocusCameraOnTarget(chatData.targetTransform));
        }

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
        Vector3 panelTargetPos = GetPanelPosition(chatData.position);
        mainPanel.transform.DOLocalMoveX(panelTargetPos.x, imageMoveDuration).SetEase(Ease.OutQuad);

        // Di chuyển image frame
        Vector3 targetPosition = GetImageFramePosition(chatData.position);
        imageFrame.DOLocalMoveX(targetPosition.x, imageMoveDuration).SetEase(Ease.OutQuad);

        // Delay trước khi typing
        if (chatData.delay > 0f)
        {
            yield return new WaitForSeconds(chatData.delay);
        }

        // Typing animation
        if (currentTypingCoroutine != null)
            StopCoroutine(currentTypingCoroutine);

        currentTypingCoroutine = StartCoroutine(TypeText($"{chatData.characterName}: {chatData.message}"));
        yield return currentTypingCoroutine;

        // Chạy callback khi chat hoàn thành
        chatData.onComplete?.Invoke();

        // Fire event
        OnChatCompleted?.Invoke(chatData.characterName, chatData.message);

        // Đợi một chút trước khi chuyển message tiếp theo
        yield return new WaitForSeconds(1f);

        // Chỉ ẩn panel nếu không còn message trong queue
        if (chatQueue.Count == 0)
        {
            // Animation xuống dưới
            messageText.text = "";
            mainPanel.transform.DOMoveY(panelStartY, panelAnimationDuration).SetEase(Ease.InBack);
            mainPanel.transform.DOScale(Vector3.zero, panelAnimationDuration).SetEase(Ease.InBack)
                .OnComplete(() => mainPanel.SetActive(false));
            yield return new WaitForSeconds(panelAnimationDuration);

            // Fire event khi tất cả chat hoàn thành
            OnAllChatsCompleted?.Invoke();
        }
    }

    private IEnumerator TypeText(string text)
    {
        messageText.text = "";
        audioSource.clip = typingSound;
        audioSource.Stop();
        audioSource.pitch = 2f;
        audioSource.loop = true;
        audioSource.Play();

        // Normal typing: type từng ký tự
        for (int i = 0; i <= text.Length; i++)
        {
            messageText.text = text.Substring(0, i);

            if (i > 0 && i <= text.Length)
            {
                char lastChar = text[i - 1];
                if (i < text.Length && (lastChar == ',' || lastChar == '.' || lastChar == '!' || lastChar == '?'))
                {
                    audioSource.Stop();
                    yield return new WaitForSeconds(punctuationDelay);
                    audioSource.Play();
                }
            }

            yield return new WaitForSeconds(typingSpeed);
        }
        audioSource.Stop();
    }

    private void LoadCharacterSprite(string spriteName)
    {
        Sprite targetSprite = Resources.Load<Sprite>($"ProfileSprite/{spriteName}");

        if (targetSprite != null)
        {
            characterImage.sprite = targetSprite;
        }
        else
        {
            characterImage.sprite = defaultSprite;
        }
    }

    private IEnumerator FocusCameraOnTarget(Transform target)
    {
        if (cameraController == null)
        {
            FindCameraController();
        }

        if (cameraController == null) yield break;

        // Lưu vị trí camera ban đầu
        Camera worldCamera = Camera.main;
        if (worldCamera == null) yield break;

        originalCameraPosition = worldCamera.transform.position;

        // Lưu PPU ban đầu
        var pixelPerfectCamera = worldCamera.GetComponent<PixelPerfectCamera>();
        if (pixelPerfectCamera != null)
        {
            originalCameraPPU = pixelPerfectCamera.assetsPPU;
        }

        // Focus vào target
        Vector3 targetPosition = target.position;
        targetPosition.z = worldCamera.transform.position.z;

        // Di chuyển camera
        worldCamera.transform.DOMove(targetPosition, focusDuration).SetEase(Ease.OutQuad);

        // Zoom in
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.assetsPPU = (int)focusZoom;
        }

        yield return new WaitForSeconds(focusDuration);
    }
    private Vector3 GetPanelPosition(ChatPosition position)
    {
        switch (position)
        {
            case ChatPosition.Left:
                return Vector3.left * panelSlideDistance;
            case ChatPosition.Middle:
                return Vector3.zero;
            case ChatPosition.Right:
                return Vector3.right * panelSlideDistance;
            default:
                return Vector3.zero;
        }
    }

    private Vector3 GetImageFramePosition(ChatPosition position)
    {
        switch (position)
        {
            case ChatPosition.Left:
                return Vector3.left * 300f;
            case ChatPosition.Middle:
                return Vector3.zero;
            case ChatPosition.Right:
                return Vector3.right * 300f;
            default:
                return Vector3.zero;
        }
    }
}
