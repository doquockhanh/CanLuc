using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameWorldOpenChat : MonoBehaviour
{
    public static GameWorldOpenChat Instance { get; private set; }

    [Header("Chat UI")]
    [SerializeField] private GameObject chatBoxPrefab;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float chatDuration = 3f;
    [SerializeField] private Vector3 offsetFromTransform = new Vector3(0, 2f, 0);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typingSound;

    private readonly Queue<ChatData> chatQueue = new Queue<ChatData>();
    private readonly List<GameObject> activeChatBoxes = new List<GameObject>();
    private bool isProcessingChat;
    private Camera currentCamera;

    #region Struct
    private struct ChatData
    {
        public readonly Transform target;
        public readonly string[] messages;

        public ChatData(Transform t, string message)
        {
            target = t;
            messages = new[] { message };
        }

        public ChatData(Transform t, string[] msgs)
        {
            target = t;
            messages = msgs;
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start() => InitializeComponents();

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearAllChats();
        UpdateCameraReference();
        EnsureCanvasExists();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        EnsureCanvasExists();
        EnsureAudioSource();
        UpdateCameraReference();
    }

    private void EnsureCanvasExists()
    {
        if (worldCanvas != null) return;

        worldCanvas = FindFirstObjectByType<Canvas>();
        if (worldCanvas == null) worldCanvas = CreateWorldCanvas();
    }

    private Canvas CreateWorldCanvas()
    {
        var canvasObj = new GameObject("GameWorldChatCanvas");
        canvasObj.transform.SetParent(transform);

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;

        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = Vector2.one;
        rect.localScale = Vector3.one;

        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 0.01f;

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        return canvas;
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null)
            audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void UpdateCameraReference()
    {
        currentCamera = Camera.main ?? FindFirstObjectByType<Camera>();
    }
    #endregion

    #region Public API
    /// <summary>
    /// Hiển thị ngay một câu thoại (có thể song song với chat khác).
    /// </summary>
    public void WriteChat(Transform target, string message)
    {
        if (target == null || string.IsNullOrEmpty(message)) return;
        StartCoroutine(ShowImmediateChat(new ChatData(target, message)));
    }

    /// <summary>
    /// Hiển thị nhiều câu thoại tuần tự (sử dụng queue).
    /// </summary>
    public void WriteChats(Transform target, string[] messages)
    {
        if (target == null || messages == null || messages.Length == 0) return;
        chatQueue.Enqueue(new ChatData(target, messages));
        if (!isProcessingChat) StartCoroutine(ProcessChatQueue());
    }

    public void ClearAllChats()
    {
        StopAllCoroutines();
        isProcessingChat = false;

        foreach (var chat in activeChatBoxes)
            if (chat != null) Destroy(chat);
        activeChatBoxes.Clear();

        chatQueue.Clear();
    }
    #endregion

    #region Queue Processing
    private IEnumerator ProcessChatQueue()
    {
        isProcessingChat = true;

        while (chatQueue.Count > 0)
        {
            var chatData = chatQueue.Dequeue();
            yield return ShowChats(chatData);
        }

        isProcessingChat = false;
    }
    #endregion

    #region Chat Display
    private IEnumerator ShowImmediateChat(ChatData chatData)
    {
        var chatBox = CreateChatBox(chatData.target);
        var textComponent = chatBox.GetComponentInChildren<TextMeshProUGUI>();

        yield return TypeText(textComponent, chatData.messages[0]);
        yield return new WaitForSeconds(chatDuration);

        activeChatBoxes.Remove(chatBox);
        Destroy(chatBox);
    }

    private IEnumerator ShowChats(ChatData chatData)
    {
        var chatBox = CreateChatBox(chatData.target);
        var textComponent = chatBox.GetComponentInChildren<TextMeshProUGUI>();

        foreach (var msg in chatData.messages)
        {
            yield return TypeText(textComponent, msg);
            if (msg != chatData.messages[^1]) yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(chatDuration);
        activeChatBoxes.Remove(chatBox);
        Destroy(chatBox);
    }

    private GameObject CreateChatBox(Transform target)
    {
        var chatBox = chatBoxPrefab != null
            ? Instantiate(chatBoxPrefab, worldCanvas.transform)
            : CreateDefaultChatBox();

        if (!chatBox.TryGetComponent<ChatBox>(out _))
            chatBox.AddComponent<ChatBox>();

        activeChatBoxes.Add(chatBox);
        SetChatBoxPosition(chatBox, target);
        return chatBox;
    }

    private GameObject CreateDefaultChatBox()
    {
        var chatBox = new GameObject("ChatBox", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        chatBox.transform.SetParent(worldCanvas.transform, false);

        // Background
        var bg = chatBox.GetComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        // RectTransform chat box (kích thước hợp lý trong world)
        var chatRect = chatBox.GetComponent<RectTransform>();
        chatRect.sizeDelta = new Vector2(10f, 2.4f);

        // Text
        var textObj = new GameObject("Text", typeof(TextMeshProUGUI));
        textObj.transform.SetParent(chatBox.transform, false);

        var text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 0.8f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        // RectTransform text
        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(0.2f, 0.2f);
        textRect.offsetMax = new Vector2(-0.2f, -0.2f);

        return chatBox;
    }

    private void SetChatBoxPosition(GameObject chatBox, Transform target)
    {
        if (chatBox == null || target == null) return;

        var rect = chatBox.GetComponent<RectTransform>();

        // lấy vị trí đỉnh sprite thay vì center
        Vector3 topPos = GetSpriteTopPosition(target);

        // cộng thêm offset tuỳ chỉnh (nếu muốn cao hơn 1 chút)
        rect.position = topPos + offsetFromTransform;
    }

    private Vector3 GetSpriteTopPosition(Transform target)
    {
        var spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Bounds bounds = spriteRenderer.bounds;
            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        // fallback: dùng transform.position nếu không có sprite
        return target.position;
    }
    #endregion

    #region Typing Effect
    private IEnumerator TypeText(TextMeshProUGUI textComponent, string fullText)
    {
        textComponent.text = "";
        PlayTypingSound();

        int index = 0;
        while (index < fullText.Length)
        {
            int charCount = Random.Range(1, 4);
            charCount = Mathf.Min(charCount, fullText.Length - index);
            textComponent.text += fullText.Substring(index, charCount);
            index += charCount;
            yield return new WaitForSeconds(typingSpeed);
        }

        StopTypingSound();
    }

    private void PlayTypingSound()
    {
        if (audioSource == null || typingSound == null) return;
        audioSource.clip = typingSound;
        audioSource.pitch = 2f;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopTypingSound()
    {
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
    }
    #endregion
}
