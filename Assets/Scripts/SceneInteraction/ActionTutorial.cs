using UnityEngine;
using System.Collections;

public class ActionTutorial : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public L1_1Conversation l1Conversation;

    private bool isTutorialActive = false;
    private bool hasPlayerClicked = false;
    private bool hasPlayerMoved = false;
    private bool hasPlayerRotated = false;
    private bool hasPlayerCharged = false;
    private bool hasPlayerStarted = false;

    private Coroutine tutorialCoroutine;
    private bool isWaitingForInput = false;
    private string currentWaitingFor = "";

    void Start()
    {
        if (GameWorldChatManager.Instance == null) return;
        if (l1Conversation == null) return;

        // Subscribe vào event của L1_1Conversation
        l1Conversation.OnConversationCompleted += StartTutorial;

        // Subscribe vào focus change event
        ActionBase.OnFocusChanged += OnFocusChanged;
        
        // Subscribe vào game phase change event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += OnGamePhaseChanged;
        }
        
        // Subscribe vào UIChatManager events để detect khi chat bị skip
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnAllChatsCompleted += OnAllChatsCompleted;
        }
    }

    void Update()
    {
        if (!isWaitingForInput || !isTutorialActive) return;
        
        // Kiểm tra nếu Action này đang được focus
        ActionBase actionBase = GetComponent<ActionBase>();
        if (actionBase == null || !actionBase.IsFocused) return;

        // Kiểm tra input dựa trên bước hiện tại
        switch (currentWaitingFor)
        {
            case "movement":
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    hasPlayerMoved = true;
                    isWaitingForInput = false;
                }
                break;

            case "rotation":
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.W))
                {
                    hasPlayerRotated = true;
                    isWaitingForInput = false;
                }
                break;

            case "charge":
                if (Input.GetKey(KeyCode.Space))
                {
                    hasPlayerCharged = true;
                    isWaitingForInput = false;
                }
                break;

            case "start":
                // Không cần kiểm tra input ở đây nữa, sẽ được xử lý trong OnGamePhaseChanged
                break;
        }
    }

    void StartTutorial()
    {
        if (isTutorialActive) return;
        isTutorialActive = true;
        
        // Focus camera vào Action này
        FocusCameraOnAction();
        
        tutorialCoroutine = StartCoroutine(TutorialSequence());
    }
    
    void FocusCameraOnAction()
    {
        // Tìm CameraController và focus vào Action này
        CameraController cameraController = CameraController.Instance;
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }
        
        if (cameraController != null)
        {
            // Di chuyển camera đến vị trí của Action này
            StartCoroutine(cameraController.FocusOnTarget(transform, 1f));
        }
    }

    IEnumerator TutorialSequence()
    {
        Debug.Log("Start Tutorial");

        // Bước 1: Yêu cầu click vào Action
        yield return StartCoroutine(WaitForClick());

        // Bước 2: Hướng dẫn di chuyển A/D
        yield return StartCoroutine(WaitForMovement());

        // Bước 3: Hướng dẫn xoay súng S/W
        yield return StartCoroutine(WaitForRotation());

        // Bước 4: Hướng dẫn tích lực Space
        yield return StartCoroutine(WaitForCharge());

        // Bước 5: Hướng dẫn bắt đầu
        yield return StartCoroutine(WaitForStart());

        // Kết thúc tutorial
        GameWorldChatManager.Instance.ClearAllChats();
        isTutorialActive = false;
    }

    IEnumerator WaitForClick()
    {
        GameWorldChatManager.Instance.SendChatWithAction("Hãy bấm vào tôi", transform,
            () => hasPlayerClicked,
            () => { });

        while (!hasPlayerClicked)
        {
            yield return null;
        }
    }

    IEnumerator WaitForMovement()
    {
        currentWaitingFor = "movement";
        isWaitingForInput = true;
        
        GameWorldChatManager.Instance.SendChatWithAction("A/D để di chuyển", transform,
            () => hasPlayerMoved,
            () => { });

        while (!hasPlayerMoved)
        {
            yield return null;
        }
        
        isWaitingForInput = false;
    }

    IEnumerator WaitForRotation()
    {
        currentWaitingFor = "rotation";
        isWaitingForInput = true;
        
        GameWorldChatManager.Instance.SendChatWithAction("S/W để xoay súng", transform,
            () => hasPlayerRotated,
            () => { });

        while (!hasPlayerRotated)
        {
            yield return null;
        }
        
        isWaitingForInput = false;
    }

    IEnumerator WaitForCharge()
    {
        currentWaitingFor = "charge";
        isWaitingForInput = true;
        
        GameWorldChatManager.Instance.SendChatWithAction("Giữ Space hoặc nút đỏ để tích lực", transform,
            () => hasPlayerCharged,
            () => { });

        while (!hasPlayerCharged)
        {
            yield return null;
        }
        
        isWaitingForInput = false;
    }

    IEnumerator WaitForStart()
    {
        currentWaitingFor = "start";
        isWaitingForInput = true;
        
        GameWorldChatManager.Instance.SendChatWithAction("Bấm bắt đầu để chạy", transform,
            () => hasPlayerStarted,
            () => { });

        while (!hasPlayerStarted)
        {
            yield return null;
        }
        
        isWaitingForInput = false;
    }

    void OnFocusChanged(ActionBase previous, ActionBase current)
    {
        ActionBase actionBase = GetComponent<ActionBase>();

        // Nếu Action này được focus
        if (current == actionBase && !hasPlayerClicked)
        {
            hasPlayerClicked = true;
        }
        // Nếu Action này bị unfocus và tutorial đang chạy
        else if (previous == actionBase && isTutorialActive && !IsTutorialCompleted())
        {
            // Reset trạng thái input khi bị unfocus
            isWaitingForInput = false;
            currentWaitingFor = "";
            
            ResetTutorialState();
            StartCoroutine(ResumeTutorialFromCurrentStep());
        }
    }
    
    void OnGamePhaseChanged(GamePhase newPhase)
    {
        // Kiểm tra nếu đang chờ bước "start" và game chuyển sang battle phase
        if (currentWaitingFor == "start" && newPhase == GamePhase.Battle)
        {
            // Chỉ cho phép hoàn thành nếu Action này đang được focus
            ActionBase actionBase = GetComponent<ActionBase>();
            if (actionBase != null && actionBase.IsFocused)
            {
                hasPlayerStarted = true;
                isWaitingForInput = false;
            }
        }
    }
    
    void OnAllChatsCompleted()
    {
        // Nếu tutorial chưa bắt đầu và conversation đã hoàn thành (có thể bị skip)
        if (!isTutorialActive && l1Conversation != null)
        {
            // Delay một chút để đảm bảo conversation state đã được cập nhật
            StartCoroutine(CheckAndStartTutorialAfterDelay());
        }
    }
    
    IEnumerator CheckAndStartTutorialAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Kiểm tra nếu conversation đã hoàn thành (bằng cách khác)
        if (!isTutorialActive)
        {
            StartTutorial();
        }
    }

    bool IsTutorialCompleted()
    {
        bool result = hasPlayerClicked && hasPlayerMoved && hasPlayerRotated && hasPlayerCharged && hasPlayerStarted;
        if (result){
            Destroy(this);
        }
        return result;
    }

    void ResetTutorialState()
    {
        hasPlayerClicked = false;
    }

    IEnumerator ResumeTutorialFromCurrentStep()
    {
        // Dừng coroutine cũ nếu có
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }

        // Chờ một chút để đảm bảo focus change hoàn tất
        yield return new WaitForSeconds(0.1f);

        // Bước 1: Yêu cầu click vào Action
        yield return StartCoroutine(WaitForClick());
        
        // Tiếp tục từ bước hiện tại dựa trên trạng thái đã hoàn thành
        if (!hasPlayerMoved)
        {
            yield return StartCoroutine(WaitForMovement());
        }
        if (!hasPlayerRotated)
        {
            yield return StartCoroutine(WaitForRotation());
        }
        if (!hasPlayerCharged)
        {
            yield return StartCoroutine(WaitForCharge());
        }
        if (!hasPlayerStarted)
        {
            yield return StartCoroutine(WaitForStart());
        }

        // Kết thúc tutorial
        GameWorldChatManager.Instance.ClearAllChats();
        isTutorialActive = false;
    }

    void OnDestroy()
    {
        // Unsubscribe khỏi event
        if (l1Conversation != null)
        {
            l1Conversation.OnConversationCompleted -= StartTutorial;
        }

        ActionBase.OnFocusChanged -= OnFocusChanged;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= OnGamePhaseChanged;
        }
        
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnAllChatsCompleted -= OnAllChatsCompleted;
        }

        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }

        if (GameWorldChatManager.Instance != null)
        {
            GameWorldChatManager.Instance.ClearAllChats();
        }
    }
}
